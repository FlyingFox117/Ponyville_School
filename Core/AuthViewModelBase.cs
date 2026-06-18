using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PonyvilleSchool2._0.Core
{
    public class AuthViewModelBase : ViewModelBase
    {
        private string _statusMessage = "";
        private Brush _statusColor = Brushes.Red;
        private bool _isNotLoading = true;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        } //Значение уведомления
        public Brush StatusColor
        {
            get => _statusColor;
            set
            {
                _statusColor = value;
                OnPropertyChanged();
            }
        } //Цвет уведомления
        public bool IsNotLoading
        {
            get => _isNotLoading;
            set
            {
                _isNotLoading = value;
                OnPropertyChanged();
            }
        } //Статус загрузки

        protected void ShowError(string text)
        {
            StatusMessage = text;
            StatusColor = Brushes.Red;
            ClearStatus();
        }

        protected void ShowSuccess(string text)
        {
            StatusMessage = text;
            StatusColor = Brushes.Green;         
            ClearStatus();
        }

        protected async void ClearStatus()
        {
            await Task.Delay(6000);
            StatusMessage = "";
        }
    }
}
