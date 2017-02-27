using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using shannon_exp;

namespace shannon_exp.Tests
{
    [TestClass]
    public class ReduceTest
    {


        [TestMethod]
        public void TestMethod1()
        {
            ShannonExpansion s = new ShannonExpansion("a+b");
            string result = s.expand();

        }
    }
}
