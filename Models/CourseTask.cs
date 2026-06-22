using System.Windows.Media;

namespace PonyvilleSchool2._0.Models
{
    public class CourseTask
    {
        public int id { get; set; } //ID задания
        public string title { get; set; } = ""; //Название задания
        public string description { get; set; } = ""; //Описание задания
        public int number { get; set; } //Номер задания в списке
        public string image_url { get; set; } //Превью 
        public string blocks { get; set; } //Структурные блоки задания
        public int? result { get; set; } //Результат прохождения
        public int max_score { get; set; } //Максимально возможный результат
        public bool available { get; set; } //Доступ к заданию
        public string status_text => //Статус задания
        !available ? "Недоступно" :
        result != 0 ? "Пройдено" :
        "Новое задание";
        public Brush status_brush => //Цвет задания
            !available ? Brushes.Gray :
            result != 0 ? Brushes.LightGreen :
            Brushes.Gold;
    }
}
