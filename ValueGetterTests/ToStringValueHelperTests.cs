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
        private static MyClass _Data = new MyClass() { MyProperty1 = 123, MyProperty2 = "test" };

        [TestMethod]
        public void ValueObjectHelper()
        {
            var result = _Data.GetObjectValues();
            Assert.AreEqual(_Data.MyProperty1, result["MyProperty1"]);
            Assert.AreEqual(_Data.MyProperty2, result["MyProperty2"]);
        }

    }
}
