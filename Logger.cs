using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Diagnostics;
using System.Windows;

namespace MyPlugin
{
    public static class Logger
    {
        private static readonly string logsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

        static Logger()
        {
            if (!Directory.Exists(logsDirectory))
            {
                Directory.CreateDirectory(logsDirectory);
            }
        }

        public static void SaveLog(string userInput, AIResponse response)
        {
            try
            {
                string requestId = Guid.NewGuid().ToString(); // Уникальный идентификатор запроса
                string logFilePath = Path.Combine(logsDirectory, $"{DateTime.Now:yyyy-MM-dd}.txt");
                string jsonLogFilePath = Path.Combine(logsDirectory, $"{DateTime.Now:yyyy-MM-dd}.json");

                var logEntry = new
                {
                    RequestId = requestId,
                    Time = DateTime.Now.ToString("HH:mm:ss"),
                    Question = userInput,
                    Answer = response.Answer,
                    Cost = response.Cost,
                    ErrorMessage = response.ErrorMessage
                };

                string logText = new StringBuilder()
                    .AppendLine($"Запрос ID: {requestId}")
                    .AppendLine($"Время: {logEntry.Time}")
                    .AppendLine($"Вопрос: {logEntry.Question}")
                    .AppendLine($"Ответ: {logEntry.Answer}")
                    .AppendLine($"Цена: ${logEntry.Cost:F4}")
                    .AppendLine(string.IsNullOrEmpty(logEntry.ErrorMessage) ? "" : $"Ошибка: {logEntry.ErrorMessage}")
                    .AppendLine(new string('-', 50))
                    .ToString();

                string jsonLog = JsonSerializer.Serialize(logEntry, new JsonSerializerOptions { WriteIndented = true });

                // Записываем в обычный лог
                File.AppendAllText(logFilePath, logText, Encoding.UTF8);

                // Записываем в JSON лог
                File.AppendAllText(jsonLogFilePath, jsonLog + Environment.NewLine, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при записи лога: {ex.Message}", "Ошибка логирования",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void OpenLog()
        {
            string logFilePath = Path.Combine(logsDirectory, $"{DateTime.Now:yyyy-MM-dd}.txt");

            if (!File.Exists(logFilePath))
            {
                MessageBox.Show("Лог-файл за сегодня отсутствует.", "Журнал",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = logFilePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии лога: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
