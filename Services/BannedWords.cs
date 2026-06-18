using System.IO;
using System.Reflection;
using System.Text;

namespace PonyvilleSchool2._0.Services
{
    public static class BannedWords
    {
        private static HashSet<string> _bannedList; //Список запрещенных имен
        static BannedWords()
        {
            _bannedList = LoadFromEmbeddedResource() ?? new HashSet<string>();
        } //Конструктор
        public static bool IsNameBanned(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            //Оставляем только буквы, приводим к нижнему регистру
            string clean = Normalize(name);
            return _bannedList.Any(banned => clean.Contains(banned));
        } //Проверка на заблокированность имени
        private static string Normalize(string input)
        {
            input = input.ToLowerInvariant();
            var sb = new StringBuilder();
            foreach (char c in input)
            {
                //Оставляем любые буквы (включая кириллицу)
                if (char.IsLetter(c))
                    sb.Append(c);
                //Остальное игнорируется
            }
            return sb.ToString();
        } //Нормализация строки
        private static HashSet<string> LoadFromEmbeddedResource()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                // Ресурс ищется по имени: ПространствоИмён.ИмяФайла (если файл в корне проекта)
                // У тебя может отличаться: проверь точное имя через assembly.GetManifestResourceNames()
                string resourceName = assembly.GetName().Name + ".Assets.banned_words.txt";
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                        return null;
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        return reader.ReadToEnd()
                            .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(line => line.Trim().ToLowerInvariant())
                            .Where(line => !string.IsNullOrEmpty(line))
                            .ToHashSet();
                    }
                }
            }
            catch
            {
                return null;
            }
        } //Загрузка списка из файла ресурсов
    }
}
