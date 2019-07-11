using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SenseNet.Configuration;

namespace SenseNet.Tools.Tests
{
    internal enum ConfigEnum { Value1, Value2, Value3 }

    [TestClass]
    public class SnConfigTests
    {
        #region Test data
        private static readonly Dictionary<string, string> TestConfigData = new Dictionary<string, string>
        {
            {"key1", "value1"},
            {"key2", "123"},
            {"key3", "1.2"},
            {"key4", "a,b,c"},
            {"keyX", "qwe"},

            // section
            {"feature1:key1", "value999"},
            {"feature1:key2", "456"},
            {"feature1:key3", "3.4"},
            {"feature1:key4", "d,e;f"}, // note the two different separators
            {"feature1:key5", "7,8,9"},
            {"feature1:key6", "Value2"},
            {"feature1:key7", ""},
            {"feature1:key8", "true"},

            //subsection
            {"sensenet:subsection:key1", "subvalue1"}
        };
        #endregion

        [TestMethod]
        public void SnConfig_SectionName_ExistingValue()
        {
            void Test()
            {
                // from section
                Assert.AreEqual("value999", SnConfig.GetValue<string>("feature1", "key1"));
                // from root settings directly
                Assert.AreEqual("value1", SnConfig.GetValue<string>(null, "key1"));
                
                // empty, but existing key
                Assert.IsTrue(SnConfig.GetList<string>("feature1", "key7").SequenceEqual(new List<string>()));
                Assert.IsTrue(SnConfig.GetListOrEmpty<string>("feature1", "key7").SequenceEqual(new List<string>()));
            }

            using (new ConfigurationSwindler(CreateTestConfiguration()))
            {
                Test();

                // In new environments there should be no fallback: keyX does not exist under feature1.
                Assert.IsNull(SnConfig.GetValue<string>("feature1", "keyX"));
            }

            using (new ConfigurationSwindler(new SnLegacyConfiguration()))
            {
                Test();

                // legacy behavior in old environments: appSettings fallback
                Assert.AreEqual("qwe", SnConfig.GetValue<string>("feature1", "keyX"));
            }
        }
        [TestMethod]
        public void SnConfig_SectionName_NonExistingValue()
        {
            void Test()
            {
                // existing section, no key
                Assert.IsNull(SnConfig.GetValue<string>("feature1", "NO-KEY"));
                // no section, no key
                Assert.IsNull(SnConfig.GetValue<string>("NO-feature", "NO-KEY"));
                Assert.IsNull(SnConfig.GetValue<string>(null, "NO-KEY"));

                // default values
                Assert.AreEqual("DEFAULT", SnConfig.GetValue("feature1", "NO-KEY", "DEFAULT"));
                Assert.AreEqual(99, SnConfig.GetValue("feature1", "NO-KEY", 99));
                Assert.AreEqual(99.9, SnConfig.GetValue("feature1", "NO-KEY", 99.9));
                Assert.IsTrue(SnConfig.GetList("feature1", "NO-KEY", new List<string> {"a", "b"})
                    .SequenceEqual(new List<string> {"a", "b"}));

                // empty list
                Assert.IsNull(SnConfig.GetList<string>("feature1", "NO-KEY"));
                Assert.IsTrue(SnConfig.GetList("feature1", "NO-KEY", new List<string>(0)).SequenceEqual(new List<string>()));
                Assert.IsTrue(SnConfig.GetListOrEmpty<string>("feature1", "NO-KEY").SequenceEqual(new List<string>()));
            }

            using (new ConfigurationSwindler(CreateTestConfiguration()))
                Test();
            using (new ConfigurationSwindler(new SnLegacyConfiguration()))
                Test();
        }

        [TestMethod]
        public void SnConfig_SectionName_TypeConversion()
        {
            void Test()
            {
                Assert.AreEqual(456, SnConfig.GetValue<int>("feature1", "key2"));
                Assert.AreEqual(3.4, SnConfig.GetValue<double>("feature1", "key3"));
                Assert.AreEqual(ConfigEnum.Value2, SnConfig.GetValue<ConfigEnum>("feature1", "key6"));

                var a1 = SnConfig.GetList<string>("feature1", "key4");
                Assert.IsTrue(a1.SequenceEqual(new []{"d", "e", "f"}));

                var a2 = SnConfig.GetList<int>("feature1", "key5");
                Assert.IsTrue(a2.SequenceEqual(new[] { 7, 8, 9 }));
            }

            using (new ConfigurationSwindler(CreateTestConfiguration()))
                Test();
            using (new ConfigurationSwindler(new SnLegacyConfiguration()))
                Test();
        }

        [TestMethod]
        public void SnConfig_SectionName_Boundaries()
        {
            void Test()
            {
                var b1 = SnConfig.GetInt("feature1", "key2", 0, 400);
                Assert.AreEqual(456, b1);
                b1 = SnConfig.GetInt("feature1", "key2", 0, 500);
                Assert.AreEqual(500, b1);
                b1 = SnConfig.GetInt("feature1", "key2", 0, 0, 400);
                Assert.AreEqual(400, b1);

                var b2 = SnConfig.GetDouble("feature1", "key3", 0, 1);
                Assert.AreEqual(3.4, b2);
                b2 = SnConfig.GetDouble("feature1", "key3", 0, 4.5);
                Assert.AreEqual(4.5, b2);
                b2 = SnConfig.GetDouble("feature1", "key3", 0, 1, 2.3);
                Assert.AreEqual(2.3, b2);
            }

            using (new ConfigurationSwindler(CreateTestConfiguration()))
                Test();
            using (new ConfigurationSwindler(new SnLegacyConfiguration()))
                Test();
        }

        [TestMethod]
        public void SnConfig_SectionName_Subsection()
        {
            void Test()
            {
                Assert.AreEqual("subvalue1", SnConfig.GetValue<string>("sensenet/subsection", "key1"));
                Assert.AreEqual("subvalue1", SnConfig.GetValue<string>("sensenet:subsection", "key1"));
            }

            using (new ConfigurationSwindler(CreateTestConfiguration()))
                Test();
            using (new ConfigurationSwindler(new SnLegacyConfiguration()))
                Test();
        }

        //============================================================================== Error tests

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void SnConfig_TypeConversion_Error1()
        {
            void Test()
            {
                // type conversion error
                SnConfig.GetValue<int>("feature1", "key1");
            }

            using (new ConfigurationSwindler(CreateTestConfiguration()))
                Test();
        }
        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void SnConfig_TypeConversion_Error2()
        {
            void Test()
            {
                // type conversion error
                SnConfig.GetList<int>("feature1", "key4");
            }

            using (new ConfigurationSwindler(CreateTestConfiguration()))
                Test();
        }

        private static IConfiguration CreateTestConfiguration()
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(TestConfigData)
                .Build();
        }
    }

    internal class ConfigurationSwindler : IDisposable
    {
        private readonly IConfiguration _originalConfig;

        public ConfigurationSwindler(IConfiguration configuration)
        {
            _originalConfig = SnConfig.Instance;
            SnConfig.Instance = configuration;
        }

        public void Dispose()
        {
            SnConfig.Instance = _originalConfig;
        }
    }
}
