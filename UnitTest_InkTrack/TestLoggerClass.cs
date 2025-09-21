using InkTrack.Classes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest_InkTrack
{
    [TestClass]
    public class TestLoggerClass
    {
        [TestMethod]
        public void TestLoggerOut()
        {
            // Arrange
            string expectString = $"{DateTime.Now.ToLongTimeString()} | TEST | text for test";

            // Act
            string stringText = Logger.LogText("TEST", "text for test");
            Console.WriteLine(stringText);

            //Assert
            Assert.AreEqual(expectString, stringText);

        }
    }
}
