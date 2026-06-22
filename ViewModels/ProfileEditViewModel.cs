using PonyvilleSchool2._0.Core;
using PonyvilleSchool2._0.Services;
using System.Threading.Tasks;

namespace PonyvilleSchool2._0.ViewModels
{
    public class ProfileEditViewModel
    {
        private ProfileViewModel _main; //Родительская VM

        public string Username { get; set; } = AppState.Instance.CurrentUser.name; //Значение имени для регистрации
        public string Password { get; set; } = ""; //Значение пароля для регистрации
        public string ConfirmPassword { get; set; } = ""; //Повтор пароля

        public RelayCommand BackCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand DeleteAccountCommand { get; }
        public ProfileEditViewModel(ProfileViewModel profileViewModel)
        {
            _main = profileViewModel;
            BackCommand = new RelayCommand(_ => _main.ToProfile());
            SaveCommand = new RelayCommand(_ => SaveChanges());
            DeleteAccountCommand = new RelayCommand(_ => DeleteAccount());
        }

        private async Task SaveChanges()
        {
            if (string.IsNullOrEmpty(Username))
            {
                MessageBoxHelper.Show("Введите новое имя", "Ошибка сохранения", MessageBoxViewModel.Images.Warning);
                return;
            }
            if ((Password != ConfirmPassword))
            {
                MessageBoxHelper.Show("Пароли не совпадают", "Ошибка сохранения", MessageBoxViewModel.Images.Warning);
                return;
            }
            string password_hash = null;
            int? user_id = AppState.Instance.CurrentUser.id;
            if (!string.IsNullOrEmpty(Password))
            {
                password_hash = SecurityService.Hash(Password);
            }
            var result = MessageBoxHelper.Show("Сохранить изменения?", "Сохранение", MessageBoxViewModel.Images.Save, MessageBoxViewModel.Buttons.YesNo);

            if (result == MessageBoxViewModel.Result.Yes)
            {
                await AppState.Instance.Supabase.UpdateUserProfile(user_id, Username, password_hash);
                AppState.Instance.CurrentUser.name = Username;
                _main.ToProfile();
            }
        } //Сохранение изменений
        private async void DeleteAccount()
        {
            int? user_id = AppState.Instance.CurrentUser.id;
            var result = MessageBoxHelper.Show("Вы действительно хотите удалить аккаунт? Вы сможете восстановить его в течении 30 дней", "Удаление", MessageBoxViewModel.Images.Warning, MessageBoxViewModel.Buttons.YesNo);
            if (result == MessageBoxViewModel.Result.Yes)
            {
                bool deleted = await AppState.Instance.Supabase.DeleteAccountById(user_id);
                if (deleted)
                {
                    MessageBoxHelper.Show("Аккаунт удален", "Успешно", MessageBoxViewModel.Images.Save);
                    _main.Logout(true);
                }
                else
                {
                    MessageBoxHelper.Show("Ошибка удаления аккаунта", "Ошибка удаления", MessageBoxViewModel.Images.Error);
                    _main.ToProfile();
                }
            }
        } //Удаление аккаунта
    }
}
