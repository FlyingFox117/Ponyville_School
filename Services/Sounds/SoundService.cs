using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PonyvilleSchool2._0.Services.Sounds
{
    //Класс для проигрывания звуков в программе. Основной экземпляр хранится в AppState.
    public class SoundService : ISoundService
    {
        private readonly Dictionary<string, SoundPlayer> _players = new();
        public void PlaySound(string soundName) //Проигрывание звука
        {
            if (!_players.ContainsKey(soundName))
            {
                var uri = GetSoundUri(soundName);
                //Получение звука
                var stream = Application.GetResourceStream(uri)?.Stream;
                if (stream == null)
                    throw new FileNotFoundException($"Звук не найден: {soundName}.wav");

                _players[soundName] = new SoundPlayer(stream);
            }
            _players[soundName].Play();
        }
        private static Uri GetSoundUri(string fileName) //Получение пути на звук
        {
            return new Uri($"pack://application:,,,/Assets/Sounds/{fileName}.wav");
        }
    }
    public interface ISoundService
    {
        void PlaySound(string soundName);
    }
}
