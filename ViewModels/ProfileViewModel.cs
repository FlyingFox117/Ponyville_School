using PonyvilleSchool2._0.Core;
using PonyvilleSchool2._0.Models;
using PonyvilleSchool2._0.Services;
using PonyvilleSchool2._0.Views;
using PonyvilleSchool2._0.Views.Authentication;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace PonyvilleSchool2._0.ViewModels
{
    public class ProfileViewModel : ViewModelBase
    {
        private bool _historyLoaded; //Загружена ли история
        public bool NotCompleted => CompletedTasks.Count == 0;
        public bool NotAchievements => Achievements.Count == 0;

        private object _currentView; 
        public object CurrentView
        {
            get => _currentView;
            set
            { 
                _currentView = value;
                OnPropertyChanged();
            }
        } //Текущее окно (ProfileVM, SettingsVM, UserEditVM)

        //Дочерние VM
        public UserDataViewModel _userDataVM;
        public SettingsViewModel _settingsVM;
        public ProfileEditViewModel _userEditVM;

        private readonly Action _openHub; 

        public RelayCommand BackCommand { get; } //Обработчик возврата к меню

        public ProfileViewModel(Action openHub)
        {
            _openHub = openHub;

            BackCommand = new RelayCommand(_ =>
            {
                AppState.Instance.SoundService.PlaySound("select1");
                CurrentView = _userDataVM;
                _openHub();
            });

            AppState.Instance.TaskCompleted += async () =>
            {
                _historyLoaded = false;
                await LoadHistory();
                await LoadAchievements();
                await AppState.Instance.RefreshProfileStats();
                _userDataVM.UpdateUserStats();
            };

            ToProfile();
            LoadHistory();
            LoadAchievements();
        } //Конструктор

        public async Task Logout(bool deleting)
        {
            if (!deleting)
            {
                bool result = await PasswordChecking();
                if (!result)
                {
                    return;
                }
            }
            // Формируем путь к файлу сессии
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string sessionFile = Path.Combine(appData, "Ponyville School", "session.dat");

            // Удаляем файл, если он есть
            try
            {
                if (File.Exists(sessionFile))
                {
                    string encryptedtoken = File.ReadAllText(sessionFile);
                    string token = SecurityService.Decrypt(encryptedtoken);
                    string tokenhash = SecurityService.HashToken(token.ToString());

                    await AppState.Instance.Supabase.DeleteToken(tokenhash);
                    File.Delete(sessionFile);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка удаления сессии: {ex.Message}");
            }
            // Открываем обратно меню авторизации
            var start = new StartWindow();
            AppState.Instance.CurrentUser = new User();
            start.Show();

            Application.Current.MainWindow = start;

            Application.Current.Windows //Закрытие окна меню
                    .OfType<MainMenuWindow>()
                    .FirstOrDefault()?
                    .Close();
        } //Выход из аккаунта
        public async void ToRedactionMode()
        {
            bool result = await PasswordChecking();
            if (!result)
            {
                return;
            }
            _userEditVM ??= new ProfileEditViewModel(this);
            AppState.Instance.SoundService.PlaySound("select1");
            CurrentView = _userEditVM;
        } //В режим редактирования
        public void ToSettingsMode()
        {
            AppState.Instance.SoundService.PlaySound("select1");
            _settingsVM ??= new SettingsViewModel(this);
            CurrentView = _settingsVM;
        } //Переход в настройки
        public void ToProfile()
        {
            _userDataVM ??= new UserDataViewModel(this);
            CurrentView = _userDataVM;
        } //Переход в профиль
        private async Task<bool> PasswordChecking()
        {
           var input = MessageBoxHelper.ShowInput(
           "Введите пароль для подтверждения действия:",
           "Нужен пароль!",
           "",
           MessageBoxViewModel.Images.Question);

            if (input.Result == MessageBoxViewModel.Result.OK)
            {
                string enteredPassword = input.InputText;
                //Сравнение пароля
                bool result = await AppState.Instance.Supabase.CheckPassword(AppState.Instance.CurrentUser.id, enteredPassword);
                if (result)
                {
                    return true;
                }
                else
                {
                    MessageBoxHelper.Show("Неверный пароль!", "Ошибка", MessageBoxViewModel.Images.Error);
                    return false;
                }
            }
            return false;
        } //Проверка пароля пользователя
        private async Task LoadHistory()
        {
            if (_historyLoaded)
                return;

            var tasks =
                await AppState.Instance.Supabase
                    .GetCompletedTasks(
                        AppState.Instance.CurrentUser.id);

            CompletedTasks.Clear();

            foreach (var task in tasks)
                CompletedTasks.Add(task);

            _historyLoaded = true;
            OnPropertyChanged(nameof(NotCompleted));
        } //Загрузка списка выполненных задач
        private async Task LoadAchievements()
        {
            var achievements =
                await AppState.Instance.Supabase
                    .GetAchievementStats(AppState.Instance.CurrentUser.id);

            Achievements.Clear();

            foreach (var achievement in achievements)
            {
                Achievements.Add(achievement);
            }

            OnPropertyChanged(nameof(NotAchievements));
        } //Загрузка полученных достижений
       
        public ObservableCollection<Achievement> Achievements { get; }
            = new(); //Список достижений
        public ObservableCollection<CompletedTaskInfo> CompletedTasks { get; }
            = new(); //Список выполненных задач
    }
}
