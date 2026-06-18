using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PonyvilleSchool2._0.Services
{
    class MessageService
    {
        private static readonly Random _rand = new();

        //Список приветствующих сообщений
        private static readonly List<string> WelcomeMessagesAvailable =
    [
        $"Добро пожаловать в Школу Дружбы, {AppState.Instance.CurrentUser.name}!",
        $"Снова привет, {AppState.Instance.CurrentUser.name}!",
        "Сегодня отличный день для прогулки в Понивилле!",
        "Готов к новым заданиям?",
        $"{AppState.Instance.CurrentUser.name}, это снова ты! Добро пожаловать!"
    ];
        private static readonly List<string> WelcomeMessagesNonAvailable =
    [
        $"Ты выполнил все задания на сегодня, возвращайся завтра!",
        $"Понивилль сейчас отдыхает, {AppState.Instance.CurrentUser.name}!",
        "Все дела на сегодня выполнены, возвращайся завтра!",
        "Кажется, ты уже выполнил все задания",
        $"{AppState.Instance.CurrentUser.name}, задания сегодня кончились"
    ];
        //Список сообщений при завершении задания
        private static readonly List<string> TaskCompletedMessage =
    [
            $"Поздравляем с завершением задания! ",
            "Отличная работа! ",
            "Молодец! "
    ];
        public static string GetWelcomeMessage()
        {
            if (AppState.Instance.CurrentUser.available != 0)
            {
                return WelcomeMessagesAvailable[_rand.Next(WelcomeMessagesAvailable.Count)];
            }
            else
                return WelcomeMessagesNonAvailable[_rand.Next(WelcomeMessagesNonAvailable.Count)];
        } //Приветствующее сообщение
        public static string GetTaskCompletedMessage(int score)
        {
            string baseString = TaskCompletedMessage[_rand.Next(TaskCompletedMessage.Count)];
            return $"{baseString}Твой результат: {score}";
        } //После завершения задания
    }
}
