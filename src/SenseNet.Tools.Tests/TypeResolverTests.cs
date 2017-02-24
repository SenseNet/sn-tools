using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SenseNet.Tools.Tests
{
    #region Test classes

    internal interface ITestInterface
    {
    }

    internal class BaseClass
    {
    }

    internal class DerivedClass1 : BaseClass
    {
    }
    internal class DerivedClass11 : DerivedClass1, ITestInterface
    {
    }
    internal class IndependentClass1 : ITestInterface
    {
    }

    #endregion

    [TestClass]
    public class TypeResolverTests
    {
        [TestMethod]
        public void TypeResolver_GetType()
        {
            var t = TypeResolver.GetType("SenseNet.Tools.TypeResolver");
            Assert.IsNotNull(t);
        }

        [TestMethod]
        public void TypeResolver_GetTypesByBaseType()
        {
            var types = TypeResolver.GetTypesByBaseType(typeof(BaseClass));

            Assert.AreEqual(2, types.Length);
            Assert.IsTrue(types.Any(t => t.Name == "DerivedClass1"));
            Assert.IsTrue(types.Any(t => t.Name == "DerivedClass11"));
        }
        [TestMethod]
        public void TypeResolver_GetTypesByInterface()
        {
            var types = TypeResolver.GetTypesByInterface(typeof(ITestInterface));

            Assert.AreEqual(2, types.Length);
            Assert.IsTrue(types.Any(t => t.Name == "DerivedClass11"));
            Assert.IsTrue(types.Any(t => t.Name == "IndependentClass1"));
        }

        [TestMethod]
        [ExpectedException(typeof(TypeNotFoundException))]
        public void TypeResolver_Error_GetType()
        {
            var t = TypeResolver.GetType("UnknownType");
            Assert.IsNotNull(t);
        }
    }
}
