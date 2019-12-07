using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SenseNet.Tools.Tests
{
    [TestClass]
    public class UtilityTests
    {
        [TestMethod]
        public void Utility_Convert_BytesToLongToBytes_8bytes()
        {
            var bytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            var converted = Utility.Convert.BytesToLong(bytes);
            var back = Utility.Convert.LongToBytes(converted);

            Assert.AreEqual(0x0102030405060708L, converted);
            Assert.AreEqual("1,2,3,4,5,6,7,8", string.Join(",", back.Select(x => x.ToString())));
        }
        [TestMethod]
        public void Utility_Convert_BytesToLongToBytes_NegativeOne()
        {
            var bytes = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, };
            var converted = Utility.Convert.BytesToLong(bytes);
            var back = Utility.Convert.LongToBytes(converted);

            Assert.AreEqual(-1, converted);
            Assert.AreEqual("255,255,255,255,255,255,255,255", string.Join(",", back.Select(x => x.ToString())));
        }
        [TestMethod]
        public void Utility_Convert_BytesToLongToBytes_ShorterArray()
        {
            var bytes = new byte[] { 1, 2, 3 };
            var converted = Utility.Convert.BytesToLong(bytes);
            var back = Utility.Convert.LongToBytes(converted);

            Assert.AreEqual(0x010203L, converted);
            Assert.AreEqual("0,0,0,0,0,1,2,3", string.Join(",", back.Select(x => x.ToString())));
        }
        [TestMethod]
        public void Utility_Convert_BytesToLongToBytes_LongerArray()
        {
            var bytes = new byte[] { 111, 112, 113, 114, 115, 116, 117, 118, 1, 2, 3, 4, 5, 6, 7, 8 };
            var converted = Utility.Convert.BytesToLong(bytes);
            var back = Utility.Convert.LongToBytes(converted);

            Assert.AreEqual(0x0102030405060708L, converted);
            Assert.AreEqual("1,2,3,4,5,6,7,8", string.Join(",", back.Select(x => x.ToString())));
        }

    }
}
