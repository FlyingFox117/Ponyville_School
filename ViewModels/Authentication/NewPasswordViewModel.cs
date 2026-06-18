using PonyvilleSchool2._0.Core;
using PonyvilleSchool2._0.Services;
using System.Windows;

namespace PonyvilleSchool2._0.ViewModels.Authentication
{
    class NewPasswordViewModel : AuthViewModelBase
    {
        private readonly StartViewModel _startVM;

        private string Login { get; set; } = ""; //Значение логина
        public string Password { get; set; } = ""; //Значение пароля
        public string ConfirmPassword { get; set; } = ""; //Значение повтора пароля

        public RelayCommand ConfirmCommand { get; } //Обработчик подтверждения токена
        public RelayCommand GoToLoginCommand { get; } //Обработчик возврата к авторизации

        public NewPasswordViewModel(StartViewModel startVM, string login) //Конструктор
        {
            _startVM = startVM;
            Login = login;

            GoToLoginCommand = new RelayCommand(_ =>
            {
                var result = MessageBoxHelper.Show("Отменить изменение пароля?", "Вопрос", MessageBoxViewModel.Images.Warning, MessageBoxViewModel.Buttons.YesNo);

                if (result == MessageBoxViewModel.Result.Yes)
                {
                    _startVM.ShowLogin();
                }
            });
            ConfirmCommand = new RelayCommand(_ => SaveNewPassword());
        }
        private async void SaveNewPassword() //Сохранение нового пароля в БД
        {
            if (string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ShowError("Заполните все поля");
                return;
            }

            if (Password.Length < 8)
            {
                ShowError("Пароль должен быть минимум 8 символов");
                return;
            }

            if (Password != ConfirmPassword)
            {
                ShowError("Пароли не совпадают");
                return;
            }

            try
            {
                IsNotLoading = false;
                string password =
                    SecurityService.Hash(Password);

                bool success = await AppState.Instance.Supabase.ChangePassword(Login, password);

                if (!success)
                {
                    ShowError("Произошла непредвиденная ошибка");
                    return;
                }

                MessageBoxHelper.Show("Пароль успешно именён!", "Успех", MessageBoxViewModel.Images.Save);
                _startVM.ShowLogin(Login, Password);
            }
            finally
            {
                IsNotLoading = true;
            }
        }
    }
}
