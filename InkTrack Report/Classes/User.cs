using InkTrack_Report.Database;
using InkTrack_Report.Windows.Dialog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows.Documents;

namespace InkTrack_Report.Classes
{
    public class User
    {
        static public int GetID()
        {
            string UserName = Environment.UserName.ToLower();
            switch (UserName)
            {
                case "student":
                    return -1;
                default:
                bool Exist = true;
                List<Employee> Employees = App.entities.Employee.Where(Employee => Employee.DomainName.Contains(UserName)).ToList();
                if (Employees.Count == 0)
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
                break;
            }
            return -1;
        }
    }
}
