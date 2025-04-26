using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Request_Refill.Classes
{
    public class PrintedDocument
    {
        int Number { get; set; }
        string NameDocument { get; set; }
        DateTime date { get; set; }
        int NumberOfPages { get; set; }
    }

}
