using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace NextGen.src.Services.Api
{
    internal class DadataService
    {
        private readonly string _token;
        private readonly HttpClient _httpClient;

        public DadataService(string token)
        {
            _token = token;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Token {token}");
        }

        public async Task<List<string>> GetEmailSuggestions(string query)
        {
            string apiUrl = "https://suggestions.dadata.ru/suggestions/api/4_1/rs/suggest/email";
            var content = new StringContent($"{{\"query\":\"{query}\"}}", Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                JObject responseJson = JObject.Parse(responseBody);

                var suggestions = new List<string>();
                foreach (var suggestion in responseJson["suggestions"])
                {
                    suggestions.Add(suggestion["value"].ToString());
                }

                return suggestions;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении подсказок: {ex.Message}");
                return new List<string>();
            }
        }
    }
}
