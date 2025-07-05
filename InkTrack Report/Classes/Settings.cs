using Newtonsoft.Json;
using System.IO;


namespace InkTrack_Report.Classes
{
    public struct Settings
    {
        static public int SelectedCabinetID = -1;
        public int _selectedCabinetID { get { return SelectedCabinetID; } set { SelectedCabinetID = value; } }
        static public int SelectedPrinterID = -1;
        public int _selectedPrinterID { get { return SelectedCabinetID; } set { SelectedCabinetID = value; } }
        static public void Init()
        {
            if (!Directory.Exists("C:\\ProgramData\\InkTrackReport"))
            {
                Directory.CreateDirectory("C:\\ProgramData\\InkTrackReport");
            }
            if (File.Exists("C:\\ProgramData\\InkTrackReport\\data.json"))
            {
                string fileData = File.ReadAllText("C:\\ProgramData\\InkTrackReport\\data.json");
                Settings settings = JsonConvert.DeserializeObject<Settings>(fileData);

                SelectedCabinetID = settings._selectedCabinetID;
                SelectedPrinterID = settings._selectedPrinterID;
            }
            else
            {
                File.WriteAllText("C:\\ProgramData\\InkTrackReport\\data.json", JsonConvert.SerializeObject(new Settings()));
            }
        }
    }
}
