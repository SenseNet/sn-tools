using System;

namespace SenseNet.Tools
{
    /// <summary>
    /// Represents a type loading error.
    /// </summary>
    [Serializable]
    public class TypeNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the TypeNotFoundException class.
        /// </summary>
        public TypeNotFoundException() { }
        /// <summary>
        /// Initializes a new instance of the TypeNotFoundException class.
        /// </summary>
        /// <param name="typeName">Name of the type that was not found.</param>
        public TypeNotFoundException(string typeName) : base("Type was not found: " + typeName) { }
        /// <summary>
        /// Initializes a new instance of the TypeNotFoundException class.
        /// </summary>
        /// <param name="typeName">Name of the type that was not found.</param>
        /// <param name="inner">Original exception.</param>
        public TypeNotFoundException(string typeName, Exception inner) : base("Type was not found: " + typeName, inner) { }

        /// <summary>
        /// Initializes a new instance of the TypeNotFoundException class with serialized data.
        /// </summary>
        protected TypeNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        { }
    }
}
