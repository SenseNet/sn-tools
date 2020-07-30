using System;
using System.Xml.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SenseNet.Testing;

namespace SenseNet.Tools.Tests
{
    [TestClass]
    public class AccessorTests
    {
        #region private class AccessorTestObjects

        private abstract class AbstractionForAccessorTest
        {
            protected static int ProtectedStaticProperty { get; set; } = 42;

            protected static string ProtectedStaticMethod1(char c, int count)
            {
                return "ProtectedStaticMethod1" + new string(c, count);
            }
            protected static string ProtectedStaticMethod2(char c, int count)
            {
                return "ProtectedStaticMethod2" + new string(c, count);
            }
            protected abstract int ProtectedInheritedProperty { get; set; }
            protected abstract string ProtectedInheritedAbstractMethod(char c, int count);
            protected virtual string ProtectedInheritedVirtualMethod(char c, int count) { return "abstract value"; }
        }

        private class AccessorTestObject : AbstractionForAccessorTest
        {
            private static int _staticPrivateField;
            private static int StaticPrivateProperty { get; set; }

            private int _instancePrivateField;
            private int InstancePrivateProperty { get; set; }

            public static int _staticPublicField;
            public static int StaticPublicProperty { get; set; }

            public int _instancePublicField;
            public int InstancePublicProperty { get; set; }

            public AccessorTestObject()
            {
            }
            private AccessorTestObject(int field)
            {
                _instancePrivateField = field;
                _instancePublicField = field;
            }
            private AccessorTestObject(int field, int property):this(field)
            {
                InstancePrivateProperty = property;
                InstancePublicProperty = property;
            }
            public AccessorTestObject(int field, int property, bool isPublic) : this(field, property)
            {
                InstancePrivateProperty = property;
                InstancePublicProperty = property;
            }

            private static string StaticPrivateMethod(char c, int count)
            {
                return "StaticPrivateMethod" + new string(c, count);
            }
            private string InstancePrivateMethod(char c, int count)
            {
                return "InstancePrivateMethod" + new string(c, count);
            }

            public static string StaticPublicMethod(char c, int count)
            {
                return "StaticPublicMethod" + new string(c, count);
            }
            public string InstancePublicMethod(char c, int count)
            {
                return "InstancePublicMethod" + new string(c, count);
            }

            protected new static string ProtectedStaticMethod2(char c, int count)
            {
                return "ProtectedOverriddenStaticMethod2" + new string(c, count);
            }
            protected override int ProtectedInheritedProperty { get; set; }
            protected override string ProtectedInheritedAbstractMethod(char c, int count)
            {
                return "ProtectedInheritedAbstractMethod" + new string(c, count);
            }
            protected override string ProtectedInheritedVirtualMethod(char c, int count)
            {
                return $"ProtectedInheritedVirtualMethod{new string(c, count)} " +
                       $"{base.ProtectedInheritedVirtualMethod(c, count)}";
            }
        }
        #endregion

        /* =============================================== Access to private members */

        [TestMethod]
        public void Accessor_Private_Type_Field()
        {
            var objAcc = new TypeAccessor(typeof(AccessorTestObject));

            var origValue = (int)objAcc.GetStaticField("_staticPrivateField");
            objAcc.SetStaticField("_staticPrivateField", origValue + 1);
            var actualValue = (int)objAcc.GetStaticField("_staticPrivateField");
            Assert.AreEqual(origValue + 1, actualValue);

            objAcc.SetStaticFieldOrProperty("_staticPrivateField", origValue - 1);
            actualValue = (int)objAcc.GetStaticFieldOrProperty("_staticPrivateField");
            Assert.AreEqual(origValue - 1, actualValue);
        }
        [TestMethod]
        public void Accessor_Private_Type_Property()
        {
            var objAcc = new TypeAccessor(typeof(AccessorTestObject));

            var origValue = (int)objAcc.GetStaticProperty("StaticPrivateProperty");
            objAcc.SetStaticProperty("StaticPrivateProperty", origValue + 1);
            var actualValue = (int)objAcc.GetStaticProperty("StaticPrivateProperty");
            Assert.AreEqual(origValue + 1, actualValue);

            objAcc.SetStaticFieldOrProperty("StaticPrivateProperty", origValue - 1);
            actualValue = (int)objAcc.GetStaticFieldOrProperty("StaticPrivateProperty");
            Assert.AreEqual(origValue - 1, actualValue);
        }
        [TestMethod]
        public void Accessor_Private_Type_Method()
        {
            var objAcc = new TypeAccessor(typeof(AccessorTestObject));

            var actualValue = (string)objAcc.InvokeStatic("StaticPrivateMethod", '*', 5);
            Assert.AreEqual("StaticPrivateMethod*****", actualValue);

            actualValue = (string)objAcc.InvokeStatic("StaticPrivateMethod", 
                new[] {typeof(char), typeof(int)}, 
                new object[]{'*', 3});

            Assert.AreEqual("StaticPrivateMethod***", actualValue);
        }

        [TestMethod]
        public void Accessor_Private_Object_Ctor()
        {
            var objAcc = new ObjectAccessor(typeof(AccessorTestObject), 12, 42);

            Assert.AreEqual(12, objAcc.GetFieldOrProperty("_instancePrivateField"));
            Assert.AreEqual(42, objAcc.GetFieldOrProperty("InstancePrivateProperty"));
        }
        [TestMethod]
        public void Accessor_Private_Object_Ctor_Types()
        {
            var objAcc = new ObjectAccessor(typeof(AccessorTestObject),
                new[] {typeof(int), typeof(int)}, new object[] {12, 42});

            Assert.AreEqual(12, objAcc.GetFieldOrProperty("_instancePrivateField"));
            Assert.AreEqual(42, objAcc.GetFieldOrProperty("InstancePrivateProperty"));
        }

        [TestMethod]
        public void Accessor_Private_Object_Field()
        {
            var typeAcc = new ObjectAccessor(new AccessorTestObject());

            var origValue = (int)typeAcc.GetField("_instancePrivateField");
            typeAcc.SetField("_instancePrivateField", origValue + 1);
            var actualValue = (int)typeAcc.GetField("_instancePrivateField");
            Assert.AreEqual(origValue + 1, actualValue);

            typeAcc.SetFieldOrProperty("_instancePrivateField", origValue - 1);
            actualValue = (int)typeAcc.GetFieldOrProperty("_instancePrivateField");
            Assert.AreEqual(origValue - 1, actualValue);
        }
        [TestMethod]
        public void Accessor_Private_Object_Property()
        {
            var typeAcc = new ObjectAccessor(new AccessorTestObject());

            var origValue = (int)typeAcc.GetProperty("InstancePrivateProperty");
            typeAcc.SetProperty("InstancePrivateProperty", origValue + 1);
            var actualValue = (int)typeAcc.GetProperty("InstancePrivateProperty");
            Assert.AreEqual(origValue + 1, actualValue);

            typeAcc.SetFieldOrProperty("InstancePrivateProperty", origValue - 1);
            actualValue = (int)typeAcc.GetFieldOrProperty("InstancePrivateProperty");
            Assert.AreEqual(origValue - 1, actualValue);
        }
        [TestMethod]
        public void Accessor_Private_Object_Method()
        {
            var typeAcc = new ObjectAccessor(new AccessorTestObject());

            var actualValue = (string)typeAcc.Invoke("InstancePrivateMethod", '*', 5);
            Assert.AreEqual("InstancePrivateMethod*****", actualValue);

            actualValue = (string)typeAcc.Invoke("InstancePrivateMethod",
                new[] { typeof(char), typeof(int) },
                new object[] { '*', 3 });

            Assert.AreEqual("InstancePrivateMethod***", actualValue);
        }

        /* =============================================== Access to public members */

        [TestMethod]
        public void Accessor_Public_Type_Field()
        {
            var objAcc = new TypeAccessor(typeof(AccessorTestObject));

            var origValue = (int)objAcc.GetStaticField("_staticPublicField");
            objAcc.SetStaticField("_staticPublicField", origValue + 1);
            var actualValue = (int)objAcc.GetStaticField("_staticPublicField");
            Assert.AreEqual(origValue + 1, actualValue);

            objAcc.SetStaticFieldOrProperty("_staticPublicField", origValue - 1);
            actualValue = (int)objAcc.GetStaticFieldOrProperty("_staticPublicField");
            Assert.AreEqual(origValue - 1, actualValue);
        }
        [TestMethod]
        public void Accessor_Public_Type_Property()
        {
            var objAcc = new TypeAccessor(typeof(AccessorTestObject));

            var origValue = (int)objAcc.GetStaticProperty("StaticPublicProperty");
            objAcc.SetStaticProperty("StaticPublicProperty", origValue + 1);
            var actualValue = (int)objAcc.GetStaticProperty("StaticPublicProperty");
            Assert.AreEqual(origValue + 1, actualValue);

            objAcc.SetStaticFieldOrProperty("StaticPublicProperty", origValue - 1);
            actualValue = (int)objAcc.GetStaticFieldOrProperty("StaticPublicProperty");
            Assert.AreEqual(origValue - 1, actualValue);
        }
        [TestMethod]
        public void Accessor_Public_Type_Method()
        {
            var objAcc = new TypeAccessor(typeof(AccessorTestObject));

            var actualValue = (string)objAcc.InvokeStatic("StaticPublicMethod", '*', 5);
            Assert.AreEqual("StaticPublicMethod*****", actualValue);

            actualValue = (string)objAcc.InvokeStatic("StaticPublicMethod",
                new[] { typeof(char), typeof(int) },
                new object[] { '*', 3 });

            Assert.AreEqual("StaticPublicMethod***", actualValue);
        }

        [TestMethod]
        public void Accessor_Public_Object_Ctor()
        {
            var objAcc = new ObjectAccessor(typeof(AccessorTestObject), 12, 42, true);

            Assert.AreEqual(12, objAcc.GetFieldOrProperty("_instancePublicField"));
            Assert.AreEqual(42, objAcc.GetFieldOrProperty("InstancePublicProperty"));
        }
        [TestMethod]
        public void Accessor_Public_Object_Ctor_Types()
        {
            var objAcc = new ObjectAccessor(typeof(AccessorTestObject),
                new[] { typeof(int), typeof(int), typeof(bool) }, 
                new object[] { 12, 42, true });

            Assert.AreEqual(12, objAcc.GetFieldOrProperty("_instancePublicField"));
            Assert.AreEqual(42, objAcc.GetFieldOrProperty("InstancePublicProperty"));
        }

        [TestMethod]
        public void Accessor_Public_Object_Field()
        {
            var typeAcc = new ObjectAccessor(new AccessorTestObject());

            var origValue = (int)typeAcc.GetField("_instancePublicField");
            typeAcc.SetField("_instancePublicField", origValue + 1);
            var actualValue = (int)typeAcc.GetField("_instancePublicField");
            Assert.AreEqual(origValue + 1, actualValue);

            typeAcc.SetFieldOrProperty("_instancePublicField", origValue - 1);
            actualValue = (int)typeAcc.GetFieldOrProperty("_instancePublicField");
            Assert.AreEqual(origValue - 1, actualValue);
        }
        [TestMethod]
        public void Accessor_Public_Object_Property()
        {
            var typeAcc = new ObjectAccessor(new AccessorTestObject());

            var origValue = (int)typeAcc.GetProperty("InstancePublicProperty");
            typeAcc.SetProperty("InstancePublicProperty", origValue + 1);
            var actualValue = (int)typeAcc.GetProperty("InstancePublicProperty");
            Assert.AreEqual(origValue + 1, actualValue);

            typeAcc.SetFieldOrProperty("InstancePublicProperty", origValue - 1);
            actualValue = (int)typeAcc.GetFieldOrProperty("InstancePublicProperty");
            Assert.AreEqual(origValue - 1, actualValue);
        }
        [TestMethod]
        public void Accessor_Public_Object_Method()
        {
            var typeAcc = new ObjectAccessor(new AccessorTestObject());

            var actualValue = (string)typeAcc.Invoke("InstancePublicMethod", '*', 5);
            Assert.AreEqual("InstancePublicMethod*****", actualValue);

            actualValue = (string)typeAcc.Invoke("InstancePublicMethod",
                new[] { typeof(char), typeof(int) },
                new object[] { '*', 3 });

            Assert.AreEqual("InstancePublicMethod***", actualValue);
        }

        /* ----------------------------------------------- Inheritance tests */

        [TestMethod]
        public void Accessor_Protected_Type_Property_OnAbstract()
        {
            var typeAcc = new TypeAccessor(typeof(AbstractionForAccessorTest));

            var origValue = (int)typeAcc.GetStaticProperty("ProtectedStaticProperty");
            typeAcc.SetStaticProperty("ProtectedStaticProperty", origValue + 1);
            var actualValue = (int)typeAcc.GetStaticProperty("ProtectedStaticProperty");
            Assert.AreEqual(origValue + 1, actualValue);

            typeAcc.SetStaticFieldOrProperty("ProtectedStaticProperty", origValue - 1);
            actualValue = (int)typeAcc.GetStaticFieldOrProperty("ProtectedStaticProperty");
            Assert.AreEqual(origValue - 1, actualValue);
        }
        [TestMethod]
        public void Accessor_Protected_Type_Property_OnImplementation()
        {
            var typeAcc = new TypeAccessor(typeof(AccessorTestObject));

            var origValue = (int)typeAcc.GetStaticProperty("ProtectedStaticProperty");
            typeAcc.SetStaticProperty("ProtectedStaticProperty", origValue + 1);
            var actualValue = (int)typeAcc.GetStaticProperty("ProtectedStaticProperty");
            Assert.AreEqual(origValue + 1, actualValue);

            typeAcc.SetStaticFieldOrProperty("ProtectedStaticProperty", origValue - 1);
            actualValue = (int)typeAcc.GetStaticFieldOrProperty("ProtectedStaticProperty");
            Assert.AreEqual(origValue - 1, actualValue);

        }

        [TestMethod]
        public void Accessor_Protected_Type_Method1_OnAbstract()
        {
            var objAcc = new TypeAccessor(typeof(AbstractionForAccessorTest));

            var actualValue = (string)objAcc.InvokeStatic("ProtectedStaticMethod1", '@', 7);
            Assert.AreEqual("ProtectedStaticMethod1@@@@@@@", actualValue);

            actualValue = (string)objAcc.InvokeStatic("ProtectedStaticMethod1",
                new[] { typeof(char), typeof(int) },
                new object[] { '*', 3 });

            Assert.AreEqual("ProtectedStaticMethod1***", actualValue);
        }
        [TestMethod]
        public void Accessor_Protected_Type_Method1_OnImplementation()
        {
            var objAcc = new TypeAccessor(typeof(AccessorTestObject));

            var actualValue = (string)objAcc.InvokeStatic("ProtectedStaticMethod1", '@', 7);
            Assert.AreEqual("ProtectedStaticMethod1@@@@@@@", actualValue);

            actualValue = (string)objAcc.InvokeStatic("ProtectedStaticMethod1",
                new[] { typeof(char), typeof(int) },
                new object[] { '*', 3 });

            Assert.AreEqual("ProtectedStaticMethod1***", actualValue);
        }
        [TestMethod]
        public void Accessor_Protected_Type_Method2_OnAbstract()
        {
            var objAcc = new TypeAccessor(typeof(AbstractionForAccessorTest));

            var actualValue = (string)objAcc.InvokeStatic("ProtectedStaticMethod2", '@', 7);
            Assert.AreEqual("ProtectedStaticMethod2@@@@@@@", actualValue);

            actualValue = (string)objAcc.InvokeStatic("ProtectedStaticMethod2",
                new[] { typeof(char), typeof(int) },
                new object[] { '*', 3 });

            Assert.AreEqual("ProtectedStaticMethod2***", actualValue);
        }
        [TestMethod]
        public void Accessor_Protected_Type_Method2_OnImplementation()
        {
            var objAcc = new TypeAccessor(typeof(AccessorTestObject));

            var actualValue = (string)objAcc.InvokeStatic("ProtectedStaticMethod2", '@', 7);
            Assert.AreEqual("ProtectedOverriddenStaticMethod2@@@@@@@", actualValue);

            actualValue = (string)objAcc.InvokeStatic("ProtectedStaticMethod2",
                new[] { typeof(char), typeof(int) },
                new object[] { '*', 3 });

            Assert.AreEqual("ProtectedOverriddenStaticMethod2***", actualValue);
        }

        [TestMethod]
        public void Accessor_Protected_Object_InheritedProperty_OnAbstract()
        {
            AbstractionForAccessorTest instance = new AccessorTestObject();
            var typeAcc = new ObjectAccessor(instance);

            var origValue = (int)typeAcc.GetProperty("ProtectedInheritedProperty");
            typeAcc.SetProperty("ProtectedInheritedProperty", origValue + 1);
            var actualValue = (int)typeAcc.GetProperty("ProtectedInheritedProperty");
            Assert.AreEqual(origValue + 1, actualValue);

            typeAcc.SetFieldOrProperty("ProtectedInheritedProperty", origValue - 1);
            actualValue = (int)typeAcc.GetFieldOrProperty("ProtectedInheritedProperty");
            Assert.AreEqual(origValue - 1, actualValue);
        }
        [TestMethod]
        public void Accessor_Protected_Object_InheritedProperty_OnImplementation()
        {
            var typeAcc = new ObjectAccessor(new AccessorTestObject());

            var origValue = (int)typeAcc.GetProperty("ProtectedInheritedProperty");
            typeAcc.SetProperty("ProtectedInheritedProperty", origValue + 1);
            var actualValue = (int)typeAcc.GetProperty("ProtectedInheritedProperty");
            Assert.AreEqual(origValue + 1, actualValue);

            typeAcc.SetFieldOrProperty("ProtectedInheritedProperty", origValue - 1);
            actualValue = (int)typeAcc.GetFieldOrProperty("ProtectedInheritedProperty");
            Assert.AreEqual(origValue - 1, actualValue);
        }

        [TestMethod]
        public void Accessor_Protected_Object_InheritedAbstractMethod()
        {
            AbstractionForAccessorTest instance = new AccessorTestObject();
            var typeAcc = new ObjectAccessor(instance);

            var actualValue = (string)typeAcc.Invoke("ProtectedInheritedAbstractMethod", '*', 5);
            Assert.AreEqual("ProtectedInheritedAbstractMethod*****", actualValue);

            actualValue = (string)typeAcc.Invoke("ProtectedInheritedAbstractMethod",
                new[] { typeof(char), typeof(int) },
                new object[] { '*', 3 });

            Assert.AreEqual("ProtectedInheritedAbstractMethod***", actualValue);
        }

        [TestMethod]
        public void Accessor_Protected_Object_InheritedVirtualMethod_OnAbstract()
        {
            AbstractionForAccessorTest instance = new AccessorTestObject();
            var typeAcc = new ObjectAccessor(instance);

            var actualValue = (string)typeAcc.Invoke("ProtectedInheritedVirtualMethod", '*', 5);
            Assert.AreEqual("ProtectedInheritedVirtualMethod***** abstract value", actualValue);

            actualValue = (string)typeAcc.Invoke("ProtectedInheritedVirtualMethod",
                new[] { typeof(char), typeof(int) },
                new object[] { '*', 3 });

            Assert.AreEqual("ProtectedInheritedVirtualMethod*** abstract value", actualValue);
        }

        [TestMethod]
        public void Accessor_Protected_Object_InheritedVirtualMethod_OnImplementation()
        {
            var typeAcc = new ObjectAccessor(new AccessorTestObject());

            var actualValue = (string)typeAcc.Invoke("ProtectedInheritedVirtualMethod", '*', 5);
            Assert.AreEqual("ProtectedInheritedVirtualMethod***** abstract value", actualValue);

            actualValue = (string)typeAcc.Invoke("ProtectedInheritedVirtualMethod",
                new[] { typeof(char), typeof(int) },
                new object[] { '*', 3 });

            Assert.AreEqual("ProtectedInheritedVirtualMethod*** abstract value", actualValue);
        }

        /* =============================================== Swindler tests */

        [TestMethod]
        public void Swindler()
        {
            AccessorTestObject.StaticPublicProperty = 42;
            try
            {
                using (new Swindler<int>(142,
                    () => AccessorTestObject.StaticPublicProperty,
                    v => AccessorTestObject.StaticPublicProperty = v))
                {
                    Assert.AreEqual(142, AccessorTestObject.StaticPublicProperty);
                    throw new Exception("An exception for fault-tolerance test.");
                }
            }
            catch { /* ignored */ }
            Assert.AreEqual(42, AccessorTestObject.StaticPublicProperty);
        }
    }
}
