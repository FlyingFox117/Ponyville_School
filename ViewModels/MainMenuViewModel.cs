using PonyvilleSchool2._0.Core;

namespace PonyvilleSchool2._0.ViewModels
{
    public class MainMenuViewModel : ViewModelBase
    {
        public HubViewModel HubVM { get; }
        public ProfileViewModel ProfileVM { get; }

        private object _currentPage;
        public object CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged();
            }
        }

        public MainMenuViewModel()
        {
            HubVM = new HubViewModel(OpenProfile);
            ProfileVM = new ProfileViewModel(OpenHub);

            CurrentPage = HubVM;
        } //Конструктор

        public void OpenProfile()
        {
            CurrentPage = ProfileVM;
        } //Открытие профиля
        public void OpenHub()
        {
            CurrentPage = HubVM;
        } //Открытие главного меню
    }
}
