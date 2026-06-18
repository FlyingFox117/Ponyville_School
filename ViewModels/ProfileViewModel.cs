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
        private bool _historyLoaded;
        public bool NotCompleted => CompletedTasks.Count == 0;
        public bool NotAchievements => Achievements.Count == 0;

        //Привязка к данным профиля
        public string Username => AppState.Instance.CurrentUser.name;
        public string UserId => AppState.Instance.CurrentUser.id.ToString();
        public string UserLevel => AppState.Instance.CurrentUser.level.ToString();
        public string RegistrationDate => AppState.Instance.CurrentUser.reg_date.ToString("yyyy-MM-dd");
        public string UserAvatarUrl => AppState.Instance.CurrentUser.avatar;
        public int CompletedTasksCount => AppState.Instance.CurrentUser.unique_tasks;
        public int TotalCompletedTasks => AppState.Instance.CurrentUser.total_results;
        public string FavoriteCourse => AppState.Instance.CurrentUser.favourite_course;
        private readonly Action _openHub; 

        public RelayCommand BackCommand { get; } //Обработчик возврата к меню
        public RelayCommand ExitCommand { get; } //Обработчик выхода из аккаунта
        public RelayCommand EditingCommand { get; } //Обработчик редактирования аккаунта

        public ProfileViewModel(Action openHub)
        {
            _openHub = openHub;

            BackCommand = new RelayCommand(_ =>
            {
                AppState.Instance.SoundService.PlaySound("select1");
                _openHub();
            });
            ExitCommand = new RelayCommand(_ => Logout());
            EditingCommand = new RelayCommand(_ => ToRedactionMode());
            AppState.Instance.TaskCompleted += async () =>
            {
                _historyLoaded = false;
                await LoadHistory();
                await LoadAchievements();
                await AppState.Instance.RefreshProfileStats();
                UpdateUserStats();
            };
            LoadHistory();
            LoadAchievements();
        } //Конструктор
        private async Task Logout()
        {
            bool result = await PasswordChecking();
            if (!result)
            {
                return;
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
        private async void ToRedactionMode()
        {
            bool result = await PasswordChecking();
            if (!result)
            {
                return;
            }
        } //В режим редактирования
        private async Task<bool> PasswordChecking()
        {
           var input = MessageBoxHelper.ShowInput(
           "Введите пароль для выхода:",
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
        private void UpdateUserStats()
        {
            OnPropertyChanged(nameof(UserLevel));
            OnPropertyChanged(nameof(CompletedTasksCount));
            OnPropertyChanged(nameof(FavoriteCourse));
            OnPropertyChanged(nameof(TotalCompletedTasks));
        } //Обновление данных пользователя
        public ObservableCollection<Achievement> Achievements { get; }
            = new(); //Список достижений
        public ObservableCollection<CompletedTaskInfo> CompletedTasks { get; }
            = new(); //Список выполненных задач
    }
}
