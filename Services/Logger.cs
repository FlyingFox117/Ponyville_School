using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PonyvilleSchool2._0.Services
{
    public static class Logger
    {
        private static readonly string LogDirectory =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData),
                "Ponyville School",
                "logs");
        public enum LogLevel
        {
            Info,
            Warning,
            Error
        }

        private static string LogFilePath;

        public static void Initialize()
        {
            Directory.CreateDirectory(LogDirectory);

            LogFilePath = Path.Combine(
                LogDirectory,
                $"log_{DateTime.Now:yyyy-MM-dd}.txt");

            Write(
                LogLevel.Info,
                "SYSTEM",
                "PROGRAM_START",
                "Программа запущена");
        }

        public static void Write(
            LogLevel level,
            string source,
            string action,
            string message)
        {
            try
            {
                string line =
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] | " +
                    $"{level} | " +
                    $"{source} | " +
                    $"{action} | " +
                    $"{message}";

                File.AppendAllText(
                    LogFilePath,
                    line + Environment.NewLine);
            }
            catch
            {
                // Логгер никогда не должен ронять программу
            }
        }
    }
}
