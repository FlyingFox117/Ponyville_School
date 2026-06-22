using AutoUpdaterDotNET;
using PonyvilleSchool2._0.Core;
using PonyvilleSchool2._0.Services;
using PonyvilleSchool2._0.Services.Sounds;

namespace PonyvilleSchool2._0.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private ProfileViewModel _main; //Родительское окно

        private bool _isSoundEnabled;
        public bool IsSoundEnabled
        {
            get => _isSoundEnabled;
            set
            {
                if (SetProperty(ref _isSoundEnabled, value))
                {
                    // сразу применяем в сервис
                    VoicePlayerService.IsEnabled = value;
                    // сохраняем в файл
                    var settings = SettingsService.Load();
                    settings.SoundEnabled = value;
                    SettingsService.Save(settings);
                }
            }
        }

        public RelayCommand BackCommand { get; }
        public RelayCommand ShowInfo { get; }
        public RelayCommand CheckUpdates { get; }

        public SettingsViewModel(ProfileViewModel profileViewModel)
        {
            _main = profileViewModel;
            // загружаем сохранённое значение
            var settings = SettingsService.Load();
            _isSoundEnabled = settings.SoundEnabled;

            BackCommand = new RelayCommand(_ =>
            {
                AppState.Instance.SoundService.PlaySound("select1"); // если звук включён
                _main.ToProfile();
            });
            ShowInfo = new RelayCommand(_ => InfoBox());
            CheckUpdates = new RelayCommand(_ => Updates());
        }
        private void Updates()
        {
            AutoUpdater.Start(
                "https://raw.githubusercontent.com/FlyingFox117/Ponyville_School/master/update.xml");
        }

        private void InfoBox()
        {
            MessageBoxHelper.Show("Версия программы: 2.2.0.\n" +
                "Разработчик: Сыркин Роман, ЭТИ СГТУ.\n" +
                "Программа является дипломным проектом.\n" +
                "Энгельс, 2026\n", "Информация", MessageBoxViewModel.Images.Save);
        }
    }
}
