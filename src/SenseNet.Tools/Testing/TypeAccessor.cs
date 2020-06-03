using System;
using System.Linq;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace SenseNet.Testing
{
    /// <summary>
    /// Exposes accessor methods for non-visible static members of a target type.
    /// </summary>
    public class TypeAccessor
    {
        private BindingFlags _publicFlags = BindingFlags.Static | BindingFlags.Public;
        private BindingFlags _privateFlags = BindingFlags.Static | BindingFlags.NonPublic;

        /// <summary>
        /// Gets the target type.
        /// </summary>
        public Type TargetType { get; }

        /// <summary>
        /// Initializes a <see cref="TypeAccessor"/> instance that wraps given target type.
        /// </summary>
        /// <param name="target">Type to wrap.</param>
        public TypeAccessor(Type target)
        {
            TargetType = target;
        }
        /// <summary>
        /// Initializes a <see cref="TypeAccessor"/> instance containing the given target type.
        /// </summary>
        /// <param name="assemblyName">Assembly name.</param>
        /// <param name="typeName">Fully qualified name of the desired target type.</param>
        public TypeAccessor(string assemblyName, string typeName)
        {
            TargetType = GetTypeByName(assemblyName, typeName);
        }
        internal static Type GetTypeByName(string assemblyName, string typeName)
        {
            var asm = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == assemblyName);
            if (asm == null)
                throw new TypeLoadException("Unknown assembly: " + assemblyName);

            var tName = typeName;
            var p = tName.IndexOf("[", StringComparison.Ordinal);
            if (p > 0)
                tName = tName.Substring(0, p);

            var type = asm.GetType(tName, false, false);
            if (type == null)
                throw new TypeLoadException("Unknown type: " + typeName);

            return type;
        }

        /// <summary>Gets a static field's value.</summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>The value of the static field.</returns>
        public object GetStaticField(string fieldName)
        {
            var field = GetField(fieldName);
            return field.GetValue(null);
        }
        /// <summary>Sets a static field's value.</summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="value">Value to set.</param>
        public void SetStaticField(string fieldName, object value)
        {
            var field = GetField(fieldName);
            field.SetValue(null, value);
        }
        private FieldInfo GetField(string name, bool throwOnError = true)
        {
            var field = TargetType.GetField(name, _publicFlags) ?? TargetType.GetField(name, _privateFlags);
            if (field == null && throwOnError)
                throw new ApplicationException("Field not found: " + name);
            return field;
        }

        /// <summary>Gets a static property's value.</summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The value of the static property.</returns>
        public object GetStaticProperty(string propertyName)
        {
            var property = GetProperty(propertyName);
            var method = property.GetGetMethod(true) ?? property.GetGetMethod(false);
            return method.Invoke(null, null);
        }
        /// <summary>Sets a static property's value.</summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">Value to set.</param>
        public void SetStaticProperty(string propertyName, object value)
        {
            var property = GetProperty(propertyName);
            var method = property.GetSetMethod(true) ?? property.GetSetMethod(false);
            method.Invoke(null, new [] { value });
        }
        private PropertyInfo GetProperty(string name, bool throwOnError = true)
        {
            var property = TargetType.GetProperty(name, _publicFlags) ?? TargetType.GetProperty(name, _privateFlags);
            if (property == null && throwOnError)
                throw new ApplicationException("Property not found: " + name);
            return property;
        }

        /// <summary>Gets a static field's or property's value.</summary>
        /// <param name="memberName">Name of the field or property</param>
        /// <returns>The value of the static field or property.</returns>
        public object GetStaticFieldOrProperty(string memberName)
        {
            var field = GetField(memberName, false);
            if (field != null)
                return field.GetValue(null);

            var property = GetProperty(memberName, false);
            if (property == null)
                throw new ApplicationException("Field or property not found: " + memberName);

            var method = property.GetGetMethod(true) ?? property.GetGetMethod(false);
            if (method == null)
                throw new ApplicationException("The property does not have getter: " + memberName);

            return method.Invoke(null, null);
        }
        /// <summary>Sets a static field's or property's value.</summary>
        /// <param name="memberName">Name of the field or property</param>
        /// <param name="value">Value to set.</param>
        public void SetStaticFieldOrProperty(string memberName, object value)
        {
            var field = GetField(memberName, false);
            if (field != null)
            {
                field.SetValue(null, value);
                return;
            }

            var property = GetProperty(memberName, false);
            if (property == null)
                throw new ApplicationException("Field or property not found: " + memberName);

            var method = property.GetSetMethod(true) ?? property.GetSetMethod(false);
            if (method == null)
                throw new ApplicationException("The property does not have setter: " + memberName);

            method.Invoke(null, new [] { value });
        }

        /// <summary>Invokes a static member.</summary>
        /// <param name="name">Name of the member.</param>
        /// <param name="args">Arguments to the invocation.</param>
        /// <returns>Result of invocation.</returns>
        public object InvokeStatic(string name, params object[] args)
        {
            var paramTypes = args.Select(x => x.GetType()).ToArray();
            return InvokeStatic(name, paramTypes, args);
        }
        /// <summary>Invokes a static member.</summary>
        /// <param name="name">Name of the member.</param>
        /// <param name="parameterTypes">An array of <see cref="T:System.Type" /> objects representing the number,
        /// order, and type of the parameters for the method to invoke.</param>
        /// <param name="args">Arguments to the invocation.</param>
        /// <returns>Result of invocation</returns>
        public object InvokeStatic(string name, Type[] parameterTypes, object[] args)
        {
            var method = TargetType.GetMethod(name, _privateFlags, null, parameterTypes, null)
                ?? TargetType.GetMethod(name, _publicFlags, null, parameterTypes, null);
            if (method == null)
                throw new ApplicationException("Method not found: " + name);
            return method.Invoke(null, args);
        }

    }
}
