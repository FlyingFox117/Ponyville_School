using PonyvilleSchool2._0.ViewModels;
using PonyvilleSchool2._0.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static PonyvilleSchool2._0.ViewModels.MessageBoxViewModel;

namespace PonyvilleSchool2._0.Services
{
    public static class MessageBoxHelper
    {
        //Обычный вызов (без поля ввода)
        public static MessageBoxViewModel.Result Show(
            string message,
            string title,
            Images image = Images.Info,
            Buttons buttons = Buttons.OK,
            Window owner = null)
        {
            string imagePath = GetImagePath(image);
            MessageBoxViewModel.Result result = MessageBoxViewModel.Result.Cancel;

            // Создаём окно
            var window = new PonyMessage(null);
            window.Owner = owner ?? Application.Current.MainWindow;

            // Создаём VM с делегатом, который ссылается на window
            var vm = new MessageBoxViewModel(
                message,
                title,
                imagePath,
                buttons,
                (r, text) =>
                {
                    result = r;
                    window.DialogResult = (r == MessageBoxViewModel.Result.OK || r == MessageBoxViewModel.Result.Yes);
                    window.Close();
                });
            window.DataContext = vm;

            //Воспроизведение звука
            switch (image)
            {
                case MessageBoxViewModel.Images.Info:
                    AppState.Instance.SoundService.PlaySound("select2");
                    break;
                case MessageBoxViewModel.Images.Warning:
                    AppState.Instance.SoundService.PlaySound("select1");
                    break;
                case MessageBoxViewModel.Images.Question:
                    AppState.Instance.SoundService.PlaySound("question");
                    break;
                case MessageBoxViewModel.Images.Save:
                    AppState.Instance.SoundService.PlaySound("continue");
                    break;
                default:
                    AppState.Instance.SoundService.PlaySound("select2");
                    break;
            }

            window.ShowDialog();
            return result;
        }

        public static MessageBoxInputResult ShowInput(
        string message,
        string title,
        string defaultText = "",
        Images image = Images.Info,
        Window owner = null)
        {
            string imagePath = GetImagePath(image);
            var result = new MessageBoxInputResult { Result = Result.Cancel, InputText = defaultText };

            var window = new PonyMessage(null);
            window.Owner = owner ?? Application.Current.MainWindow;

            var vm = new MessageBoxViewModel(
                message,
                title,
                imagePath,
                Buttons.Input,
                (r, text) => 
                {
                    result.Result = r;
                    result.InputText = text;
                    window.DialogResult = (r == Result.OK || r == Result.Yes);
                    window.Close();
                });
            vm.InputText = defaultText;
            window.DataContext = vm;

            AppState.Instance.SoundService.PlaySound("question");

            window.ShowDialog();
            return result;
        }

        private static string GetImagePath(MessageBoxViewModel.Images image)
        {
            string baseUri = "pack://application:,,,/Assets/Messages/";
            switch (image)
            {
                case MessageBoxViewModel.Images.Info: return baseUri + "info.png";
                case MessageBoxViewModel.Images.Save: return baseUri + "saved.png";
                case MessageBoxViewModel.Images.Question: return baseUri + "info.png";
                case MessageBoxViewModel.Images.Warning: return baseUri + "question.png";
                case MessageBoxViewModel.Images.Error: return baseUri + "warning.png";
                default: return null;
            }
        }
        public struct MessageBoxInputResult
        {
            public MessageBoxViewModel.Result Result { get; set; }
            public string InputText { get; set; }
        }
    }
}
