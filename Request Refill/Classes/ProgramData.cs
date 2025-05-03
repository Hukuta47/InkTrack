using Request_Refill.Database;

namespace Request_Refill.Classes
{
    public class ProgramData
    {
        static public GetEmployeesInCabinet_Result FromWhoDefaultSelect { get; set; } = null;
        static public GetPrintersInCabinet_Result PrinterDefaultSelect { get; set; } = null;
        static public int CountPrintersInCabinet { get; set; } = 0;
        static public int CountEmployeesInCabinet { get; set; } = 0;
    }
}
