using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using shannon_exp;

namespace ShannonTests
{
    [TestClass]
    public class BasicTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            string input = "~a & ~b & ~c | a & ~b & ~c | a & b & ~c | a & b & c | d";
            ShannonExpansion s = new shannon_exp.ShannonExpansion();
            string output = "";
            s.expand(input, ref output);
            Assert.AreEqual("~b(~c) + b(a)", output);
        }
    }
}
