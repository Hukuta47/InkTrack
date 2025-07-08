using InkTrack_Report.Windows.Dialog;
using System;
using System.Linq;

namespace InkTrack_Report.Classes
{
    public class User
    {
        static public int GetID()
        {
            if (Environment.UserName.ToLower() == "сотрудник")
            {
                var dialog = new GetEmployeeIdDialog();
                if (dialog.ShowDialog() == true)
                {
                    return dialog.EmployeeId;
                }
            }
            else
            {
                return App.entities.Employee.FirstOrDefault(e => e.DomainName == Environment.UserName.ToLower()).Id;
            }
            return 0;
        }
    }
}
