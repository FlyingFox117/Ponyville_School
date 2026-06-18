using PonyvilleSchool2._0.Core;
using PonyvilleSchool2._0.Services;
using PonyvilleSchool2._0.Views;
using PonyvilleSchool2._0.Views.Authentication;
using System.IO;
using System.Windows;

namespace PonyvilleSchool2._0.ViewModels.Authentication
{
    public class LoginViewModel : AuthViewModelBase
    {
        private readonly StartViewModel _startVM;

        private string _password = "";
        private bool _isPasswordVisible = false;
        public string Login { get; set; } = ""; //Значение логина
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        } //Значение пароля
        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set
            {
                _isPasswordVisible = value;
                OnPropertyChanged();
            }
        } //Отображение/Скрытие пароля
        public bool SaveLogin { get; set; } = false; //Подтверждение сохранения входа
        public RelayCommand ResetPassword { get; } //Обработчик перехода к сбросу пароля
        public RelayCommand LoginCommand { get; } //Обработчик авторизации
        public RelayCommand GoToRegisterCommand { get; } //Обработчик перехода к регистрации
        public RelayCommand TogglePasswordVisibilityCommand { get; } //Показ и скрытие пароля

        public LoginViewModel(StartViewModel startVM) //Конструктор
        {
            _startVM = startVM;

            LoginCommand = new RelayCommand(_ => LoginUser());
            GoToRegisterCommand = new RelayCommand(_ => _startVM.ShowRegister());
            ResetPassword = new RelayCommand(_ => _startVM.ShowPasswordRestore(Login));
            TogglePasswordVisibilityCommand = new RelayCommand(_ =>
            {
                IsPasswordVisible = !IsPasswordVisible;
            });
        }
        private async void LoginUser() //Авторизация пользователя
        {
            if (string.IsNullOrWhiteSpace(Login) ||
                string.IsNullOrWhiteSpace(Password))
            {
                ShowError("Заполните поля для входа");
                return;
            }

            try
            {
                IsNotLoading = false;
                var user = await AppState.Instance.Supabase.AuthenticateUser(Login, Password);
                switch (user)
                {
                    case SupabaseClient.AuthResult.Failed:
                        {
                            ShowError("Неверный логин или пароль");
                            break;
                        }
                    case SupabaseClient.AuthResult.NeedVerification:
                        {
                            var result = MessageBoxHelper.Show("Кажется, ваш аккаунт ещё не активирован. Хотите активировать сейчас?", "Активация аккаунта", MessageBoxViewModel.Images.Info, MessageBoxViewModel.Buttons.YesNo);
                            if (result == MessageBoxViewModel.Result.Yes)
                            {
                                _startVM.ShowRegister(Login, Password);
                            }
                            break;
                        }
                    case SupabaseClient.AuthResult.Success:
                        {
                            if (AppState.Instance.CurrentUser.role != null && AppState.Instance.CurrentUser.role == "administrator")
                            {
                                AdministratorAuth();
                                return;
                            } //Вход, если администратор

                            if (SaveLogin)
                            {
                                SaveToken(); //Сохранение токена для быстрой авторизации
                            }
                            //Новое окно программы становится главное меню,
                            //авторизация полностью уходит

                            var main = new MainMenuWindow(); //Запуск главного окна
                            main.Show();
                            Application.Current.MainWindow = main;

                            Application.Current.Windows //Закрытие окна авторизации
                                .OfType<StartWindow>()
                                .FirstOrDefault()?
                                .Close();
                            break;
                        }
                }
                
            }
            catch
            {
                ShowError("Неверный логин или пароль");
                return;
            }
            finally
            {
                IsNotLoading = true;
            }
        }
        private async void SaveToken() //Сохранение токена для авторизации
        {
            Guid token = Guid.NewGuid(); //Создание токена
            string tokenhash = SecurityService.HashToken(token.ToString()); //Шифрование токена

            await AppState.Instance.Supabase.CreateToken(tokenhash, AppState.Instance.CurrentUser.id); //Сохранение токена в БД

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string folder = Path.Combine(appData, "Ponyville School");
            Directory.CreateDirectory(folder);

            string encryptedToken = SecurityService.Encrypt(token.ToString()); //Шифрование токена через DPAPI

            File.WriteAllText(Path.Combine(folder, "session.dat"), encryptedToken); //Сохранение токена локально
        }
        private void AdministratorAuth()
        {
            //Ниже - проверка, если программа учебной версии и не имеет администратора
            var adminWindowType = Type.GetType("PonyvilleSchool2._0.Views.Administrator.AdminWindow");

            if (adminWindowType != null)
            {
                MessageBoxHelper.Show($"Вы вошли в систему как администратор", "Администратор", MessageBoxViewModel.Images.Info);

                var admin = new Views.Administrator.AdminWindow();
                admin.Show();
                Application.Current.MainWindow = admin;

                Application.Current.Windows //Закрытие окна авторизации
                    .OfType<StartWindow>()
                    .FirstOrDefault()?
                    .Close();
                return;
            }
            else
            {
                ShowError("Неверный логин или пароль!");
            }
        } //Вход в режим администратора
    }
}
