namespace PonyvilleSchool2._0.Models
{
    public class CompletedTaskInfo
    {
        public string task_title { get; set; } = ""; //Название задания
        public string course_title { get; set; } = ""; //Название курса
        public DateTime completed_date { get; set; } //Дата выполнения
        public int score { get; set; } = 0; //Полученный результат
        public string score_image_url { get; set; } = ""; //Изображение счёта
    }
}
