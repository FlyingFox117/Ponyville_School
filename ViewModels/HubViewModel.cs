using PonyvilleSchool2._0.Core;
using PonyvilleSchool2._0.Models;
using PonyvilleSchool2._0.Services;
using System.DirectoryServices.ActiveDirectory;

namespace PonyvilleSchool2._0.ViewModels
{
    public class HubViewModel : ViewModelBase
    {
        public CoursesViewModel CoursesVM { get; } //ViewModel курсов
        public TasksViewModel TasksVM { get; } //ViewModel заданий
        public HeadViewModel HeadVM { get; } //ViewModel шапки

        private object _currentView;
        public readonly Action _openProfile;
        public object CurrentView //Текущий экран (CoursesVM/TasksVM)
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }
        private Course _selectedCourse;
        public Course SelectedCourse //Выбранный курс
        {
            get => _selectedCourse;
            set
            {
                _selectedCourse = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProgressText));
                OnPropertyChanged(nameof(IsCourseSelected));
                OnPropertyChanged(string.Empty);
                AppState.Instance.SoundService.PlaySound("select1");
            }
        }
        public bool IsCourseSelected =>
           SelectedCourse != null; //Проверка, выбран ли курс
        public string ProgressText =>
            SelectedCourse == null
                ? ""
                : $"Выполнено: {SelectedCourse.completed_tasks}/{SelectedCourse.total_tasks}"; //Прогресс курса
        private bool _isTasksOpened;
        public bool IsTasksOpened
        {
            get => _isTasksOpened;
            set
            {
                _isTasksOpened = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(OpenButtonText));
            }
        } //Проверка, открыто ли задание
        public string OpenButtonText =>
            IsTasksOpened ? "Вернуться" : "Открыть задания"; //Надпись кнопки в правой панели
        public string MenuTitle =>
    SelectedCourse?.title ?? "Добро пожаловать!"; //Название курса
        public string MenuDescription =>
            SelectedCourse?.description ?? "Выбери открытый домик на карте и играй!"; //Описание курса
        public string ButtonPhrase =>
            IsTasksOpened ? "back" : "opentasks";
        public RelayCommand ToggleTasksCommand { get; } //Обработчик переключения с курсов на задания

        public HubViewModel(Action openProfile) //Конструктор
        {
            _openProfile = openProfile;

            CoursesVM = new CoursesViewModel(this);
            TasksVM = new TasksViewModel(this);
            HeadVM = new HeadViewModel(this);

            AppState.Instance.TaskCompleted += OnTaskCompleted;
            _ = CoursesVM.InitializeAsync();

            CurrentView = CoursesVM;

            ToggleTasksCommand = new RelayCommand(async _ =>
            {
                IsTasksOpened = !IsTasksOpened;

                if (IsTasksOpened)
                    await TasksVM.LoadTasksAsync(false);

                OnPropertyChanged(nameof(ButtonPhrase));

                CurrentView = IsTasksOpened ? TasksVM : CoursesVM;
            });
        }
        private async void OnTaskCompleted()
        {
            string MessageFinish = MessageService.GetTaskCompletedMessage(AppState.Instance.LastCompletedScore);

            HeadVM.ShowMessage(MessageFinish); //Обновление шапки
            await CoursesVM.CourseReflesh(); //Обновление данных курса
            await TasksVM.LoadTasksAsync(true); //Обновление данных заданий
            SelectedCourse = CoursesVM.Courses.FirstOrDefault(c => c.id == SelectedCourse.id);
        } //Обработка завершения задания

    }
}
