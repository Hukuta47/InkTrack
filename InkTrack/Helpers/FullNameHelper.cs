using NPetrovich;

namespace InkTrack.Helpers
{
    public class FullNameHelper
    {
        static public string GetGenetiveFullName(string FullName)
        {
            

            var fioParts = FullName.Split(' ');
            string lastName = fioParts.Length > 0 ? fioParts[0] : "";
            string firstName = fioParts.Length > 1 ? fioParts[1] : "";
            string middleName = fioParts.Length > 2 ? fioParts[2] : "";

            var petrovich = new Petrovich()
            {
                LastName = lastName,
                FirstName = firstName,
                MiddleName = middleName,
                AutoDetectGender = true
            };

            var inflectedFIO = petrovich.InflectTo(Case.Genitive);
            string GenetiveFIO = string.IsNullOrWhiteSpace(inflectedFIO.MiddleName)
                ? $"{inflectedFIO.LastName} {inflectedFIO.FirstName}"
                : $"{inflectedFIO.LastName} {inflectedFIO.FirstName} {inflectedFIO.MiddleName}";


            return GenetiveFIO;
        }

        

    }
}
