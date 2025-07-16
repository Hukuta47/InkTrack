using InkTrack.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTest_InkTrack
{
    [TestClass]
    public class TestGenativeFullName
    {
        [TestMethod]
        public void TestGetGenetiveFullName()
        {
            // Arrange
            string expectFullName = "Розетки Вилки Питаниковича";

            // Act
            string checkingFullName = FullNameHelper.GetGenetiveFullName("Розетка Вилка Питаникович");
            Console.WriteLine(checkingFullName);

            // Assert
            Assert.AreEqual(expectFullName, checkingFullName);
        }
    }
}
