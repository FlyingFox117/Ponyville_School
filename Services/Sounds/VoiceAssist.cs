using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PonyvilleSchool2._0.Services.Sounds
{
    public static class VoiceAssist
    {
        public static readonly DependencyProperty PhraseProperty =
            DependencyProperty.RegisterAttached(
                "Phrase",
                typeof(string),
                typeof(VoiceAssist),
                new PropertyMetadata(null, OnPhraseChanged));

        public static string GetPhrase(DependencyObject obj)
            => (string)obj.GetValue(PhraseProperty);

        public static void SetPhrase(DependencyObject obj, string value)
            => obj.SetValue(PhraseProperty, value);

        private static void OnPhraseChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is not FrameworkElement element)
                return;

            element.MouseEnter -= VoiceMouseEnter;
            element.MouseLeave -= VoiceMouseLeave;

            element.MouseEnter += VoiceMouseEnter;
            element.MouseLeave += VoiceMouseLeave;
        }

        private static readonly Dictionary<FrameworkElement, CancellationTokenSource> Tokens = new();

        private static async void VoiceMouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is not FrameworkElement element)
                return;

            var cts = new CancellationTokenSource();

            Tokens[element] = cts;

            try
            {
                await Task.Delay(500, cts.Token);

                string phrase = GetPhrase(element);

                VoicePlayerService.PlayPhrase(phrase);
            }
            catch (TaskCanceledException)
            {

            }
        }

        private static void VoiceMouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is not FrameworkElement element)
                return;

            if (Tokens.TryGetValue(element, out var cts))
            {
                cts.Cancel();
                Tokens.Remove(element);
            }
        }
    }
}
