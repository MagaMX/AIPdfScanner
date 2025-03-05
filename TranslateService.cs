using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace AIPdfScanner
{
    class TranslateService
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private const int MaxQueryLength = 25; // Максимальная длина запроса

        public async Task<string> TranslateTextAsync(string text, string targetLang = "ru")
        {
            try
            {
                List<string> parts = SplitText(text, MaxQueryLength);
                List<string> translatedParts = new List<string>();

                foreach (var part in parts)
                {
                    string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=auto&tl={targetLang}&dt=t&q={Uri.EscapeDataString(part)}";
                    HttpResponseMessage response = await httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string jsonResponse = await response.Content.ReadAsStringAsync();

                    using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                    JsonElement root = doc.RootElement;
                    translatedParts.Add(root[0][0][0].GetString());
                }

                return string.Join(" ", translatedParts);
            }
            catch (Exception ex)
            {
                return $"Ошибка перевода: {ex.Message}";
            }
        }

        private List<string> SplitText(string text, int maxLength)
        {
            List<string> parts = new List<string>();
            for (int i = 0; i < text.Length; i += maxLength)
            {
                parts.Add(text.Substring(i, Math.Min(maxLength, text.Length - i)));
            }
            return parts;
        }
    }
}
