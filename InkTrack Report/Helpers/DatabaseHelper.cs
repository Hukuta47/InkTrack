using InkTrack_Report.Database;
using InkTrack_Report.Classes;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace InkTrack_Report.Helpers
{
    public struct DatabaseHelper
    {
        public static void SavePrintDataToDatabase(PrintoutData printoutData, Printer printer)
        {
            List<PrintoutData> printoutDatas;

            XmlSerializer serializer = new XmlSerializer(typeof(List<PrintoutData>));
            if (string.IsNullOrWhiteSpace(printer.PrinterDocumentsList))
            {
                printoutDatas = new List<PrintoutData>();
            }
            else
            {
                using (var stringReader = new StringReader(printer.PrinterDocumentsList))
                {
                    printoutDatas = (List<PrintoutData>)serializer.Deserialize(stringReader);
                }
            }
            printoutDatas.Add(printoutData);

            serializer = new XmlSerializer(typeof(List<PrintoutData>));
            using (var stringWriter = new StringWriter())
            {
                serializer.Serialize(stringWriter, printoutDatas);
                printer.PrinterDocumentsList = stringWriter.ToString();
                App.entities.SaveChanges();
            }

        }
        public static void ResetPrintoutDataHistory(Printer printer)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<PrintoutData>));
            using (var stringWriter = new StringWriter())
            {
                serializer.Serialize(stringWriter, new List<PrintoutData>());
                printer.PrinterDocumentsList = stringWriter.ToString();
                App.entities.SaveChanges();
            }
        }
        public static List<PrintoutData> GetPrintOutDataList(Printer printer)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<PrintoutData>));
            if (string.IsNullOrWhiteSpace(printer.PrinterDocumentsList))
            {
                return new List<PrintoutData>();
            }
            else
            {
                using (var stringReader = new StringReader(printer.PrinterDocumentsList))
                {
                    return (List<PrintoutData>)serializer.Deserialize(stringReader);
                }
            }
        }
    }
}
