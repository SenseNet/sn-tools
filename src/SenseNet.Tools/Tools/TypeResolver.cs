using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using SenseNet.Diagnostics;

// ReSharper disable once CheckNamespace
namespace SenseNet.Tools
{
    /// <summary>
    /// Provides methods for loading available types in the system.
    /// </summary>
    public static class TypeResolver
    {
        private static readonly object TypeCacheSync = new object();
        private static readonly Dictionary<string, Type> TypeCacheByName = new Dictionary<string, Type>();
        private static readonly Dictionary<Type, Type[]> TypeCacheByBase = new Dictionary<Type, Type[]>();

        /// <summary>
        /// Creates an instance of the specified type.
        /// </summary>
        /// <typeparam name="T">Type of the object to create.</typeparam>
        /// <param name="typeName">Name of the type to use.</param>
        /// <returns>A newly created object of type T.</returns>
        // ReSharper disable once UnusedMember.Global
        public static T CreateInstance<T>(string typeName) where T : new()
        {
            return (T)CreateInstance(typeName);
        }
        /// <summary>
        /// Creates an instance of the specified type.
        /// </summary>
        /// <typeparam name="T">Type of the object to create.</typeparam>
        /// <param name="typeName">Name of the type to use.</param>
        /// <param name="args">An array of arguments that match in number, order, and type the parameters 
        /// of the constructor to invoke.</param>
        /// <returns>A newly created object of type T.</returns>
        // ReSharper disable once UnusedMember.Global
        public static T CreateInstance<T>(string typeName, params object[] args)
        {
            return (T)CreateInstance(typeName, args);
        }
        /// <summary>
        /// Creates an instance of the specified type.
        /// </summary>
        /// <param name="typeName">Name of the type to use.</param>
        /// <returns>A newly created object of the provided type.</returns>
        public static object CreateInstance(string typeName)
        {
            var t = GetType(typeName);

            return Activator.CreateInstance(t, true);
        }
        /// <summary>
        /// Creates an instance of the specified type.
        /// </summary>
        /// <param name="typeName">Name of the type to use.</param>
        /// <param name="args">An array of arguments that match in number, order, and type the parameters 
        /// of the constructor to invoke.</param>
        /// <returns>A newly created object of the provided type.</returns>
        public static object CreateInstance(string typeName, params object[] args)
        {
            var t = GetType(typeName);

            return Activator.CreateInstance(t, args);
        }

        /// <summary>
        /// Loads the type with the specified name. First looks for the type using
        /// the built-in Type.GetType method, than (in case it is not found) uses 
        /// the type cache and an app domain search as a fallback.
        /// </summary>
        /// <param name="typeName">Name of the type to load.</param>
        /// <param name="throwOnError">Whether to throw an error when a type is not found.</param>
        public static Type GetType(string typeName, bool throwOnError = true)
        {
            EnsurePlugins();

            //assume its an assembly qualified type name
            var t = Type.GetType(typeName, false);
            //if fusion loader fails let's find the type in what we have
            return t ?? FindTypeInAppDomain(typeName, throwOnError);
        }

        /// <summary>
        /// Loads the type with the specified name from the type cache or from the current app domain.
        /// </summary>
        /// <param name="typeName">Name of the type to load.</param>
        /// <param name="throwOnError">Whether to throw an error when a type is not found.</param>
        public static Type FindTypeInAppDomain(string typeName, bool throwOnError = true)
        {
            if (!TypeCacheByName.TryGetValue(typeName, out var type))
            {
                lock (TypeCacheSync)
                {
                    if (!TypeCacheByName.TryGetValue(typeName, out type))
                    {
                        foreach (var assembly in GetAssemblies())
                        {
                            try
                            {
                                type = assembly.GetType(typeName);
                                if (type != null)
                                    break;
                            }
                            catch (Exception e)
                            {
                                if (!IgnorableException(e))
                                    throw;
                            }
                        }
                        if (type == null)
                        {
                            var split = typeName.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            var tName = split[0];
                            var asmName = split.Length > 1 ? split[1].ToLower(CultureInfo.InvariantCulture).Trim() : null;
                            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                            {
                                if (asmName != null && asmName != assembly.GetName().Name.ToLower(CultureInfo.InvariantCulture))
                                    continue;
                                try
                                {
                                    type = assembly.GetType(tName);
                                    if (type != null)
                                        break;
                                }
                                catch (Exception e)
                                {
                                    if (!IgnorableException(e))
                                        throw;
                                }
                            }
                        }

                        //
                        //  Important: leave this comment here
                        //  There was an error adding NULL type to _typeCacheByName dictionary, after restarting the AddDomain with iisReset. 
                        //  It is fixed by BuildManager.GetReferencedAssemblies() call when Application_OnStart event occurs.
                        //

                        if (!TypeCacheByName.ContainsKey(typeName))
                            TypeCacheByName.Add(typeName, type);
                    }
                }
            }
            if (throwOnError && type == null)
                throw new TypeNotFoundException(typeName);

            return type;
        }

        /// <summary>
        /// Loads all assemblies in the current app domain.
        /// </summary>
        public static Assembly[] GetAssemblies()
        {
            EnsurePlugins();

            return AppDomain.CurrentDomain.GetAssemblies();
        }

        private static bool _pluginsLoaded;

        /// <summary>
        /// Loads all assemblies from the specified file system path.
        /// </summary>
        /// <param name="path">A file system path where assemblies should be loaded from.</param>
        /// <returns>An array of file names of loaded assemblies in the specified folder.</returns>
        public static string[] LoadAssembliesFrom(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (path.Length == 0)
                throw new ArgumentException("Path cannot be empty.", nameof(path));

            var loaded = new List<string>();
            var badImageFormatMessages = new List<string>();
            using (var op = SnTrace.Repository.StartOperation("Loading assemblies from: " + path))
            {
                _pluginsLoaded = true;

                var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .GroupBy(a => new AssemblyName(a.FullName).Name)
                    .ToDictionary(grp => grp.Key, grp => grp.Last());

                var dllPaths = Directory.GetFiles(path, "*.dll");

                foreach (var dllPath in dllPaths)
                {
                    try
                    {
                        var asmName = AssemblyName.GetAssemblyName(dllPath);
                        var asmFullName = asmName.FullName;
                        var asmNameName = asmName.Name;
                        if (!assemblies.TryGetValue(asmNameName, out var origAsm))
                        {
                            var loadedAsm = Assembly.LoadFrom(dllPath);
                            assemblies.Add(asmNameName, loadedAsm);
                            loaded.Add(Path.GetFileName(dllPath));
                        }
                    }
                    catch (BadImageFormatException e) //logged
                    {
                        badImageFormatMessages.Add(e.Message);
                    }
                }

                op.Successful = true;
            }
            if (badImageFormatMessages.Count > 0)
                SnLog.WriteInformation(
                    $"Skipped assemblies from {path} on start: {Environment.NewLine}{string.Join(Environment.NewLine, badImageFormatMessages)}");
            return loaded.ToArray();
        }

        private static readonly object PluginSync = new object();

        private static void EnsurePlugins()
        {
            if (_pluginsLoaded)
                return;

            lock (PluginSync)
            {
                if (_pluginsLoaded)
                    return;

                LoadAssembliesFrom(AppDomain.CurrentDomain.BaseDirectory);
            }
        }

        /// <summary>
        /// Loads all types that implement the specified interface.
        /// </summary>
        /// <param name="interfaceType">Interface type to look for.</param>
        public static Type[] GetTypesByInterface(Type interfaceType)
        {
            if (!TypeCacheByBase.TryGetValue(interfaceType, out var temp))
            {
                lock (TypeCacheSync)
                {
                    if (!TypeCacheByBase.TryGetValue(interfaceType, out temp))
                    {
                        var list = new List<Type>();
                        foreach (var asm in GetAssemblies())
                        {
                            try
                            {
                                if (asm.IsDynamic)
                                    continue;

                                // Add all types to the list that have the specified
                                // interface type among their implemented interfaces.
                                list.AddRange(asm.GetTypes().Where(type =>
                                    type.GetInterfaces().Any(@interface => @interface == interfaceType)));
                            }
                            catch (ReflectionTypeLoadException e)
                            {
                                if (!IgnorableException(e))
                                {
                                    LogTypeLoadException(e, asm.FullName);
                                    throw;
                                }
                            }
                            catch (Exception e)
                            {
                                if (!IgnorableException(e))
                                    throw TypeDiscoveryError(e, null, asm);
                            }
                        }
                        temp = list.ToArray();

                        if (!TypeCacheByBase.ContainsKey(interfaceType))
                            TypeCacheByBase.Add(interfaceType, temp);
                    }
                }
            }
            var result = new Type[temp.Length];
            temp.CopyTo(result, 0);
            return result;
        }
        /// <summary>
        /// Loads all types that inherit from the specified base type.
        /// </summary>
        /// <param name="baseType">Base type to look for.</param>
        public static Type[] GetTypesByBaseType(Type baseType)
        {
            if (!TypeCacheByBase.TryGetValue(baseType, out var temp))
            {
                lock (TypeCacheSync)
                {
                    if (!TypeCacheByBase.TryGetValue(baseType, out temp))
                    {
                        var list = new List<Type>();
                        foreach (var asm in GetAssemblies())
                        {
                            try
                            {
                                if (asm.IsDynamic)
                                    continue;

                                var types = asm.GetTypes();
                                foreach (var type in types)
                                {
                                    try
                                    {
                                        if (type.IsSubclassOf(baseType))
                                            list.Add(type);
                                    }
                                    catch (Exception e)
                                    {
                                        if (!IgnorableException(e))
                                            throw TypeDiscoveryError(e, type.FullName, asm);
                                    }
                                }
                            }
                            catch (ReflectionTypeLoadException e)
                            {
                                if (!IgnorableException(e))
                                {
                                    LogTypeLoadException(e, asm.FullName);
                                    throw;
                                }
                            }
                            catch (Exception e)
                            {
                                if (!IgnorableException(e))
                                    throw TypeDiscoveryError(e, null, asm);
                            }
                        }
                        temp = list.ToArray();

                        if (!TypeCacheByBase.ContainsKey(baseType))
                            TypeCacheByBase.Add(baseType, temp);
                    }
                }
            }

            var result = new Type[temp.Length];
            temp.CopyTo(result, 0);
            return result;
        }

        private static void LogTypeLoadException(ReflectionTypeLoadException ex, string assemblyName = null)
        {
            SnLog.WriteError(ex.ToString(), properties: new Dictionary<string, object> { { "Assembly", assemblyName ?? "unknown" } });

            foreach (var exc in ex.LoaderExceptions)
            {
                SnLog.WriteError(exc);
            }
        }
        private static bool IgnorableException(Exception e)
        {
            if (!Debugger.IsAttached)
                return false;
            var rte = e as ReflectionTypeLoadException;
            if (rte?.LoaderExceptions.Length == 2)
            {
                if (rte.LoaderExceptions[0] is TypeLoadException te0 && rte.LoaderExceptions[1] is TypeLoadException te1)
                {
                    if (te0.TypeName == "System.Web.Mvc.CompareAttribute" && te1.TypeName == "System.Web.Mvc.RemoteAttribute")
                        return true;
                }
            }
            return false;
        }
        private static Exception TypeDiscoveryError(Exception innerEx, string typeName, Assembly asm)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var duplicates = assemblies.GroupBy(f => f.ToString()).Where(g => g.Count() > 1).ToArray();

            var msg = new StringBuilder();
            msg.Append("Type discovery error. Assembly: ").Append(asm);
            if (typeName != null)
                msg.Append(", type: ").Append(typeName).Append(".");
            if (duplicates.Any())
            {
                msg.AppendLine().AppendLine("DUPLICATED ASSEMBLIES:");
                var count = 0;
                foreach (var item in duplicates)
                    msg.Append("    #").Append(count++).Append(": ").AppendLine(item.Key);
            }
            return new ApplicationException(msg.ToString(), innerEx);
        }
    }
}