using MvvmHelpers;
using PonyvilleSchool2._0.Core;
using PonyvilleSchool2._0.Views.Administrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PonyvilleSchool2._0.ViewModels.Administrator
{
    public class AdminWindowViewModel : BaseViewModel
    {
        public ICommand ShowUsersCommand { get; }
        public ICommand ShowAnalyticsCommand { get; }
        public ICommand ExitCommand { get; }

        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public AdminWindowViewModel()
        {
            ShowUsersCommand = new RelayCommand(_ => CurrentView = new UsersView());
            ShowAnalyticsCommand = new RelayCommand(_ => CurrentView = new AnalyticsView());
            ExitCommand = new RelayCommand(_ => CloseForm());
            
            CurrentView = new UsersView();
        }

        public void CloseForm()
        {
            Application.Current.Shutdown();
        }
    }
}
