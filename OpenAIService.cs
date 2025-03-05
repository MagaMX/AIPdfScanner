using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace AIPdfScanner
{
    class OpenAIService
    {
        private string apiKey = "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";
        public async Task<string> GetChatGPTResponse(string text)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                    var requestData = new
                    {
                        model = "gpt-3.5-turbo",
                        messages = new[] { new { role = "user", content = text } },
                        max_tokens = 50
                    };

                    string json = JsonConvert.SerializeObject(requestData);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
                    response.EnsureSuccessStatusCode(); // Выбросит исключение при ошибке HTTP

                    string result = await response.Content.ReadAsStringAsync();
                    dynamic parsedResult = JsonConvert.DeserializeObject(result);

                    return parsedResult.choices[0].message.content;
                }
            }
            catch (HttpRequestException ex)
            {
                return $"Ошибка HTTP: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"Общая ошибка: {ex.Message}";
            }
        }
    }
}
