using MvvmHelpers;
using PonyvilleSchool2._0.Core;
using PonyvilleSchool2._0.Services;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace PonyvilleSchool2._0.ViewModels
{
    public class HeadViewModel : BaseViewModel
    {
        private readonly HubViewModel _main; //MainMenuVM

        public string UserAvatarUrl => AppState.Instance.CurrentUser.avatar; //Аватар пользователя
        private string _message;
        private int _availableTasksCount;
        public string Message
        {
            get => _message;
            set { _message = value; OnPropertyChanged(); }
        } //Сообщение
        public int AvailableTasksCount
        {
            get => _availableTasksCount;
            set { _availableTasksCount = value; OnPropertyChanged(); }
        }  //Кол-во доступных заданий
        public SolidColorBrush HeaderBrush { get; } =
            new SolidColorBrush(Colors.LightPink); //Расцветка шапки

        private SolidColorBrush _taskBackground;
        public SolidColorBrush TaskBackground
        {
            get => _taskBackground;
            set { _taskBackground = value; OnPropertyChanged(); }
        } //Фон количества задач

        private SolidColorBrush _taskBorderBrush;
        public SolidColorBrush TaskBorderBrush
        {
            get => _taskBorderBrush;
            set { _taskBorderBrush = value; OnPropertyChanged(); }
        } //Обводка количества задач

        public RelayCommand OpenProfileCommand { get; } //Обработчик открытие профиля

        public HeadViewModel(HubViewModel main) //Конструктор
        {
            _main = main;
            _main.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(HubViewModel.SelectedCourse))
                    AnimateHeader();
            };
            Message = MessageService.GetWelcomeMessage();
            AppState.Instance.TaskCompleted += RefreshAvailableTasks;

            RefreshAvailableTasks();

            OpenProfileCommand = new RelayCommand(_ => OpenProfile());
        }
        private void OpenProfile()
        {
            _main._openProfile();
            AppState.Instance.SoundService.PlaySound("select1");
        } //Открыть профиль
        public void ShowMessage(string text)
        {
            Message = text;
        } //Показать сообщение
        public void RefreshAvailableTasks() //Обновление кол-ва доступных заданий
        {
            AvailableTasksCount = AppState.Instance.CurrentUser.available;
            UpdateTaskColor();
        }
        public void UpdateTaskColor()
        {
            switch (AvailableTasksCount)
            {
                case 0:
                    TaskBackground = new SolidColorBrush(Color.FromRgb(251, 96, 127));
                    TaskBorderBrush = new SolidColorBrush(Color.FromRgb(247, 47, 87));
                    break;
                case 1:
                    TaskBackground = new SolidColorBrush(Color.FromRgb(255, 165, 87));
                    TaskBorderBrush = new SolidColorBrush(Color.FromRgb(255, 138, 36));
                    break;
                default: // >1
                    TaskBackground = new SolidColorBrush(Color.FromRgb(163, 209, 71));
                    TaskBorderBrush = new SolidColorBrush(Color.FromRgb(122, 163, 39));
                    break;
            }
        } //Расцветка количества доступных заданий
        public void ShowTaskCompletedMessage(int score)
        {
            string baseMsg = $"Задание завершено! +{score} баллов.";
            if (AvailableTasksCount > 0)
                Message = $"{baseMsg} У вас осталось задание на сегодня.";
            else
                Message = $"{baseMsg} Все задания на сегодня выполнены!";
        } //Показать сообщение о завершении задания
        private void AnimateHeader()
        {
            var newColor =
                _main.SelectedCourse == null
                ? Colors.LightPink
                : (Color)ColorConverter.ConvertFromString(_main.SelectedCourse.color);

            var animation = new ColorAnimation
            {
                To = newColor,
                Duration = TimeSpan.FromMilliseconds(400),
                EasingFunction = new QuadraticEase()
            };

            HeaderBrush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
        } //Анимация шапки
    }
}
