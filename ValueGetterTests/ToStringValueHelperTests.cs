using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using ValueGetter;

namespace ValueGetterTests
{
    public class MyClass
    {
        public int MyProperty1 { get; set; }
        public string MyProperty2 { get; set; }
    }

    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class ValueGetterTests
    {

        [TestMethod]
        public void GetObjectValuesByObjectType()
        {
            object data = new MyClass() { MyProperty1 = 123, MyProperty2 = "test" }; 
            for (int i = 0; i < 2; i++) /*for cache*/
            {
                var result = data.GetObjectValues();
                Assert.AreEqual((data as MyClass).MyProperty1, result["MyProperty1"]);
                Assert.AreEqual((data as MyClass).MyProperty2, result["MyProperty2"]);
            }
        }

        [TestMethod]
        public void GetObjectValuesStrongType()
        {
            var data = new MyClass() { MyProperty1 = 123, MyProperty2 = "test" };
            for (int i = 0; i < 2; i++) /*for cache*/
            {
                var result = data.GetObjectValues();
                Assert.AreEqual((data).MyProperty1, result["MyProperty1"]);
                Assert.AreEqual((data).MyProperty2, result["MyProperty2"]);
            }
        }

        [TestMethod]
        public void GetObjectValuesToStringStrongType()
        {
            object data = new MyClass() { MyProperty1 = 123, MyProperty2 = "test" };
            for (int i = 0; i < 2; i++) /*for cache*/
            {
                var result = data.GetToStringValues();
                //Assert.AreEqual((data).MyProperty1, result["MyProperty1"]);
                //Assert.AreEqual((data).MyProperty2, result["MyProperty2"]);
            }
        }
    }
}
