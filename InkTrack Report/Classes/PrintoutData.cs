using System;

namespace InkTrack_Report.Classes
{
    public struct PrintoutData
    {
        public string FIOWhoPrinting { get; set; }
        public string NameDocument { get; set; }
        public DateTime Date { get; set; }
        public int CountPages { get; set; }
    }

}
