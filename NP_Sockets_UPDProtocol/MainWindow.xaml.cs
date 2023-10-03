using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Windows;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace NP_Sockets_UPDProtocol
{
    public partial class MainWindow : Window
    {
        private UdpClient udpClient;
        private IPEndPoint serverEndPoint;
        private ChatBot chatBot;


        public MainWindow()
        {
            InitializeComponent();
            udpClient = new UdpClient();
            serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345);
            chatBot = new ChatBot();

        }

        private async void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string question = QuestionTextBox.Text;
                byte[] data = Encoding.UTF8.GetBytes(question);
                udpClient.Send(data, data.Length, serverEndPoint);

                string response = await chatBot.GetResponse(question); 
                ResponseTextBox.Text = response;

                UpdateHistoryAndIpAddress(question, response);
                await AddResponseToDatabase(question, response);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка: " + ex.Message);
            }
        }

        private void UpdateHistoryAndIpAddress(string question, string response)
        {
            DateTime timestamp = DateTime.Now;
            HistoryListBox.Items.Add(new { Question = question, Timestamp = timestamp, Response = response });

            IPAddress ipAddress = ((IPEndPoint)udpClient.Client.LocalEndPoint).Address;
            int port = ((IPEndPoint)udpClient.Client.LocalEndPoint).Port;
            IpAddressTextBlock.Text = $"IP-адреса: {ipAddress}, Порт: {port}";
        }

        public async Task AddResponseToDatabase(string question, string response)
        {
            using (var dbContext = new ChatBotDbContext())
            {
                var responseRecord = new ResponseRecord
                {
                    Question = question,
                    Response = response,
                    Timestamp = DateTime.Now
                };

                dbContext.Responses.Add(responseRecord);
                await dbContext.SaveChangesAsync();
            }
        }

        public class ChatBot
        {
            private Dictionary<string, string> responses;
            private readonly string apiKey = "13d1715dfd1a40c8bc9182437232609&q=Rivne&aqi=no";

            public ChatBot()
            {
                responses = new Dictionary<string, string>
        {
            {"привіт", "Привіт!"},
            {"як справи", "Справи в мене гарно, дякую!"},
            {"який сьогодні день", $"Сьогодні {GetDayOfWeek()} ."},
            {"скільки годин", $"Зараз {DateTime.Now.ToString("HH:mm")}."},
            {"хто ти?", "\"Я - Говорун, ваш особистий розмовний помічник.\""},
            {"яка твоя улюблена кольорова схема?","Моя улюблена кольорова схема - це морсько-хакі." },
            {"що ти можеш робити?","Я можу відповідати на ваші питання, робити обчислення, конвертувати валюту,\n надавати інформацію про погоду та багато іншого.\n Спробуйте запитати мене щось конкретне!" },
        };
            }

            public async Task<string> GetResponse(string question)
            {
                question = question.ToLower();

                if (question.Contains("порахуй"))
                {
                    string expression = question.Replace("порахуй", "").Trim();
                    try
                    {
                        double result = Calculate(expression);
                        return $"Результат: {result}";
                    }
                    catch (Exception)
                    {
                        return "Не вдалося обчислити вираз.";
                    }
                }
                else if (question.Contains("доларів в гривнях"))
                {
                    string amountStr = question.Replace("доларів в гривнях", "").Trim();
                    if (double.TryParse(amountStr, out double amount))
                    {
                        double uahRate = 36.93;
                        double uahAmount = amount * uahRate;
                        return $"{amount} доларів дорівнює {uahAmount} гривнів.";
                    }
                    else
                    {
                        return "Не вдалося конвертувати валюту.";
                    }
                }
                else if (question.Contains("погода"))
                {
                    string weatherInfo = await GetWeatherInfo("Рівне");
                    return weatherInfo;
                }
                else if (responses.ContainsKey(question))
                {
                    return responses[question];
                }
                else
                {
                    return "Вибачте, я не розумію вашого запиту.";
                }
            }

            public string GetDayOfWeek()
            {
                DateTime currentDate = DateTime.Now;
                DayOfWeek currentDayOfWeek = currentDate.DayOfWeek;

                string dayOfWeek = currentDayOfWeek.ToString();

                return dayOfWeek;
            }

            private double Calculate(string expression)
            {
                System.Data.DataTable table = new System.Data.DataTable();
                table.Columns.Add("expression", string.Empty.GetType(), expression);
                System.Data.DataRow row = table.NewRow();
                table.Rows.Add(row);
                return double.Parse((string)row["expression"]);
            }

            private async Task<string> GetWeatherInfo(string location)
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        string apiUrl = $"https://api.weatherapi.com/v1/current.json?key={apiKey}&q=Rivne&units=metric";
                        HttpResponseMessage response = await client.GetAsync(apiUrl);

                        if (response.IsSuccessStatusCode)
                        {
                            string content = await response.Content.ReadAsStringAsync();

                            dynamic weatherData = JsonConvert.DeserializeObject(content);

                            string cityName = weatherData.location.name;
                            string country = weatherData.location.country;
                            double temperatureC = weatherData.current.temp_c;
                            double temperatureF = weatherData.current.temp_f;
                            int humidity = weatherData.current.humidity;
                            string condition = weatherData.current.condition.text;

                            string weatherInfo = $"Погода в місті {cityName}, {country}: {temperatureC}°C ({temperatureF}°F),\n Вологість: {humidity}%, Стан: {condition}";

                            return weatherInfo;
                        }
                        else
                        {
                            return "Не вдалося отримати дані про погоду.";
                        }
                    }
                }
                catch (Exception ex)
                {
                    return $"Помилка при отриманні інформації про погоду: {ex.Message}";
                }
            }
        }
    }
}
