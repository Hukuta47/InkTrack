using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Request_Refill.Classes
{
    public class PrintoutData
    {
        public int Number { get; set; }
        public string NameDocument { get; set; }
        public DateTime date { get; set; }
        public int CountPages { get; set; }
    }

}
