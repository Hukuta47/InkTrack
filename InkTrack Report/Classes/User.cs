using InkTrack_Report.Windows.Dialog;
using System;
using System.Linq;

namespace InkTrack_Report.Classes
{
    public class User
    {
        static public int GetID()
        {
            switch (Environment.UserName.ToLower())
            {
                case "сотрудник":
                    var dialog = new GetEmployeeIdDialog();
                    if (dialog.ShowDialog() == true)
                    {
                        return dialog.EmployeeId;
                    }
                    break;
                case "student":
                    return -1;
                default:
                    return App.entities.Employee.FirstOrDefault(e => e.DomainName == Environment.UserName.ToLower()).Id;;
            }
            return -1;
        }
    }
}
