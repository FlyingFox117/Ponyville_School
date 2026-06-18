using PonyvilleSchool2._0.Models;
using PonyvilleSchool2._0.Services;
using PonyvilleSchool2._0.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace PonyvilleSchool2._0.Views
{
    /// <summary>
    /// Логика взаимодействия для TaskWindow.xaml
    /// </summary>
    public partial class TaskWindow : Window
    {
        private bool _forceClose = false;
        public TaskWindow(CourseTask task, string color, int courseId, string score)
        {
            InitializeComponent();
            var vm = new TaskWindowViewModel(task, color, courseId, score);
            DataContext = vm;
            vm.CloseWindowAction = () => this.Close();
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            if (_forceClose)
                return;
            AppState.Instance.SoundService.PlaySound("question");
            var result = MessageBoxHelper.Show(
                "Ты точно хочешь закончить прохождение задания сейчас?",
                "Уже уходишь?",
                MessageBoxViewModel.Images.Question,
                MessageBoxViewModel.Buttons.YesNo);

            if (result == MessageBoxViewModel.Result.No)
                e.Cancel = true;
        }
        public void ForceClose()
        {
            _forceClose = true;
            Close();
        }
    }
}
