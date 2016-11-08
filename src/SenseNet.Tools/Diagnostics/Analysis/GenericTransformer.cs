using System;

#pragma warning disable 1591

namespace SenseNet.Diagnostics.Analysis
{
    /// <summary>
    /// EXPERIMENTAL FEATURE
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class GenericTransformer<T> : Transformer<T> where T : Entry
    {
        private readonly Func<T, string> _transformerMethod;

        public GenericTransformer(Func<T, string> transformerMethod)
        {
            _transformerMethod = transformerMethod;
        }

        public override string Transform(T entry)
        {
            return _transformerMethod(entry);
        }
    }
}
