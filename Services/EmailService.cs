using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace PonyvilleSchool2._0.Services
{
    public class EmailService
    {
        public async Task SendTokenAsync(
            string toEmail,
            string token,
            string type)
        {
            var from = "todandvixie@yandex.ru";
            var password = "lgemgfbaearezxif";

            var message = new MailMessage();

            message.From = new MailAddress(from, "Ponyville School");
            switch (type)
            {
                case "password_reset":
                    {
                        message.Subject = "Восстановление пароля";
                        message.Body =
    $"""
Кажется, вы запросили изменение пароля для вашего аккаунта на платформе
"Школа Понивиля".

Ваш код восстановления:

{token}

Он действителен 10 минут!

Ponyville School
""";
                        break;
                    }
                case "email_verification":
                    {
                        message.Subject = "Подтверждение регистрации";
                        message.Body =
    $"""
Привет!

Твой аккаунт на платформе "Школа Понивиля" почти готов!
Осталось только подтвердить регистрацию. Вот твой код:

{token}

Он действителен 1 день!

Ponyville School
""";
                        break;
                    }
            }
            message.To.Add(toEmail);                 

            using var smtp = new SmtpClient("smtp.yandex.ru", 587);

            smtp.Credentials =
                new NetworkCredential(from, password);

            smtp.EnableSsl = true;

            await smtp.SendMailAsync(message);
        }
    }
}
