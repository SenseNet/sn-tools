using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SenseNet.Configuration;

namespace SenseNet.Tools.Tests
{
    [TestClass]
    public class SnConfigTests
    {
        #region Test infrsatructure

        /// <summary>
        /// Test config provider that serves values from an in-memory store.
        /// </summary>
        private class InMemoryConfigProvider : IConfigProvider, IDisposable
        {
            private readonly Dictionary<string, string> _appSettings = new Dictionary<string, string>();

            private readonly Dictionary<string, Dictionary<string, string>> _sections =
                new Dictionary<string, Dictionary<string, string>>();

            private readonly IConfigProvider _originalConfigProvider = SnConfig.Instance;

            private InMemoryConfigProvider(Dictionary<string, string> appSettings,
                Dictionary<string, Dictionary<string, string>> sections)
            {
                if (appSettings != null)
                    _appSettings = appSettings;
                if (sections != null)
                    _sections = sections;
            }

            public string GetString(string sectionName, string key)
            {
                Dictionary<string, string> source;
                string configValue = null;

                if (!string.IsNullOrEmpty(sectionName) && _sections.TryGetValue(sectionName, out source) && source != null && source.ContainsKey(key))
                    configValue = source[key];

                if (configValue == null && _appSettings.ContainsKey(key))
                    configValue = _appSettings[key];

                return configValue;
            }

            internal static InMemoryConfigProvider Create(Dictionary<string, string> appSettings,
                Dictionary<string, Dictionary<string, string>> sections = null)
            {
                // Set this as the current config provider - we will switch back 
                // to the original when disposing this instance.
                var cp = new InMemoryConfigProvider(appSettings, sections);
                SnConfig.Instance = cp;

                return cp;
            }

            public void Dispose()
            {
                SnConfig.Instance = _originalConfigProvider;
            }
        }

        private enum ConfigEnum { Value1, Value2, Value3 }

        #endregion

        private static readonly Dictionary<string, Dictionary<string, string>> TestConfigSections = new Dictionary
            <string, Dictionary<string, string>>
        {
            {
                "feature1", new Dictionary<string, string>
                {
                    {"key1", "value999"},
                    {"key2", "456"},
                    {"key3", "3.4"},
                    {"key4", "d,e;f"}, // note the two different separators
                    {"key5", "7,8,9"},
                    {"key6", "Value2"}
                }
            }
        };

        private static readonly Dictionary<string, string> TestAppSettings = new Dictionary<string, string>
        {
            {"key1", "value1"},
            {"key2", "123"},
            {"key3", "1.2"},
            {"key4", "a,b,c"},
            {"keyX", "qwe"}
        };

        [TestMethod]
        public void SnConfig_ExistingValue()
        {
            using (InMemoryConfigProvider.Create(TestAppSettings, TestConfigSections))
            {
                // from section
                Assert.AreEqual("value999", SnConfig.GetValue<string>("feature1", "key1"));
                // from appSettings directly
                Assert.AreEqual("value1", SnConfig.GetValue<string>(null, "key1"));
                // from appSettings fallback
                Assert.AreEqual("qwe", SnConfig.GetValue<string>("feature1", "keyX"));
            }
        }
        [TestMethod]
        public void SnConfig_NonExistingValue()
        {
            using (InMemoryConfigProvider.Create(TestAppSettings, TestConfigSections))
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

                // empty list
                Assert.IsTrue(SnConfig.GetList<string>("feature1", "NO-KEY").SequenceEqual(new List<string>()));
            }
        }
        [TestMethod]
        public void SnConfig_TypeConversion()
        {
            using (InMemoryConfigProvider.Create(TestAppSettings, TestConfigSections))
            {
                Assert.AreEqual(456, SnConfig.GetValue<int>("feature1", "key2"));
                Assert.AreEqual(3.4, SnConfig.GetValue<double>("feature1", "key3"));
                Assert.AreEqual(ConfigEnum.Value2, SnConfig.GetValue<ConfigEnum>("feature1", "key6"));

                var a1 = SnConfig.GetList<string>("feature1", "key4");
                Assert.IsTrue(a1.SequenceEqual(new []{"d", "e", "f"}));

                var a2 = SnConfig.GetList<int>("feature1", "key5");
                Assert.IsTrue(a2.SequenceEqual(new[] { 7, 8, 9 }));
            }
        }
        [TestMethod]
        public void SnConfig_Boundaries()
        {
            using (InMemoryConfigProvider.Create(TestAppSettings, TestConfigSections))
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
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void SnConfig_TypeConversion_Error1()
        {
            using (InMemoryConfigProvider.Create(TestAppSettings, TestConfigSections))
            {
                // type conversion error
                SnConfig.GetValue<int>("feature1", "key1");
            }
        }
        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void SnConfig_TypeConversion_Error2()
        {
            using (InMemoryConfigProvider.Create(TestAppSettings, TestConfigSections))
            {
                // type conversion error
                SnConfig.GetList<int>("feature1", "key4");
            }
        }
    }
}
