using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LIT_Organizer.Windows.Equipment_map
{
    /// <summary>
    /// Логика взаимодействия для EquipmentMap.xaml
    /// </summary>
    public partial class EquipmentMap : Window
    {
        public EquipmentMap()
        {
            InitializeComponent();

            DataGrid_ListCabinets.ItemsSource = App.dbEntites.CabinetDevicesEmployees.ToList();
        }
    }
}
