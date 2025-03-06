using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using Autodesk.Revit.UI;

namespace MyPlugin
{
    public partial class MyPluginWindow : Window
    {
        private static readonly string apiKey = "ВАШ_API_КЛЮЧ";
        private static readonly string apiUrl = "https://api.openai.com/v1/chat/completions";

        public MyPluginWindow()
        {
            InitializeComponent();
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string userInput = InputTextBox.Text;
            if (string.IsNullOrWhiteSpace(userInput))
            {
                TaskDialog.Show("Ошибка", "Введите сообщение перед отправкой!");
                return;
            }

            try
            {
                OutputTextBox.Text = "Ожидание ответа...";
                string response = await SendToChatGPT(userInput);
                OutputTextBox.Text = response;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Ошибка", $"Произошла ошибка: {ex.Message}");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async Task<string> SendToChatGPT(string prompt)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var requestBody = new
                {
                    model = "gpt-4",
                    messages = new[]
                    {
                        new { role = "system", content = "Ты помощник для пользователей Revit." },
                        new { role = "user", content = prompt }
                    },
                    max_tokens = 100
                };

                var jsonContent = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                HttpResponseMessage response;

                try
                {
                    response = await client.PostAsync(apiUrl, content);
                }
                catch (HttpRequestException httpEx)
                {
                    throw new Exception("Ошибка соединения с сервером. Проверьте интернет-соединение.", httpEx);
                }

                if (!response.IsSuccessStatusCode)
                {
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Ошибка API: {response.StatusCode} \n{errorDetails}");
                }

                string responseString = await response.Content.ReadAsStringAsync();

                dynamic responseObject;
                try
                {
                    responseObject = JsonConvert.DeserializeObject(responseString);
                }
                catch (JsonException jsonEx)
                {
                    throw new Exception("Ошибка обработки ответа от API.", jsonEx);
                }

                return responseObject?.choices?[0]?.message?.content?.ToString() ?? "Ошибка: пустой ответ от API.";
            }
        }

        private void InputTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (InputTextBox.Text == "Введите запрос")
            {
                InputTextBox.Text = "";
                InputTextBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void InputTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InputTextBox.Text))
            {
                InputTextBox.Text = "Введите запрос";
                InputTextBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private void LogButton_Click(object sender, RoutedEventArgs e)
        {
            TaskDialog.Show("Журнал", "Функция журнала пока не реализована.");
        }
    }
}
