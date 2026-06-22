using PonyvilleSchool2._0.Core;
using PonyvilleSchool2._0.Services;
using System.Text.RegularExpressions;
using System.Windows;

namespace PonyvilleSchool2._0.ViewModels.Authentication
{
    public class RegisterViewModel : AuthViewModelBase
    {
        private readonly StartViewModel _startVM;

        private bool _isNotLoading = true;
        private string _token = "";
        private bool _isVerify = true;
        private bool _isTokenSended = false;
        public string Login { get; set; } = ""; //Значение логина для регистрации
        public string Name { get; set; } = ""; //Значение имени для регистрации
        public string Password { get; set; } = ""; //Значение пароля для регистрации
        public string ConfirmPassword { get; set; } = ""; //Повтор пароля
        public bool IsTokenSended
        {
            get => _isTokenSended;
            set
            {
                _isTokenSended = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanConfirm));
            }
        } //Отправка токена
        public string Token
        {
            get => _token;
            set
            {
                _token = value;
                OnPropertyChanged(nameof(CanConfirm));
            }
        }  //Значение токена
        public new bool IsNotLoading
        {
            get => _isNotLoading;
            set
            {
                _isNotLoading = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(CanConfirm));
            }
        } //Загрузка после нажатия
        public bool CanConfirm
        {
            get
            {
                if (IsNotLoading && !IsTokenSended)
                {
                    return true;
                }
                else if (IsNotLoading && IsTokenSended && !string.IsNullOrEmpty(Token) && Token.Length == 6)
                {
                    {
                        return true;
                    }
                }
                else return false;
            }
        } //Возможность продолжить
        public bool IsVerify
        {
            get => _isVerify;
            set
            {
                _isVerify = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanConfirm));
            }
        } //Отправка токена для подтверждения регистрации

        public RelayCommand RegisterCommand { get; } //Обработчик регистрации
        public RelayCommand GoToLoginCommand { get; } //Обработчик возврата к авторизации

        public RegisterViewModel(StartViewModel startVM)
        {
            _startVM = startVM;

            RegisterCommand = new RelayCommand(_ =>
            {
                if (!IsTokenSended)
                {
                    RegisterUser();
                }
                else
                {
                    ConfirmEmail();
                }
            });
            GoToLoginCommand = new RelayCommand(_ => 
            {
                if (!IsTokenSended)
                {
                    _startVM.ShowLogin();
                }
                else
                {
                    var result = MessageBoxHelper.Show(
                        "Отменить регистрацию?",
                        "Отмена",
                        MessageBoxViewModel.Images.Question,
                        MessageBoxViewModel.Buttons.YesNo);

                    if (result == MessageBoxViewModel.Result.Yes)
                    {
                        DeleteAccount();
                    }
                }
            });
        } //Конструктор
        public async void RegisterUser() //Регистрация пользователя
        {
            if (string.IsNullOrWhiteSpace(Login) ||
                string.IsNullOrWhiteSpace(Name) ||
                string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ShowError("Пожалуйста, заполните все поля");
                return;
            }
            if (Password.Length < 8)
            {
                ShowError("Ваш пароль не может быть короче 8-и символов");
                return;
            }
            if (Password != ConfirmPassword)
            {
                ShowError("Кажется, пароли не совпадают");
                return;
            }
            if (!IsValidEmail(Login))
            {
                ShowError("Пожалуйста, введите корректную почту");
                return;
            }
            if (!IsValidName(Name))
            {
                ShowError("Пожалуйста, введите разрешенное имя");
                return;
            }

            try
            {
                IsNotLoading = false;
                string generatedToken = new Random() //Генерация токена для подтверждения
                    .Next(100000, 999999)
                    .ToString();
                string hash =
                    SecurityService.Hash(Password);

                bool success = await AppState.Instance.Supabase.RegisterUser(Login, hash, Name, generatedToken);
                if (!success)
                {
                    ShowError("Кажется, такой пользователь уже есть!");
                    return;
                }
                await AppState.Instance.Email
                    .SendTokenAsync(
                        Login,
                        generatedToken,
                        "email_verification");

                ShowSuccess("На вашу почту отправлен код подтверждения");
                IsVerify = false;
                IsTokenSended = true;
            }
            finally
            {
                IsNotLoading = true;
            }
        }
        private async void ConfirmEmail()
        {
            try
            {
                if (Token.Length <= 5)
                {
                    MessageBox.Show("Пожалуйста, введите шестизначный код из письма");
                    return;
                }

                IsNotLoading = false;
                bool valid =
                    await AppState.Instance.Supabase
                        .VerifyToken(
                            Login,
                            Token,
                            "email_verification");

                if (!valid)
                {
                    MessageBox.Show(
                        "Неверный или устаревший код");

                    return;
                }

                MessageBoxHelper.Show("Аккаунт успешно сохранен!", "Успех");
                _startVM.ShowLogin(Login, Password);
            }
            finally
            {
                IsNotLoading = true;
            }
        }   //Подтверждение E-Mail пользователя
        private bool IsValidEmail(string email)
        {
            string pattern =
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

            return Regex.IsMatch(email, pattern);
        }  //Проверка валидности почты
        private bool IsValidName(string name)
        {
            if (BannedWords.IsNameBanned(name))
            {
                return false;
            }
            return true;
        } //Проверка валидности имени
        private async void DeleteAccount()
        {
            bool success = await AppState.Instance.Supabase.DeleteAccount(Login);
            if (!success)
            {
                ShowError("Произошла неизвестная ошибка!");
                return;
            }
            Clear();
            IsTokenSended = false;
            IsVerify = true;
            _startVM.ShowLogin();
        } //Удаление аккаунта
        public void Clear()
        {
            Login = "";
            Password = "";
            Name = "";
            ConfirmPassword = "";
        } //Отчистка полей
    }
}
