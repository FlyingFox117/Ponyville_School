using PonyvilleSchool2._0.Core;
using PonyvilleSchool2._0.Services;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace PonyvilleSchool2._0.ViewModels.Authentication
{
    internal class PasswordRestoreViewModel : AuthViewModelBase
    {
        private StartViewModel _startVM;

        private bool _isTokenSended = false;
        private bool _isNotLoading = true;
        private string _token = "";
        private string _login = "";
        private DispatcherTimer _timer;
        private int _secondsLeft;
        public string Login 
        { 
            get => _login;
            set 
            {
                _login = value;
                OnPropertyChanged(nameof(CanConfirm));
            }
        } //Значение логина
        public string Token
        {
            get => _token;
            set
            {
                _token = value;
                OnPropertyChanged(nameof(CanConfirm));
            }
        }  //Значение токена
        public string EnteredLogin { get; set; } = ""; //Значение введенного логина после отправки кода
        public bool IsTokenSended
        {
            get => _isTokenSended;
            set { _isTokenSended = value; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanConfirm));
                OnPropertyChanged(nameof(CanSendAgain));
            }
        } //Отправка токена
        public new bool IsNotLoading
        {
            get => _isNotLoading;
            set
            {
                _isNotLoading = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(CanConfirm));
                OnPropertyChanged(nameof(CanSendAgain));
            }
        } //Загрузка после нажатия
        public bool CanConfirm
        {
            get
            {
                if (!IsNotLoading)
                {
                    return false;
                }
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
                else if (EnteredLogin != Login)
                {
                    return true;
                }
                else return false;
            }
        } //Возможность продолжить
        public bool CanSendAgain =>
    IsNotLoading &&
    IsTokenSended &&
    SecondsLeft == 0; //Возможность отправить снова
        public int SecondsLeft
        {
            get => _secondsLeft;
            set
            {
                _secondsLeft = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSendAgain));
                OnPropertyChanged(nameof(ResendButtonText));
            }
        } //Осталось времени до повторной отправки
        public string ResendButtonText
        {
            get
            {
                if (SecondsLeft > 0)
                {
                    return
                        $"Отправить снова через {SecondsLeft} сек";
                }

                return "Отправить код повторно";
            }
        } //Текст кнопки отправки кода

        public RelayCommand ContinueCommand { get; } //Обработчик подтверждения токена
        public RelayCommand GoToLoginCommand { get; } //Обработчик возврата к авторизации
        public RelayCommand ResendTokenCommand { get; } //Обработчик отправки токена

        public PasswordRestoreViewModel(StartViewModel startVM, string login) //Конструктор
        {
            _startVM = startVM;
            Login = login;
            ContinueCommand = new RelayCommand(_ =>
            {
                if (!IsTokenSended)
                {
                    SendToken();
                }
                else if (EnteredLogin != Login && CanSendAgain)
                {
                    SendToken();
                }
                else
                {
                    VerifyToken();
                }
            }
            );
            GoToLoginCommand = new RelayCommand(_ => _startVM.ShowLogin());
            ResendTokenCommand = new RelayCommand(_ => SendToken());
        }
        private async void SendToken() //Отправка токена
        {
            if (string.IsNullOrEmpty(Login))
            {
                ShowError("Введите почту, чтобы выслать код");
                return;
            }
            if (!IsValidEmail(Login))
            {
                ShowError("Пожалуйста, введите корректную почту");
                return;
            }
            try
            {
                IsNotLoading = false;
                string generatedToken = new Random() //Генерация токена
                    .Next(100000, 999999)
                    .ToString();

                var result = //Сохранение токена в базе данных
                    await AppState.Instance.Supabase
                        .CreateToken(
                            Login,
                            generatedToken,
                            "password_reset");

                if (result == null || !result.success) //Отправка токена на почту
                {
                    AppState.Instance.SoundService.PlaySound("select1");
                    ShowError("Аккаунт с указанной почтой не найден");
                    return;
                }

                IsTokenSended = false;

                await AppState.Instance.Email
                    .SendTokenAsync(
                        result.user_login,
                        generatedToken,
                        "password_reset");

                ShowSuccess("На указанную почту отправлен код подтверждения");

                StartCooldown(); //Запуск отсчёта
                IsTokenSended = true;
                EnteredLogin = Login;
            }
            catch (Exception ex)
            {
                MessageBoxHelper.Show(ex.ToString(), "Ошибка!");
            }
            finally
            {
                IsNotLoading = true;
            }
        }
        private async void VerifyToken() //Подтверждение токена
        {
            if (Token.Length <= 5)
            {
                ShowError("Пожалуйста, введите шестизначный код из письма");
                return;
            }
            try
            {
                IsNotLoading = false;
                bool valid =
                    await AppState.Instance.Supabase
                        .VerifyToken(
                            Login,
                            Token,
                            "password_reset");

                if (!valid)
                {
                    AppState.Instance.SoundService.PlaySound("select1");
                    ShowError("Неверный или устаревший код");
                    return;
                }

                ShowSuccess("Успешно!");

                _startVM.ShowNewPassword(Login);
            }
            catch (Exception ex)
            {
                MessageBoxHelper.Show(ex.ToString(), "Ошибка!");
            }
            finally
            {
                IsNotLoading = true;
            }
        }
        private void StartCooldown()
        {
            SecondsLeft = 60;

            _timer = new DispatcherTimer();

            _timer.Interval =
                TimeSpan.FromSeconds(1);

            _timer.Tick += (s, e) =>
            {
                SecondsLeft--;

                if (SecondsLeft <= 0)
                {
                    _timer.Stop();
                }
            };

            _timer.Start();
        } //Запуск отсчёта до повторной отправки
        private bool IsValidEmail(string email) //Проверка валидности почты
        {
            string pattern =
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

            return Regex.IsMatch(email, pattern);
        }
    }
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return (bool)value
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return value is Visibility visibility &&
                   visibility == Visibility.Visible;
        }
    } //Конвертер видимости объектов на форме
}
