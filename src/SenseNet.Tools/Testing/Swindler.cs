using System;
using System.Collections.Generic;
using System.Text;

// ReSharper disable once CheckNamespace
namespace SenseNet.Testing
{
    /// <summary>
    /// Represents an object that changes a value and holds it in its lifecycle and
    /// restores the original value when it is destroyed.
    /// </summary>
    public class Swindler<T> : IDisposable
    {
        private readonly T _original;
        private readonly Action<T> _setter;
        /// <summary>
        /// Initializes a new instance of the <see cref="Swindler{T}"/>.
        /// </summary>
        /// <param name="hack">Changed value.</param>
        /// <param name="getter">Callback that gets the original value.</param>
        /// <param name="setter">Callback that sets the new value and restores the original value.</param>
        public Swindler(T hack, Func<T> getter, Action<T> setter)
        {
            _original = getter();
            _setter = setter;
            setter(hack);
        }

        /// <summary>
        /// Invokes the setter callback with the original value and destroys itself.
        /// </summary>
        public void Dispose()
        {
            _setter(_original);
        }
    }
}
