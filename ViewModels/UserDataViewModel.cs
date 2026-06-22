using PonyvilleSchool2._0.Core;
using PonyvilleSchool2._0.Services;

namespace PonyvilleSchool2._0.ViewModels
{
    public class UserDataViewModel : ViewModelBase
    {
        private readonly ProfileViewModel _main; //Главное родительское окно

        //Привязка к данным профиля
        public string Username => AppState.Instance.CurrentUser.name;
        public string UserId => AppState.Instance.CurrentUser.id.ToString();
        public string UserLevel => AppState.Instance.CurrentUser.level.ToString();
        public string RegistrationDate => AppState.Instance.CurrentUser.reg_date.ToString("yyyy-MM-dd");
        public string UserAvatarUrl => AppState.Instance.CurrentUser.avatar;
        public int CompletedTasksCount => AppState.Instance.CurrentUser.unique_tasks;
        public int TotalCompletedTasks => AppState.Instance.CurrentUser.total_results;
        public string FavoriteCourse => AppState.Instance.CurrentUser.favourite_course;

        public RelayCommand ExitCommand { get; } //Обработчик выхода из аккаунта
        public RelayCommand EditingCommand { get; } //Обработчик редактирования аккаунта
        public RelayCommand SettingsCommand { get; } //Обработчик редактирования аккаунта
        public UserDataViewModel(ProfileViewModel profileViewModel)
        {
            _main = profileViewModel;

            ExitCommand = new RelayCommand(_ => _main.Logout(false));
            EditingCommand = new RelayCommand(_ => _main.ToRedactionMode());
            SettingsCommand = new RelayCommand(_ => _main.ToSettingsMode());
            UpdateUserStats();
        }
        public void UpdateUserStats() //Данные пользователя
        {
            OnPropertyChanged(nameof(UserLevel));
            OnPropertyChanged(nameof(CompletedTasksCount));
            OnPropertyChanged(nameof(FavoriteCourse));
            OnPropertyChanged(nameof(TotalCompletedTasks));
        } //Обновление данных пользователя
    }
}
