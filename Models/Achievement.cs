namespace PonyvilleSchool2._0.Models
{
    public class Achievement
    {
        public int id { get; set; } //ID достижения
        public string title { get; set; } //Название достижения
        public string description { get; set; } //Описание достижения
        public DateTime received_at { get; set; } //Дата получения
        public string image_url { get; set; } //Изображение достижения
    }
}
