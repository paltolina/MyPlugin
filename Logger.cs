using System;
using System.IO;

namespace MyPlugin
{
    public static class Logger
    {
        private static int requestNumber = 1; // Нумерация запросов
        private static readonly string logsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

        static Logger()
        {
            if (!Directory.Exists(logsDirectory))
            {
                Directory.CreateDirectory(logsDirectory);
            }
        }

        public static void SaveLog(string userInput, string chatGPTResponse, double cost, string errorMessage)
        {
            string logFilePath = Path.Combine(logsDirectory, $"{DateTime.Now:yyyy-MM-dd}.txt");

            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine($"Запрос #{requestNumber}");
                writer.WriteLine($"Время: {DateTime.Now:HH:mm:ss}");
                writer.WriteLine($"Вопрос: {userInput}");
                writer.WriteLine($"Ответ: {chatGPTResponse}");
                writer.WriteLine($"Цена: ${cost}");

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    writer.WriteLine($"Ошибка: {errorMessage}");
                }

                writer.WriteLine(new string('-', 50)); // Разделитель
            }

            requestNumber++; // Увеличиваем номер запроса
        }

        public static void OpenLog()
        {
            string logFilePath = Path.Combine(logsDirectory, $"{DateTime.Now:yyyy-MM-dd}.txt");

            if (!File.Exists(logFilePath))
            {
                System.Windows.MessageBox.Show("Лог-файл за сегодня отсутствует.", "Журнал", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return;
            }

            System.Diagnostics.Process.Start("notepad.exe", logFilePath);
        }
    }
}
