using System.Security.Cryptography;
using System.Text;

namespace PonyvilleSchool2._0.Services
{
    //Класс для работы с зашифрованными и зашированными данными
    //(Пароли или Хэши токенов)
    public class SecurityService
    {
        public static string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        } //Хэширование
        public static bool Verify(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        } //Проверка
        public static string HashToken(string token)
        {
            byte[] bytes =
            SHA256.HashData(
                Encoding.UTF8.GetBytes(token));

            return Convert.ToHexString(bytes);
        } //Шифрование
        public static string Encrypt(string text)
        {
            byte[] data = Encoding.UTF8.GetBytes(text);

            byte[] encrypted = ProtectedData.Protect(
                data,
                null,
                DataProtectionScope.CurrentUser);

            return Convert.ToBase64String(encrypted);
        } //Шифрование токена через DPAPI
        public static string Decrypt(string encryptedText)
        {
            byte[] encryptedData = Convert.FromBase64String(encryptedText);

            byte[] decrypted = ProtectedData.Unprotect(
                encryptedData,
                null,
                DataProtectionScope.CurrentUser);

            return Encoding.UTF8.GetString(decrypted);
        } //Расшифровка токена через DPAPI
    }
}
