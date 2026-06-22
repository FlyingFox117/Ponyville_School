using PonyvilleSchool2._0.Core;
using PonyvilleSchool2._0.Models;
using PonyvilleSchool2._0.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace PonyvilleSchool2._0.ViewModels
{ 
    public class CoursesViewModel : ViewModelBase
    {
        private readonly HubViewModel _main; //MainMenuVM

        public ObservableCollection<Course> Courses { get; } //Массив курсов
        public bool _isLoading { get; set; } 
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        } //Загрузка списка курсов

        public RelayCommand<Course> SelectCourseCommand { get; } //Выбор курса

        public CoursesViewModel(HubViewModel main) //Конструктор
        {
            _main = main;

            SelectCourseCommand = new RelayCommand<Course>(course =>
            {
                _main.SelectedCourse = course;
            });
            Courses = new ObservableCollection<Course>();
        }
        public async Task InitializeAsync() 
        {
            IsLoading = true;

            var courses = await AppState.Instance.Supabase
                .GetCoursesData(AppState.Instance.CurrentUser.id);

            if (courses == null)
            {
                MessageBoxHelper.Show("Кажется, что-то пошло не так", "Ой!", MessageBoxViewModel.Images.Error);
                return;
            }
            Courses.Clear();
            foreach (var c in courses)
                Courses.Add(c);

            IsLoading = false;
        } //Инициализация при запуск
        public async Task CourseReflesh()
        {
            int courseId = _main.SelectedCourse.id;
            int? userId = AppState.Instance.CurrentUser.id;

            var result = await AppState.Instance.Supabase.GetCourseData(userId, courseId);

            if (result == null)
            {
                return;
            }
            var existing = Courses.FirstOrDefault(c => c.id == courseId);

            if (existing != null)
            {
                existing.completed_tasks = result.completed_tasks;
                existing.total_tasks = result.total_tasks;
            }
        } //Обновление данных после прохождения задания
    }
}
