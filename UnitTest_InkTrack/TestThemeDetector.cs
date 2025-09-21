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
    public class TestThemeDetector
    {
        [TestMethod]
        public void CheckTheme()
        {
            // Arrange
            var expect = AppTheme.Dark;

            // Act
            var themeNow = ThemeDetector.GetWindowsTheme();

            // Assert
            Assert.AreEqual(expect, themeNow);
        }
    }
}
