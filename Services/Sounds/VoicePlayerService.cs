using NAudio.Wave;
using System;
using System.IO;
using System.Media;
using System.Windows;

namespace PonyvilleSchool2._0.Services.Sounds
{
    public static class VoicePlayerService
    {
        private static readonly Random _random = new();
        private static WaveOutEvent? _currentOutput;
        private static WaveFileReader? _currentReader;
        private static MemoryStream? _currentStream;

        public static bool IsEnabled = true;
        public static void PlayPhrase(string phrase)
        {
            if (string.IsNullOrWhiteSpace(phrase))
                return;
            if (!IsEnabled)
                return;
            try
            {
                Logger.Write(
                    Logger.LogLevel.Info,
                    "VoicePlayerService",
                    phrase,
                    "START");

                if (_currentOutput != null)
                {
                    Logger.Write(
                        Logger.LogLevel.Info,
                        "VoicePlayerService",
                        phrase,
                        "STOP PREVIOUS");
                }

                _currentOutput?.Stop();
                _currentOutput?.Dispose();
                _currentOutput = null;

                _currentReader?.Dispose();
                _currentReader = null;

                _currentStream?.Dispose();
                _currentStream = null;

                var uri = new Uri(
                    $"pack://application:,,,/Assets/Sounds/Voices/{phrase}.wav");

                var stream = Application.GetResourceStream(uri)?.Stream;

                if (stream == null)
                    return;

                _currentStream = new MemoryStream();
                stream.CopyTo(_currentStream);
                _currentStream.Position = 0;

                _currentReader = new WaveFileReader(_currentStream);
                _currentOutput = new WaveOutEvent();

                _currentOutput.Init(_currentReader);
                _currentOutput.Play();
            }
            catch (Exception ex)
            {
                Logger.Write(
                  Logger.LogLevel.Error,
                  "VoicePlayerService",
                  "Воспроизведение: " + phrase,
                  $"FAILED | {ex}");

                return;
            }
        }

        public static void PlayRandomPraise()
        {
            string[] phrases =
            {
                "good!",
                "awesome!",
                "nicework!"
            };

            string phrase = phrases[_random.Next(phrases.Length)];

            PlayPhrase(phrase);
        }
    }
}
