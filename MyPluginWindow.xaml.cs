using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
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
                ChatGPTResponse response = await SendToChatGPT(userInput);

                OutputTextBox.Text = response.Answer;

                Logger.SaveLog(userInput, response.Answer, response.Cost, response.ErrorMessage);
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

        private async Task<ChatGPTResponse> SendToChatGPT(string prompt)
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
                string responseString;
                double cost = 0;
                string errorMessage = null;

                try
                {
                    response = await client.PostAsync(apiUrl, content);
                    responseString = await response.Content.ReadAsStringAsync();
                }
                catch (HttpRequestException)
                {
                    return new ChatGPTResponse
                    {
                        Answer = "",
                        Cost = 0,
                        ErrorMessage = "Ошибка соединения с сервером. Проверьте интернет-соединение."
                    };
                }

                if (!response.IsSuccessStatusCode)
                {
                    return new ChatGPTResponse
                    {
                        Answer = "",
                        Cost = 0,
                        ErrorMessage = $"Ошибка API: {response.StatusCode} \n{responseString}"
                    };
                }

                dynamic responseObject = JsonConvert.DeserializeObject(responseString);
                string answer = responseObject?.choices?[0]?.message?.content?.ToString() ?? "Ошибка: пустой ответ.";

                int promptTokens = responseObject?.usage?.prompt_tokens ?? 0;
                int completionTokens = responseObject?.usage?.completion_tokens ?? 0;
                cost = (promptTokens / 1000.0 * 0.03) + (completionTokens / 1000.0 * 0.06);
                cost = Math.Round(cost, 4);

                return new ChatGPTResponse
                {
                    Answer = answer,
                    Cost = cost,
                    ErrorMessage = null
                };
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
            Logger.OpenLog();
        }
    }
}
