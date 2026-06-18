namespace PonyvilleSchool2._0.Models
{
    public class Course
    {
        public int id {  get; set; } //ID курса
        public string title { get; set; } //Название курса
        public string description { get; set; } //Описание курса
        public string pony_image_url { get; set; } //Ссылка на изображение пони
        public string house_image_url { get; set; } //Ссылка на изображение домика-кнопки
        public string score_image_url { get; set; } //Ссылка на иконку баллов
        public string course_image_url { get; set; } //Ссылка на фон курса
        public string color { get; set; } //Цветовой код шапки
        public int completed_tasks { get; set; } //Количество выполненных заданий
        public int total_tasks { get; set; } //Общее количество заданий
    }
}
