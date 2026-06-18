namespace PonyvilleSchool2._0.Models
{
    public class User
    {
        public int? id { get; set; } //ID пользователя
        public string name { get; set; } = ""; //Имя пользователя
        public string role { get; set; } = ""; //Роль пользователя (ученик/администратор)
        public string avatar { get; set; } = ""; //Аватар пользователя
        public int? level { get; set; } //Уровень пользователя
        public int available { get; set; } //Кол-во доступных заданий
        public DateTime reg_date { get; set; } //Дата регистрации
        public int total_results { get; set; } //Кол-во всего выполненных
        public int unique_tasks { get; set; } //Кол-во уникальных задач
        public string favourite_course { get; set; } = ""; //Любимый курс прохождения
    }
}
