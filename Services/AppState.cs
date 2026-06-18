using PonyvilleSchool2._0.Models;
using PonyvilleSchool2._0.Services.Sounds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PonyvilleSchool2._0.Services
{
    public class AppState
    {
        private static AppState? _instance;
        public static AppState Instance => _instance ??= new AppState();
        public int LastCompletedScore { get; set; } //Счёт последнего задания
        public SupabaseClient Supabase { get; set; } //Экземпляр Supabase
        public EmailService Email { get; set; }
        public User? CurrentUser { get; set; } //Данные пользователя

        public event Action TaskCompleted; //Событие при завершении задания
        public void RaiseTaskCompleted()
            => TaskCompleted?.Invoke();
        public ISoundService SoundService { get; set;  }
        //Инициализация класса AppState
        private AppState() {
            Supabase = new SupabaseClient();
            CurrentUser = new User();
            SoundService = new SoundService();
            Email = new EmailService();
        } //Конструктор
        public async Task RefreshProfileStats()
        {
            var stats =
                await Supabase.GetProfileStats(
                    CurrentUser.id);

            if (stats == null)
                return;

            CurrentUser.level = stats.level;
            CurrentUser.total_results = stats.total_results;
            CurrentUser.unique_tasks = stats.unique_tasks;
            CurrentUser.favourite_course = stats.favorite_course;
        }
    }
}
