﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ValueGetter;

namespace ValueGetterTests
{
    public interface IMy 
    {
    }

    public class MyClass:IMy
    {
        public int MyProperty1 { get; set; }
        public string MyProperty2 { get; set; }
    }

    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class ValueGetterTests
    {

        [TestMethod]
        public void ObjectGetValues()
        {
            var data = new MyClass() { MyProperty1 = 123, MyProperty2 = "test" };
            var result = data.GetObjectValues();
            Assert.AreEqual(123, result["MyProperty1"]);
            Assert.AreEqual("test", result["MyProperty2"]);
        }

        [TestMethod]
        public void ObjectGetToStringValues()
        {
            var data = new MyClass() { MyProperty1 = 123, MyProperty2 = "test" };
            var result = data.GetToStringValues();
            Assert.AreEqual("123", result["MyProperty1"]);
            Assert.AreEqual("test", result["MyProperty2"]);
        }

        [TestMethod]
        public void PropertyGetValue()
        {
            var data = new MyClass() { MyProperty1 = 123 };
            var prop = data.GetType().GetProperty("MyProperty1");
            var result = prop.GetObjectValue(data);
            Assert.AreEqual(123, result);
        }

        [TestMethod]
        public void PropertyGetToStringValue()
        {
            var data = new MyClass() { MyProperty1 = 123 };
            var prop = data.GetType().GetProperty("MyProperty1");
            var result = prop.GetToStringValue(data);
            Assert.AreEqual("123", result);
        }

        [TestMethod]
        public void CacheTest()
        {
            {
                object data = new MyClass() { MyProperty1 = 123, MyProperty2 = "test" };
                for (int i = 0; i < 2; i++) /*for cache*/
                {
                    var result = data.GetObjectValues();
                    Assert.AreEqual((data as MyClass).MyProperty1, result["MyProperty1"]);
                    Assert.AreEqual((data as MyClass).MyProperty2, result["MyProperty2"]);
                }
            }
            {
                var data = new MyClass() { MyProperty1 = 123, MyProperty2 = "test" };
                for (int i = 0; i < 2; i++) /*for cache*/
                {
                    var result = data.GetObjectValues();
                    Assert.AreEqual((data).MyProperty1, result["MyProperty1"]);
                    Assert.AreEqual((data).MyProperty2, result["MyProperty2"]);
                }
            }
            {
                object data = new { MyProperty1 = 123, MyProperty2 = "test" };
                for (int i = 0; i < 2; i++) /*for cache*/
                {
                    var result = data.GetToStringValues();
                    Assert.AreEqual("123", result["MyProperty1"]);
                    Assert.AreEqual("test", result["MyProperty2"]);
                }
            }
        }

        /// <summary>
        /// Fix:Using the upcast variable results in the following error
        /// <![CDATA[
        ///    var data1 = new MyClass { MyProperty1 = 123, MyProperty2 = "test" };
        ///    IMy data2 = new MyClass { MyProperty1 = 123, MyProperty2 = "test" };
        /// ]]>
        /// Error:System.ArgumentException: 'ParameterExpression of type 'ValueGetterTests.MyClass' 
        /// cannot be used for delegate parameter of type 'ValueGetterTests.IMy''
        /// </summary>
        [TestMethod]
        public void IssueForUpcastError()
        {
            var data1 = new MyClass { MyProperty1 = 123, MyProperty2 = "test" };
            IMy data2 = new MyClass { MyProperty1 = 123, MyProperty2 = "test" };
            for (int i = 0; i < 2; i++) /*for cache*/
            {
                var result = data1.GetToStringValues();
                Assert.AreEqual("123", result["MyProperty1"]);
                Assert.AreEqual("test", result["MyProperty2"]);
                var result2 = data2.GetToStringValues();
                Assert.AreEqual("123", result2["MyProperty1"]);
                Assert.AreEqual("test", result2["MyProperty2"]);
            }
        }

        [TestMethod]
        public void PropNull()
        {
            MyClass data = null;
            var props = typeof(MyClass).GetProperties();
            foreach (var prop in props)
            {
                //var result = prop.GetValue(data); //System.Reflection.TargetException: 'Non-static method requires a target.'
                var result =  prop.GetObjectValue(data);
            }
        }
    }
}
