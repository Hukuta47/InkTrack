using Request_Refill.Database;

namespace Request_Refill.Classes
{
    public class ProgramData
    {
        public int idSelectedCabinet { get; set; } = -1;
        public int idFromWhoDefaultSelect { get; set; } = -1;
        public int idPrinterDefaultSelect { get; set; } = -1;
        public int CountPrintersInCabinet { get; set; } = -1;
        public int CountEmployeesInCabinet { get; set; } = -1;

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            ProgramData other = (ProgramData)obj;

            return idSelectedCabinet == other.idSelectedCabinet &&
                   idFromWhoDefaultSelect == other.idFromWhoDefaultSelect &&
                   idPrinterDefaultSelect == other.idPrinterDefaultSelect &&
                   CountPrintersInCabinet == other.CountPrintersInCabinet &&
                   CountEmployeesInCabinet == other.CountEmployeesInCabinet;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + idSelectedCabinet.GetHashCode();
                hash = hash * 23 + idFromWhoDefaultSelect.GetHashCode();
                hash = hash * 23 + idPrinterDefaultSelect.GetHashCode();
                hash = hash * 23 + CountPrintersInCabinet.GetHashCode();
                hash = hash * 23 + CountEmployeesInCabinet.GetHashCode();
                return hash;
            }
        }




    }
}
