using PonyvilleSchool2._0.Core;
using PonyvilleSchool2._0.Models;
using PonyvilleSchool2._0.Services;
using PonyvilleSchool2._0.Services.Sounds;
using PonyvilleSchool2._0.Views;
using System.Collections.ObjectModel;
using System.Windows;

namespace PonyvilleSchool2._0.ViewModels
{
    public class TasksViewModel : ViewModelBase
    {
        private readonly HubViewModel _main; //MainMenuVM

        private int? _loadedCourseId = null; //Загруженный ранее курс
        public string Score { get; set; } //Изображение очков
        public string Background { get; set; } //Задний фон
        public ObservableCollection<CourseTask> Tasks { get; } = new(); //Коллекция заданий
        public bool _isLoading { get; set; }
        public bool IsLoading //Загрузка списка заданий
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand<CourseTask> OpenTaskCommand { get; } //Обработчик запуска задания

        public TasksViewModel(HubViewModel main) //Конструктор
        {
            _main = main;
            OpenTaskCommand = new RelayCommand<CourseTask>(OpenTask); //Привящка команды запуска задания
        }
        public async Task LoadTasksAsync(bool isUpdate) //Загрузка заданий
        {
            var courseId = _main.SelectedCourse.id;
            var userProgress = _main.SelectedCourse.completed_tasks;
            Score = _main.SelectedCourse.score_image_url;
            Background = _main.SelectedCourse.course_image_url;
            AppState.Instance.SoundService.PlaySound("select1");

            if (_loadedCourseId == courseId && Tasks.Count > 0 && !isUpdate) //Если курс тот же - запрос не выполняется
                return;

            IsLoading = true;

            Tasks.Clear();

            var tasks = await AppState.Instance.Supabase
                .GetTasksData(courseId, AppState.Instance.CurrentUser.id);
            if (tasks == null)
            {
                MessageBoxHelper.Show("Кажется, что-то пошло не так", "Ой!", MessageBoxViewModel.Images.Error);
                return;
            }
            foreach (var t in tasks)
                Tasks.Add(t);

            _loadedCourseId = courseId;
            foreach (var t in Tasks)
            {
                if (t.result > 0)
                {
                    t.available = true; //Выполненое задание
                }
                else if (t.number == userProgress + 1)
                {
                    t.available = true; //Новое задание
                }
                else
                {
                    t.available = false; //Недоступное
                }
            }

            IsLoading = false;
        }
        private void OpenTask(CourseTask task) //Открытие задания
        {
            // Пользователю сегодня задания недоступны
            if (AppState.Instance.CurrentUser.available == 0)
            {
                VoicePlayerService.PlayPhrase("taskout");
                MessageBoxHelper.Show("На сегодня задания кончились. Приходи завтра!",
                    "Ой!",
                    MessageBoxViewModel.Images.Error,
                    MessageBoxViewModel.Buttons.OK);
                return;
            }
            // Нарушена последовательность внутри курса
            if (!task.available)
            {
                VoicePlayerService.PlayPhrase("locked");
                MessageBoxHelper.Show("Сначала пройди предыдущее задание",
                    "Ой!",
                    MessageBoxViewModel.Images.Warning,
                    MessageBoxViewModel.Buttons.OK);
                return;
            }
            // Если задание уже выполнялось (есть очки)
            if (task.result > 0)
            {
                VoicePlayerService.PlayPhrase("finishedtask");
                var result = MessageBoxHelper.Show(
                    $"Ты уже проходил это задание.\nТы набрал: {task.result} очков.\n\nХочешь перепройти?",
                    "Повторное прохождение",
                    MessageBoxViewModel.Images.Question,
                    MessageBoxViewModel.Buttons.YesNo);

                if (result == MessageBoxViewModel.Result.No)
                    return;
            }
            else
            {
                // Новое задание
                VoicePlayerService.PlayPhrase("starttask");
                var result = MessageBoxHelper.Show(
                    "Начать новое задание?",
                    "Новое задание",
                    MessageBoxViewModel.Images.Info,
                    MessageBoxViewModel.Buttons.YesNo);

                if (result == MessageBoxViewModel.Result.No)
                    return;
            }

            // Запуск окна задания
            var taskWindow = new TaskWindow(
                task,
                _main.SelectedCourse.color,
                _main.SelectedCourse.id,
                _main.SelectedCourse.score_image_url);
            taskWindow.Show();

            Application.Current.MainWindow.Hide();

            taskWindow.Closed += (_, __) =>
            {
                Application.Current.MainWindow.Show();
            };
        }
    }
}
