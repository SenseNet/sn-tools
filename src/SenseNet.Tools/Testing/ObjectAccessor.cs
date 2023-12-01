using System;
using System.Linq;
using System.Reflection;
// ReSharper disable UnusedParameter.Local

// ReSharper disable once CheckNamespace
namespace SenseNet.Testing
{
    /// <summary>
    /// Exposes accessor methods for non-visible members of a target object.
    /// </summary>
    public class ObjectAccessor
    {
        private Type _targetType;
        private BindingFlags _publicFlags = BindingFlags.Instance | BindingFlags.Public;
        private BindingFlags _privateFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        /// <summary>
        /// Gets the wrapped object.
        /// </summary>
        public object Target { get; }

        /// <summary>
        /// Initializes a <see cref="ObjectAccessor"/> instance that wraps the given target object.
        /// </summary>
        /// <param name="target">Object to wrap.</param>
        public ObjectAccessor(object target)
        {
            Target = target;
            _targetType = Target.GetType();
        }
        /// <summary>
        /// Initializes a <see cref="ObjectAccessor"/> instance that wraps the given target object.
        /// Target object can be accessed by the given <paramref name="baseType"/> type.
        /// Typical usage: access a field that defined on the abstract ancestor.
        /// </summary>
        /// <param name="target">Object to wrap.</param>
        /// <param name="baseType">Accessor abstraction.</param>
        public ObjectAccessor(object target, Type baseType)
        {
            Target = target;
            _targetType = baseType;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectAccessor" /> class that wraps the
        /// newly created instance of the specified type.
        /// </summary>
        /// <param name="type">Type of the object to create.</param>
        /// <param name="arguments">Arguments to pass to the constructor.</param>
        public ObjectAccessor(Type type, params object[] arguments)
        {
            _targetType = type;
            var ctor = GetConstructorByParams(type, arguments);
            Target = ctor.Invoke(arguments);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectAccessor" /> class that wraps the
        /// newly created instance of the specified type.
        /// </summary>
        /// <param name="type">Type of the object to create.</param>
        /// <param name="parameterTypes">An array of Type objects representing the number, order,
        /// and type of the parameters for the constructor to get.</param>
        /// <param name="arguments">Arguments to pass to the constructor.</param>
        public ObjectAccessor(Type type, Type[] parameterTypes, object[] arguments)
        {
            _targetType = type;
            var ctor = GetConstructorByTypes(type, parameterTypes);
            Target = ctor.Invoke(arguments);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectAccessor" /> class that wraps the
        /// newly created instance of the specified type.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly that contains the type.</param>
        /// <param name="typeName">Fully qualified name of the type.</param>
        /// <param name="arguments">Arguments to pass to the constructor.</param>
        public ObjectAccessor(string assemblyName, string typeName, params object[] arguments)
        {
            var type = TypeAccessor.GetTypeByName(assemblyName, typeName);
            _targetType = type;
            var ctor = GetConstructorByParams(type, arguments);
            Target = ctor.Invoke(arguments);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectAccessor" /> class that wraps the
        /// newly created instance of the specified type.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly that contains the type.</param>
        /// <param name="typeName">Fully qualified name of the type.</param>
        /// <param name="parameterTypes">An array of Type objects representing the number, order,
        /// and type of the parameters for the constructor to get.</param>
        /// <param name="arguments">Arguments to pass to the constructor.</param>
        public ObjectAccessor(string assemblyName, string typeName, Type[] parameterTypes, params object[] arguments)
        {
            var type = TypeAccessor.GetTypeByName(assemblyName, typeName);
            _targetType = type;
            var ctor = GetConstructorByTypes(type, parameterTypes);
            Target = ctor.Invoke(arguments);
        }
        /// <summary>
        /// NOT IMPLEMENTED YET.
        /// </summary>
        public ObjectAccessor(object target, string memberToAccess)
        {
            throw new NotImplementedException();
        }

        private ConstructorInfo GetConstructorByParams(Type type, object[] arguments)
        {
            var argTypes = arguments.Select(a => a.GetType()).ToArray();
            return GetConstructorByTypes(type, argTypes);
        }
        private ConstructorInfo GetConstructorByTypes(Type type, Type[] argTypes)
        {
            var ctor = type.GetConstructor(_privateFlags, null, argTypes, null);
            if (ctor == null)
            {
                ctor = type.GetConstructor(_publicFlags, null, argTypes, null);
                if (ctor == null)
                    throw new ApplicationException("Constructor not found.");
            }
            return ctor;
        }

        /// <summary>Gets a field's value.</summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>Value of the field.</returns>
        public object GetField(string fieldName)
        {
            var field = GetFieldInfo(fieldName);
            return field.GetValue(Target);
        }
        /// <summary>Sets a field's value.</summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="value">Value to set.</param>
        public void SetField(string fieldName, object value)
        {
            var field = GetFieldInfo(fieldName);
            field.SetValue(Target, value);
        }
        private FieldInfo GetFieldInfo(string name, bool throwOnError = true)
        {
            var field = _targetType.GetField(name, BindingFlags.GetField | _publicFlags) ??
                        _targetType.GetField(name, BindingFlags.GetField | _privateFlags);
            if (field == null && throwOnError)
                throw new ApplicationException("Field not found: " + name);
            return field;
        }

        /// <summary>Gets a property's value.</summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>Value of the property.</returns>
        public object GetProperty(string propertyName)
        {
            var property = GetPropertyInfo(propertyName);
            var method = property.GetGetMethod(true) ?? property.GetGetMethod(false);
            return method.Invoke(Target, null);
        }
        /// <summary>Sets a property's value.</summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">Value to set.</param>
        public void SetProperty(string propertyName, object value)
        {
            var property = GetPropertyInfo(propertyName);
            var method = property.GetSetMethod(true) ?? property.GetSetMethod(false);
            method.Invoke(Target, new [] { value });
        }
        private PropertyInfo GetPropertyInfo(string name, bool throwOnError = true)
        {
            var property = _targetType.GetProperty(name, _publicFlags) ?? _targetType.GetProperty(name, _privateFlags);
            if (property == null && throwOnError)
                throw new ApplicationException("Property not found: " + name);
            return property;
        }

        /// <summary>Gets a field's or property's value.</summary>
        /// <param name="memberName">Name of the field or property.</param>
        /// <returns>Value of the field or property.</returns>
        public object GetFieldOrProperty(string memberName)
        {
            var field = GetFieldInfo(memberName, false);
            if (field != null)
                return field.GetValue(Target);

            var property = GetPropertyInfo(memberName, false);
            if (property == null)
                throw new ApplicationException("Field or property not found: " + memberName);

            var method = property.GetGetMethod(true) ?? property.GetGetMethod(false);
            if (method == null)
                throw new ApplicationException("The property does not have getter: " + memberName);

            return method.Invoke(Target, null);
        }
        /// <summary>Sets a field's or property's value.</summary>
        /// <param name="memberName">Name of the field or property.</param>
        /// <param name="value">Value to set.</param>
        public void SetFieldOrProperty(string memberName, object value)
        {
            var field = GetFieldInfo(memberName, false);
            if (field != null)
            {
                field.SetValue(Target, value);
                return;
            }

            var property = GetPropertyInfo(memberName, false);
            if (property == null)
                throw new ApplicationException("Field or property not found: " + memberName);

            var method = property.GetSetMethod(true) ?? property.GetSetMethod(false);
            if (method == null)
                throw new ApplicationException("The property does not have setter: " + memberName);

            method.Invoke(Target, new [] { value });
        }

        /// <summary>Invokes a member.</summary>
        /// <param name="name">Name of the member.</param>
        /// <param name="args">Arguments to the invocation.</param>
        /// <returns>Result of invocation.</returns>
        public object Invoke(string name, params object[] args)
        {
            var paramTypes = args.Select(x => x.GetType()).ToArray();
            return Invoke(name, paramTypes, args);
        }
        /// <summary>Invokes a static member.</summary>
        /// <param name="name">Name of the member.</param>
        /// <param name="parameterTypes">An array of <see cref="T:System.Type" /> objects representing the number,
        /// order, and type of the parameters for the method to invoke.</param>
        /// <param name="args">Arguments to the invocation.</param>
        /// <returns>Result of invocation</returns>
        public object Invoke(string name, Type[] parameterTypes, object[] args)
        {
            var method = _targetType.GetMethod(name, _privateFlags, null, parameterTypes, null)
                ?? _targetType.GetMethod(name, _publicFlags, null, parameterTypes, null);
            if (method == null)
                throw new ApplicationException("Method not found: " + name);
            return method.Invoke(Target, args);
        }
        /// <summary>
        /// NOT IMPLEMENTED YET.
        /// </summary>
        public object Invoke(string name, Type[] parameterTypes, object[] args, Type[] typeArguments)
        {
            throw new NotImplementedException();
        }
    }
}
