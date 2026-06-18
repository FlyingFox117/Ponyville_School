using MvvmHelpers;
using PonyvilleSchool2._0.Models.Administrator;
using PonyvilleSchool2._0.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PonyvilleSchool2._0.ViewModels.Administrator
{
    public class UsersViewModel : BaseViewModel
    {
        public ObservableCollection<UserStat> Users { get; set; } = new();

        public UsersViewModel()
        {
            LoadUsers();
        }

        private async void LoadUsers()
        {
            var result = await AppState.Instance.Supabase.GetUsersStats();
            if (result != null)
            {
                foreach (var u in result)
                    Users.Add(u);
            }
        }
    }
}
