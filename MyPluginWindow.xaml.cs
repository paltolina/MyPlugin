using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Reflection;

namespace MyPlugin
{
    public partial class MyPluginWindow : Window
    {
        public ExternalCommandData _commandData;
        public string _message;
        public ElementSet _elements;

        private static readonly string apiKey = Environment.GetEnvironmentVariable("DEEPSEEK_FREE");
        private static readonly string apiUrl = "https://openrouter.ai/api/v1/chat/completions";
        private static readonly string MODEL = "deepseek/deepseek-chat:free";

        public MyPluginWindow(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            this._commandData = commandData;
            this._message = message;
            this._elements = elements;


            InitializeComponent();
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string userInput = InputTextBox.Text;
            InputTextBox.Clear();

            if (string.IsNullOrWhiteSpace(userInput)) //  || string.ToLower(userInput) == "введите запрос" || string.ToLower(userInput).Trim() == "/edit"
            {
                TaskDialog.Show("Ошибка", "Введите сообщение перед отправкой!");
                return;
            }

            //if (userInput.StartsWith("/edit"))
            //{

            //    return;
            //}

            try
            {
                OutputTextBox.Text = "Ожидание ответа...";
                AIResponse response = await SendToChatGPT(userInput);

                response.Answer = response.Answer.Replace("```csharp", string.Empty).Replace("```", string.Empty).Trim();

                OutputTextBox.Text = response.Answer;

                OutputTextBox.Text += $"\nСохраняю скрипт...";

                // Сохраняем скрипт в файл cs
                try
                {
                    string assemblyLocation = Assembly.GetExecutingAssembly().Location;

                    using (StreamWriter writer = new StreamWriter(Path.GetDirectoryName(assemblyLocation) + @"\AICommand.cs", false, Encoding.UTF8))
                    {
                        writer.WriteLine(response.Answer);
                    }
                    OutputTextBox.Text += "\nСкрипт успешно сохранен!";
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Ошибка", $"Произошла ошибка при сохранении скрипта: {ex.Message}");
                }

                OutputTextBox.Text += "\nЗапускаю скрипт...";

                ScriptCompiler compiler = new ScriptCompiler();
                Result result = compiler.Execute(_commandData, ref _message, _elements);

                if (result == Result.Failed)
                {
                    OutputTextBox.Text += "\nПроизошла ошибка при запуске скрипта.";
                }
                else
                {
                    OutputTextBox.Text += "\nСкрипт успешно выполнен!";
                }

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

        private async Task<AIResponse> SendToChatGPT(string prompt)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var requestBody = new
                {
                    model = MODEL,
                    messages = new[]
                    {
                new { role = "system", content = "Ты помощник для пользователей Autodesk Revit." +
                        "Тебе необходимо написать полностью рабочий c# скрипт, использующий RevitAPI и RevitAPIUI, выполняющий задачу, о которой попросит тебя пользователь." +
                        "Твой ответ не должен содержать ничего более, кроме исходного кода самого скрипта." +
                        "Класс, реализующий интерфейс IExternalCommand обязательно должен называться \"AICommand\", пространства имён быть не должно." +
                        "Скрипт должен содержать Все импорты Библиотек, которые он использует. Если используется System.Linq, то он обязательно должен импортироваться." }, // Ты помощник для пользователей Revit.
                new { role = "user", content = prompt }
            },
                    max_tokens = 5000
                    //max_tokens = 100
                };

                var jsonContent = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                HttpResponseMessage response;
                string responseString;
                double cost = 0;
                //string errorMessage = null;

                try
                {
                    response = await client.PostAsync(apiUrl, content);
                    responseString = await response.Content.ReadAsStringAsync();
                }
                catch (HttpRequestException)
                {
                    return new AIResponse
                    {
                        Answer = "",
                        Cost = 0,
                        ErrorMessage = "Ошибка соединения с сервером. Проверьте интернет-соединение."
                    };
                }

                if (!response.IsSuccessStatusCode)
                {
                    return new AIResponse
                    {
                        Answer = "",
                        Cost = 0,
                        ErrorMessage = $"Ошибка API: {response.StatusCode} \n{responseString}"
                    };
                }

                dynamic responseObject = JsonConvert.DeserializeObject(responseString);
                //string answer = responseObject?.choices?[0]?.message?.content?.ToString() ?? "Ошибка: пустой ответ.";
                //string reasoning = responseObject?["choices"]?[0]?["message"]?["reasoning"]?.ToString();
                string answer = responseObject?["choices"]?[0]?["message"]?["content"]?.ToString() ?? "Ошибка: пустой ответ.";

                int promptTokens = responseObject?.usage?.prompt_tokens ?? 0;
                int completionTokens = responseObject?.usage?.completion_tokens ?? 0;
                cost = (promptTokens / 1000.0 * 0.03) + (completionTokens / 1000.0 * 0.06);
                cost = Math.Round(cost, 4);

                return new AIResponse
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
