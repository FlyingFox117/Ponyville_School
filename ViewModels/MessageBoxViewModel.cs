using PonyvilleSchool2._0.Core;
using System.Windows;
using System.Windows.Input;

namespace PonyvilleSchool2._0.ViewModels
{
    public class MessageBoxViewModel : ViewModelBase
    {
        //Кнопки окна
        public enum Buttons
        {
            OK,
            YesNo,
            Input
        }

        //Картинка окна
        public enum Images
        {
            Info,
            Save,
            Question,
            Warning,
            Error
        }

        //Результат диалога
        public enum Result
        {
            OK,
            Yes,
            No,
            Cancel
        }

        //Закрытие окна после взаимодействия
        private readonly Action<Result, string> _closeAction;

        //Значения окна
        public string Message { get; }
        public string Title { get; }
        public string ImagePath { get; }

        public Visibility OkVisibility { get; }
        public Visibility YesNoVisibility { get; }
        public Visibility IsInputVisible { get; }
        public Visibility IsOkCancelVisible { get; }

        //Обработчики взаимодействий
        public RelayCommand OkCommand { get; } //OK
        public RelayCommand YesCommand { get; } //Да
        public RelayCommand NoCommand { get; } //Нет
        public RelayCommand CancelCommand { get; } //Отмена

        private string _inputText;
        public string InputText
        {
            get => _inputText;
            set
            {
                if (_inputText != value)
                {
                    _inputText = value;
                    OnPropertyChanged(nameof(InputText));
                }
            }
        } //Текст внутри поля

        public MessageBoxViewModel(
            string message,
            string title,
            string imagePath,
            Buttons buttons,
            Action<Result, string> closeAction)
        {
            Message = message;
            Title = title;
            ImagePath = imagePath;
            _closeAction = closeAction;

            OkCommand = new RelayCommand(_ => _closeAction(MessageBoxViewModel.Result.OK, InputText));
            YesCommand = new RelayCommand(_ => _closeAction(MessageBoxViewModel.Result.Yes, InputText));
            NoCommand = new RelayCommand(_ => _closeAction(MessageBoxViewModel.Result.No, null));
            CancelCommand = new RelayCommand(_ => _closeAction(MessageBoxViewModel.Result.Cancel, null));

            OkVisibility = buttons == Buttons.OK ? Visibility.Visible : Visibility.Collapsed;
            YesNoVisibility = buttons == Buttons.YesNo ? Visibility.Visible : Visibility.Collapsed;
            IsInputVisible = (buttons == Buttons.Input) ? Visibility.Visible : Visibility.Collapsed;
            IsOkCancelVisible = (buttons == Buttons.Input) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
