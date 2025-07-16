using InkTrack.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace UnitTest_InkTrack
{
    [TestClass]
    public class WordHelperTests
    {
        [TestMethod]
        public void TestCheckGenerateWord()
        {
            // Arrange 
            bool expected = true;

            // Act
            WordHelper wh = new WordHelper("C:\\Users\\hk\\Desktop\\LIT-Program-solution\\InkTrack\\bin\\Debug\\Resources\\Request replace cartrige.docx");
            wh.GenerateFileWord(
                FULL_NAME: "Зелтынь Никита Станиславович",
                CARTRIDGE_NUMBER: "0001",
                DEVICE_NAME: "Xerox Phaser 3300 MFP",
                ROOM_NAME: "210",
                SUGGESTION: "было распечатано 2 страницы"
            );

            //Assert

            bool actual = File.Exists($"Заявка на заправку картриджа от {DateTime.Now:dd.MM.yyyy}.docx");

            Assert.AreEqual(expected, actual, "Тест пройден");


        }
    }
}
