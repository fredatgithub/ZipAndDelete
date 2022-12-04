using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ZipAndDeleteUnitTest
{
  [TestClass]
  public class UnitTest1
  {
    [TestMethod]
    public void TestMethod_concat_0()
    {
      var numberOfSpace = 0;
      var result = new String(' ', numberOfSpace);
      var expected = string.Empty;
      Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void TestMethod_concat_1()
    {
      var numberOfSpace = 1;
      var result = new String(' ', numberOfSpace);
      var expected = " ";
      Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void TestMethod_concat_2()
    {
      var numberOfSpace = 2;
      var result = new String(' ', numberOfSpace);
      var expected = "  ";
      Assert.AreEqual(expected, result);
    }
  }
}
