using InkTrack_Report.Database;
using InkTrack_Report.Windows.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InkTrack_Report.Helpers
{
    public struct UserHelper
    {
        static public int GetID()
        {
            string UserName = Environment.UserName.ToLower();
            switch (UserName)
            {
                case "student":
                    return -1;
                default:
                List<Employee> Employees = App.entities.Employee.Where(Employee => Employee.DomainName.Contains(UserName)).ToList();
                if (Employees.Count == 0)
                {
                    return 0;
                }
                else
                {
                    return App.entities.Employee.FirstOrDefault(e => e.DomainName == Environment.UserName.ToLower()).Id;
                }
            }
        }
    }
}
