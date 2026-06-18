using AutoUpdaterDotNET;
using PonyvilleSchool2._0.Core;
using PonyvilleSchool2._0.Services;
using System.Windows.Threading;

namespace PonyvilleSchool2._0.ViewModels.Authentication
{
    public class StartViewModel : ViewModelBase
    {
        //Состояния окон для хранения значений
        private object _currentView;
        private string _currentImage = "/Assets/Authentication/friendship2.png";
        private string _currentWindow = "Авторизация";
        private int _currentImageIndex = 0;

        private readonly DispatcherTimer _imageTimer;
        private readonly List<string> _imageList = new List<string> //Массив для переключения изображений
        {
            "/Assets/Authentication/friendship2.png",
            "/Assets/Authentication/friendship.png"
        };
        public string CurrentWindow 
        { 
            get => _currentWindow; 
            set
            {
                _currentWindow = value;
                OnPropertyChanged();
            }
        } //Текущее название окна
        public string CurrentImage
        {
            get => _currentImage;
            set
            {
                _currentImage = value;
                OnPropertyChanged();
            }
        } //Текущее изображение слева

        private LoginViewModel _loginVM;
        private RegisterViewModel _registerVM;
        private PasswordRestoreViewModel _passwordVM;
        public object CurrentView //Текущее окно (_loginVM, _registerVM, _passwordVM)
        {
            get => _currentView;
            set 
            { 
                _currentView = value; 
                OnPropertyChanged();
            }
        }

        public StartViewModel() //Конструктор
        {
            ShowLogin();
            _imageTimer = new DispatcherTimer();
            _imageTimer.Interval = TimeSpan.FromSeconds(8);
            _imageTimer.Tick += OnImageTimerTick;
            _imageTimer.Start();
            CheckUpdates();
        }
        public void ShowLogin(string? login = null, string? password = null) //Переключение на вид авторизации
        {
            _loginVM ??= new LoginViewModel(this);
            if (login != null)
            {
                _loginVM.Login = login;
                _loginVM.Password = password ?? "";
            }
            CurrentView = _loginVM ??= new LoginViewModel(this);
            CurrentWindow = "Авторизация";
        }
        public void ShowRegister(string? login = null, string? password = null) //Переключение на вид регистрации
        {
            CurrentView = _registerVM ??= new RegisterViewModel(this);
            string name = AppState.Instance.CurrentUser.name;
            if (!string.IsNullOrEmpty(name))
            {
                _registerVM.Login = login;
                _registerVM.Password = password;
                _registerVM.Name = name;
                _registerVM.ConfirmPassword = password;
                _registerVM.RegisterUser();
            }
            CurrentWindow = "Регистрация";
        }
        public void ShowPasswordRestore(string login) //Переключение на восстановление пароля
        {
            CurrentView = _passwordVM ??= new PasswordRestoreViewModel(this, login);
            CurrentWindow = "Сброс пароля";
        }
        public void ShowNewPassword(string login) //Переключение на изменение пароля
        {
            CurrentView = new NewPasswordViewModel(this, login);
            CurrentWindow = "Сброс пароля";
        }
        private void OnImageTimerTick(object sender, EventArgs e) //Переключение картинок по таймеру
        {
            if (_imageList.Count <= 1)
                { return; }
            _currentImageIndex = (_currentImageIndex + 1) % _imageList.Count;
            CurrentImage = _imageList[_currentImageIndex];
        }

        private void CheckUpdates() //Проверка обновлений в программе
        {
            AutoUpdater.Start(
                "https://raw.githubusercontent.com/FlyingFox117/Ponyville_School/main/update.xml");
        }
    }
}
