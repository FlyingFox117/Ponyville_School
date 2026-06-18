using GongSolutions.Wpf.DragDrop;
using PonyvilleSchool2._0.Core;
using PonyvilleSchool2._0.Services;
using PonyvilleSchool2._0.Services.Sounds;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace PonyvilleSchool2._0.Models
{
    public abstract class BlockBase : ViewModelBase
    {
        public string type { get; set; } = "text"; //Тип блока
        public string title { get; set; } = "Пони-Блок!"; //Название задания
        public string description { get; set; } //Текст в нижней части панели
        public abstract bool IsTimerActive { get; } //Активность таймера
        public abstract int GetScore(); //Получение очков
        public virtual void OnNext() //Выключение кнопки
        {
            IsInteractionLocked = true;
            OnPropertyChanged(nameof(CanContinue));
        }
        protected void UnlockInteraction() //Включение кнопки
        {
            IsInteractionLocked = false;
        }

        private bool _isInteractionLocked;
        public bool IsInteractionLocked
        {
            get => _isInteractionLocked;
            set
            {
                _isInteractionLocked = value;
                OnPropertyChanged(nameof(CanContinue));
            }
        }
        protected abstract bool IsTaskCompleted { get; }
        public bool CanContinue => IsTaskCompleted && !IsInteractionLocked;

        //Проверка на возможность запустить следующее задание

        public event Action TaskFinished; //Завершение задания
        protected void FinishBlock() //Завершение блока
        {
            OnPropertyChanged(nameof(CanContinue));
            TaskFinished?.Invoke();
            if (IsTimerActive)
            {
                VoicePlayerService.PlayRandomPraise();
            }
        }
    }

    // ---------- Блок десериализации ----------
    public static class BlockFactory
    {
        public static BlockBase Create(string type, string json) //Создание блока задания в зависимости от типа
        {
            BlockBase block = type switch
            {
                "text" => JsonSerializer.Deserialize<TextBlock>(json), //Текстовая теория
                "video" => JsonSerializer.Deserialize<VideoBlock>(json), //Видертеория
                "match" => JsonSerializer.Deserialize<MatchBlock>(json), //Найти соответствие
                "test" => JsonSerializer.Deserialize<TestBlock>(json), //Тестовая практика
                _ => null
            };
            InitializeIfNeeded(block); //Инициализация по требованию

            return block;
        }

        private static void InitializeIfNeeded(BlockBase block)
        {
            switch (block)
            {
                case MatchBlock match:
                    match.Initialize();
                    break;

                case TextBlock text:
                    text.Initialize();
                    break;

                case VideoBlock video:
                    break;

                case TestBlock test:
                    test.Initialize();
                    break;
            }
        }
    }

    // ---------- Текстовый блок (Теория) ----------
    public class TextBlock : BlockBase
    {
        public List<TextPage> pages { get; set; }

        private int _pageIndex = 0;
        public override bool IsTimerActive => false;
        public TextPage CurrentPage => pages[_pageIndex];
        protected override bool IsTaskCompleted => true;

        private string _currentBackgroundUrl;
        public string CurrentBackgroundUrl
        {
            get => _currentBackgroundUrl;
            set { _currentBackgroundUrl = value; OnPropertyChanged(); }
        }
        private string _currentCharacterUrl;
        private bool _isPageChanging;
        public bool IsPageChanging
        {
            get => _isPageChanging;
            set { _isPageChanging = value; OnPropertyChanged(); }
        }
        private string _displayedCharacterUrl;
        public string DisplayedCharacterUrl
        {
            get => _displayedCharacterUrl;
            set { _displayedCharacterUrl = value; OnPropertyChanged(); }
        }
        public void Initialize()
        {
            if (pages == null || pages.Count == 0)
                return;

            var first = pages[0];
            description = pages[_pageIndex].text;
            _currentBackgroundUrl = first.image_url;
            _currentCharacterUrl = first.character_url;
            DisplayedCharacterUrl = _currentCharacterUrl;   // сразу показываем
        }
        public override async void OnNext()
        {
            base.OnNext();
            if (_pageIndex < pages.Count - 1)
            {
                AppState.Instance.SoundService.PlaySound("select2");
                IsPageChanging = true;
                _pageIndex++;
                // Ждём, пока завершится фаза исчезновения (200 мс)
                await Task.Delay(200);

                // Обновляем URL персонажа по тем же правилам (сохраняем старый, если не задан)
                UpdateCharacterUrl();

                // Уведомляем, что текущая страница сменилась (для текста и фона)
                OnPropertyChanged(nameof(CurrentPage));
                UpdateBackgroundUrl();

                // Ждём, пока пройдёт фаза появления (ещё 200 мс)
                await Task.Delay(200);

                IsPageChanging = false;     // анимация завершена
                UnlockInteraction();        // кнопка снова активна
                description = pages[_pageIndex].text;
            }
            else //Если страница последняя - блок завершается
            {
                FinishBlock();
            }
        }
        private void UpdateBackgroundUrl()
        {
            var page = CurrentPage;
            if (!string.IsNullOrWhiteSpace(page.image_url))
                _currentBackgroundUrl = page.image_url;
            OnPropertyChanged(nameof(CurrentBackgroundUrl));
        }
        private void UpdateCharacterUrl()
        {
            var page = CurrentPage;
            if (page.character_url != null) // null — не трогаем, "" — прячем
            {
                _currentCharacterUrl = string.IsNullOrEmpty(page.character_url) ? null : page.character_url;
            }
            DisplayedCharacterUrl = _currentCharacterUrl;
        }
        public override int GetScore() => 1;
    } //Форма
    public class TextPage
    {
        public string text { get; set; } //Текст страницы
        public string image_url { get; set; } //Задний фон
        public string character_url { get; set; } //Персонаж
    } //Страница

    // ---------- Видео-блок (Теория) ----------
    public class VideoBlock : BlockBase
    {
        public string video_url { get; set; }
        public override int GetScore() => 1;
            
        private bool _isWatched;
        public override bool IsTimerActive => false;
        protected override bool IsTaskCompleted => _isWatched;
        public RelayCommand VideoEndedCommand => new RelayCommand(_ =>
        {
            _isWatched = true;
            OnPropertyChanged(nameof(CanContinue));
        });
        public void MarkAsWatched()
        {
            _isWatched = true;
            OnPropertyChanged(nameof(CanContinue)); 
        }
        public override void OnNext()
        {
            FinishBlock();
        }
    } //Форма

    // ---------- Сравнение (Практика) ----------
    public class MatchBlock : BlockBase, IDropTarget
    {
        public override bool IsTimerActive => true;
        public List<MatchPair> pairs { get; set; } = new();
        public ObservableCollection<MatchSlot> Slots { get; set; } = new();
        public ObservableCollection<DragCard> Cards { get; set; } = new();
        public override int GetScore() => Slots.Count(s => s.IsCorrect);
        protected override bool IsTaskCompleted => Slots.All(s => s.IsFilled);
        public override void OnNext()
        {
            FinishBlock();
        }
        public void Initialize()
        {
            Slots = new ObservableCollection<MatchSlot>(
           pairs.OrderBy(_ => Guid.NewGuid())
                .Select(p => new MatchSlot(p)));

            Cards = new ObservableCollection<DragCard>(
                pairs.OrderBy(_ => Guid.NewGuid())
                     .Select(p => new DragCard(p)));
        }
        public void DragOver(IDropInfo dropInfo) //Логика взятия
        {
            if (dropInfo.Data is DragCard)
                dropInfo.Effects = DragDropEffects.Move;
        }
        public void Drop(IDropInfo dropInfo) //Логика переноса
        {
            if (dropInfo.Data is not DragCard card)
                return;

            var targetSlot = (dropInfo.VisualTarget as FrameworkElement)?.DataContext as MatchSlot;
            var sourceSlot = Slots.FirstOrDefault(s => s.Cards.Contains(card));

            // 1. Удаляем карточку из исходного слота
            if (sourceSlot != null)
                sourceSlot.Cards.Remove(card);

            if (targetSlot != null)
            {
                // Если оба слота заняты и разные – обмен
                if (sourceSlot != null && targetSlot.Cards.Any() && sourceSlot != targetSlot)
                {
                    var existingCard = targetSlot.Cards.First();
                    targetSlot.Cards.Clear();
                    targetSlot.Cards.Add(card);      // перемещаем новую в целевой
                    sourceSlot.Cards.Add(existingCard); // старую помещаем в исходный
                }
                else
                {
                    // Стандартное поведение: если в целевом слоте уже есть ответ, возвращаем его вниз
                    if (targetSlot.Cards.Any())
                    {
                        var existing = targetSlot.Cards.First();
                        targetSlot.Cards.Clear();
                        if (!Cards.Contains(existing))
                            Cards.Add(existing);
                    }

                    // Удаляем карточку из нижнего списка, если она там
                    Cards.Remove(card);

                    // Помещаем перетаскиваемую карточку в целевой слот
                    targetSlot.Cards.Clear();
                    targetSlot.Cards.Add(card);
                    AppState.Instance.SoundService.PlaySound("select2");
                }
            }
            else
            {
                // Дроп на нижнюю панель – возвращаем карточку в общий список
                AppState.Instance.SoundService.PlaySound("select1");
                if (!Cards.Contains(card))
                     Cards.Add(card);
            }
            OnPropertyChanged(nameof(CanContinue));
        }
    } //Форма
    public class MatchContent
    {
        public string type { get; set; } = "text"; // "text" или "image"
        public string value { get; set; } = string.Empty; // текст или URL
    } //Тип объекта
    public class MatchPair
    {
        public int id { get; set; }
        public MatchContent target { get; set; } = new(); // верхний элемент (картинка/текст)
        public MatchContent source { get; set; } = new(); // нижняя карточка (картинка/текст)
    } //Пары
    public class MatchSlot : ViewModelBase
    {
        public MatchPair Pair { get; }
        public ObservableCollection<DragCard> Cards { get; } = new();
        public MatchContent Target => Pair.target; // что отображать в слоте (картинка/текст)
        public bool IsFilled => Cards.Any();
        public DragCard? Card => Cards.FirstOrDefault();
        public bool IsCorrect => Card?.Pair.id == Pair.id;

        public MatchSlot(MatchPair pair) => Pair = pair;
    } //Слот вверху
    public class DragCard
    {
        public MatchPair Pair { get; }
        public MatchContent Content => Pair.source; // Объект отображения (text/image)
        public DragCard(MatchPair pair) => Pair = pair;
    } //Слот внизу

    // ---------- Тест (Практика) ----------
    public class TestBlock : BlockBase
    {
        public List<TestQuestion> questions { get; set; }
        public override bool IsTimerActive => true;

        private int _currentIndex;
        private TestQuestion _currentQuestion;

        private bool _isLocked;
        public bool IsLocked
        {
            get => _isLocked;
            set { _isLocked = value; OnPropertyChanged(); }
        }
        public TestQuestion CurrentQuestion
        {
            get => _currentQuestion;
            set { _currentQuestion = value; OnPropertyChanged(); }
        }

        private int _correctAnswers;
        public override int GetScore() => _correctAnswers;

        protected override bool IsTaskCompleted =>
            _currentIndex >= questions.Count;
        public ICommand SelectAnswerCommand { get; }
        public TestBlock()
        {
            SelectAnswerCommand = new RelayCommand<TestAnswer>(OnAnswerSelected);
        }

        public void Initialize()
        {
            _currentIndex = 0;
            _correctAnswers = 0;
            SetQuestion(0);
        }
        private void SetQuestion(int index)
        {
            if (index >= questions.Count) return;
            var q = questions[index];
            // Перемешиваем ответы
            var rnd = new Random();
            q.answers = q.answers.OrderBy(_ => rnd.Next()).ToList();
            CurrentQuestion = q;
        }

        private async void OnAnswerSelected(TestAnswer answer)
        {
            if (IsLocked) return;
            AppState.Instance.SoundService.PlaySound("select1");
            IsLocked = true;

            // Блокируем все кнопки
            foreach (var a in CurrentQuestion.answers)
                a.IsEnabled = false;

            // Красим выбранную
            answer.Background = answer.is_correct
                ? Brushes.LightGreen
                : Brushes.IndianRed;

            if (answer.is_correct)
                _correctAnswers++;

            await Task.Delay(900);

            _currentIndex++;

            if (_currentIndex < questions.Count)
            {
                // Подготовка следующего вопроса
                foreach (var a in CurrentQuestion.answers)
                {
                    a.Background = Brushes.White;
                    a.IsEnabled = true;
                }

                SetQuestion(_currentIndex);
                IsLocked = false;
            }

            OnPropertyChanged(nameof(CanContinue));
        }

        public override void OnNext()
        {
            FinishBlock();
        }
    } //Форма
    public class TestQuestion
    {
        public string question { get; set; }
        public List<TestAnswer> answers { get; set; }
    } //Вопрос
    public class TestAnswer : ViewModelBase
    {
        public string text { get; set; }
        public bool is_correct { get; set; }
        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set { _isEnabled = value; OnPropertyChanged(); }
        }

        private Brush _background = Brushes.White;
        public Brush Background
        {
            get => _background;
            set { _background = value; OnPropertyChanged(); }
        }
    } //Ответ

    // ---------- Результат ----------
    public class ResultBlock : BlockBase
    {
        public int Score { get; }
        public string Time { get; }
        public string ScoreImage { get; }

        public ObservableCollection<Achievement>
            Achievements
        { get; }

        public ResultBlock(
            int score,
            string time,
            string scoreImage,
            IEnumerable<Achievement> achievements)
        {
            Score = score;
            Time = time;
            ScoreImage = scoreImage;
            Achievements =
                new ObservableCollection<Achievement>(
                    achievements);
        }
        public override bool IsTimerActive => false;

        public override int GetScore() => 0;

        protected override bool IsTaskCompleted => true;
    } //Форма
}
