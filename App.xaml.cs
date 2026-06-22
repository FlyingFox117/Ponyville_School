using PonyvilleSchool2._0.Services;
using PonyvilleSchool2._0.Services.Sounds;
using PonyvilleSchool2._0.Views;
using PonyvilleSchool2._0.Views.Authentication;
using System.IO;
using System.Windows;

namespace PonyvilleSchool2._0
{
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Logger.Initialize();
            var settings = SettingsService.Load();
            VoicePlayerService.IsEnabled = settings.SoundEnabled;

            var result = await TryAutoLogin(); //Проверка входа
            if (!result)
            {
                var start = new StartWindow();
                start.Show();
            }
            else
            {
                var start = new MainMenuWindow();
                start.Show();
            }
        } //Запуск программы
        private static async Task<bool> TryAutoLogin()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string folder = Path.Combine(appData, "Ponyville School");
            string tokenPath = Path.Combine(folder, "session.dat");

            if (!File.Exists(tokenPath))
            {
                return false;
            }

            string encryptedToken = File.ReadAllText(tokenPath);
            string token = SecurityService.Decrypt(encryptedToken);

            string tokenhash = SecurityService.HashToken(token);

            bool valid = await AppState.Instance.Supabase.CheckToken(tokenhash);

            return valid;
        } //Вход без пароля по токену
    }

}
