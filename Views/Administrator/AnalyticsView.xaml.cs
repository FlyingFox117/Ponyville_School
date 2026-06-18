using PonyvilleSchool2._0.ViewModels.Administrator;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PonyvilleSchool2._0.Views.Administrator
{
    /// <summary>
    /// Логика взаимодействия для AnalyticsView.xaml
    /// </summary>
    public partial class AnalyticsView : UserControl
    {
        public AnalyticsView()
        {
            InitializeComponent();
            DataContext = new AnalyticsViewModel();
        }
    }
}
