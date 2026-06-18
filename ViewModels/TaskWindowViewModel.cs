using PonyvilleSchool.Services.Sounds;
using PonyvilleSchool2._0.Core;
using PonyvilleSchool2._0.Models;
using PonyvilleSchool2._0.Services;
using PonyvilleSchool2._0.Services.Sounds;
using PonyvilleSchool2._0.Views;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace PonyvilleSchool2._0.ViewModels
{
    public class TaskWindowViewModel : ViewModelBase
    {
        private readonly Queue<int> _blockQueue = new(); //Очередь блоков заданий
        private BlockBase _currentBlock; //Текущий блок задания
        private DispatcherTimer _globalTimer; //Таймер, считающий время выполнения задания
        public bool IsLastBlock =>
           _blockQueue.Count == 0; //Проверка на последний блок в задании
        public BlockBase CurrentBlock
        {
            get => _currentBlock;
            set
            {
                _currentBlock = value; OnPropertyChanged();
                OnPropertyChanged(nameof(IsLastBlock));
            }
        } //Текущий блок
        public string ContinuePhrase => IsLastBlock? "finish" : "continue";
        public Action CloseWindowAction { get; set; } //Закрытие окна

        private readonly int _taskId; //ID задания
        private readonly int _courseId; //ID курса

        private bool _isScoreAnimating;
        public bool IsScoreAnimating
        {
            get => _isScoreAnimating;
            set { _isScoreAnimating = value; OnPropertyChanged(); }
        } //Анимация очков
        public bool IsTimerRunning => CurrentBlock?.IsTimerActive == true;
        public bool IsLoadingBlock; //Загрузка блока
        private int _totalSeconds; //Секунды
        public string TotalTime //Общее время выполнения
        {
            get
            {
                var ts = TimeSpan.FromSeconds(_totalSeconds);
                return $"{ts.Minutes}:{ts.Seconds:D2}";
            }
        }
        private int _totalScore; //Результат
        private bool _isTaskFinished = false;
        public bool IsTaskFinished
        {
            get => _isTaskFinished;
            set
            {
                _isTaskFinished = value;
                OnPropertyChanged();
            }
        } //Завершен ли блок
        public int TotalScore //Подсчёт результата
        {
            get => _totalScore;
            set { _totalScore = value;
                OnPropertyChanged();
                TriggerScoreAnimation();
            }
        }
        public Brush HeaderColor { get; } //Цвет шапки задания
        public string TaskTitle { get; } //Название задания
        public string CourseScore { get; } //Картинка очков курса

        //Ниже - конструктор и методы полного цикла запуска задания от начала до завершения
        public TaskWindowViewModel(CourseTask task, string color, int courseId, string score) //1. Конструктор
        {
            TaskTitle = task.title;
            _taskId = task.id;
            _courseId = courseId;
            CourseScore = score;
            HeaderColor = (Brush)new BrushConverter()
            .ConvertFromString(color);
            //Разделения списка блоков на очередь
            foreach (var id in task.blocks.Split('-').Select(int.Parse))
                _blockQueue.Enqueue(id);

            _ = Initialize();
            StartGlobalTimer();
        }
        private void StartGlobalTimer() //2. Запуск таймера прохождения
        {
            _globalTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            _globalTimer.Tick += (_, __) =>
            {
                _totalSeconds++;
                OnPropertyChanged(nameof(TotalTime));
            };
        }
        private async Task Initialize() //3. Инициализация первого блока
        {
            await LoadNextBlock();
        }
        private async Task LoadNextBlock() //4. Загрузка следующего блока
        {
            if (IsLoadingBlock) return;
            IsLoadingBlock = true;
            OnPropertyChanged(nameof(ContinuePhrase));

            try
            {
                if (_blockQueue.Count == 0) //Если блок был последним - завершение задания
                {
                    await FinishTaskAsync();
                    return;
                }
                AppState.Instance.SoundService.PlaySound("continue");
                int id = _blockQueue.Dequeue(); //id - первый в очереди

                var block = await AppState.Instance.Supabase.GetBlockData(id); //метод - Запрос к Supabase
                
                if (block == null)
                {
                    await LoadNextBlock();
                    return;
                }

                var newBlock = BlockFactory.Create(block.type, block.content.ToString()); //создание нового блока

                if (CurrentBlock != null) //Отписка от предыдущего блока
                {
                    CurrentBlock.TaskFinished -= OnBlockFinished;
                }
                CurrentBlock = newBlock;
                CurrentBlock.TaskFinished += OnBlockFinished; //Подписка на новый блок
                OnPropertyChanged(nameof(IsTimerRunning));
                if (IsTimerRunning) //Определение рабочего таймера в блоке
                {
                    _globalTimer.Start(); //Старт таймера
                }
                else
                    _globalTimer.Stop(); //Остановка таймера
            }
            finally
            {
                IsLoadingBlock = false;
            }
        }
        private async void OnBlockFinished() //5. Завершение блока
        {
            if (IsLoadingBlock) 
                return;
            TotalScore += CurrentBlock.GetScore();
            await LoadNextBlock();
        }
        private async void TriggerScoreAnimation() //6. Запуск анимации очков
        {
            IsScoreAnimating = true;
            await Task.Delay(400);
            IsScoreAnimating = false;
        }
        private async Task FinishTaskAsync() //7. Завершение задания
        {
            _globalTimer.Stop();
            IsTaskFinished = true;

            int? p_user_id = AppState.Instance.CurrentUser.id;
            int p_task_id = _taskId;
            int p_score = TotalScore;
            int p_course_id = _courseId;

            await AppState.Instance.Supabase.SubmitResult(
                    p_user_id,
                    p_task_id,
                    p_score,
                    p_course_id);
            AppState.Instance.SoundService.PlaySound("taskend"); //Проигрывание звука

            // Потом здесь появится список достижений
            var achievements = new List<Achievement>();

            CurrentBlock = new ResultBlock(
                TotalScore,
                TotalTime,
                CourseScore,
                achievements);

            // Изменение данных пользователя
            AppState.Instance.CurrentUser.available -= 1;
            AppState.Instance.LastCompletedScore = TotalScore;
        }

        public RelayCommand NextBlockCommand => new(_ =>
        {
            if (IsTaskFinished) //Завершено ли задание
            {
                AppState.Instance.RaiseTaskCompleted();

                ((TaskWindow)Application.Current.Windows
                    .OfType<TaskWindow>()
                    .First())
                    .ForceClose();

                return;
            }

            if (CurrentBlock == null) //Текущий блок существует
            {
                LoadNextBlock();
                return;
            }

            CurrentBlock.OnNext(); //Метод текущего блока
        }); //Обработчик запуска следующего блока
        public RelayCommand ExitCommand => new(_ =>
        {
            CloseWindowAction?.Invoke();
            if (IsTaskFinished)
            {
                AppState.Instance.RaiseTaskCompleted();

                ((TaskWindow)Application.Current.Windows
                    .OfType<TaskWindow>()
                    .First())
                    .ForceClose();

                return;
            }
        }); //Обработчик выхода из окна задания
        public RelayCommand SpeakCommand => new(_ =>
        {
            if (CurrentBlock == null)
                return;
            TtsService.Speak(CurrentBlock.description);
        }
        ); //Обработчик озвучки блока
    }
}
