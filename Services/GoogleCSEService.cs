using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json; // Needed for JSON parsing

namespace AIAgentConsole.Services // Make sure the namespace matches your project name (AIAgentConsole)
{
    public class GoogleCSEService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _cseId;
        private const string BaseUrl = "https://www.googleapis.com/customsearch/v1";

        public GoogleCSEService(HttpClient httpClient, string apiKey, string cseId)
        {
            _httpClient = httpClient;
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _cseId = cseId ?? throw new ArgumentNullException(nameof(cseId));
        }

        public async Task<List<string>> SearchAsync(string query, int numResults = 10)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<string>();
            }

            var url = $"{BaseUrl}?key={_apiKey}&cx={_cseId}&q={Uri.EscapeDataString(query)}&num={numResults}";

            try
            {
                Console.WriteLine($"Searching Google CSE for: {query}");
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode(); // Throw on error status code

                var jsonString = await response.Content.ReadAsStringAsync();

                // Parse the JSON response to extract links
                using (JsonDocument doc = JsonDocument.Parse(jsonString))
                {
                    List<string> links = new List<string>();
                    if (doc.RootElement.TryGetProperty("items", out JsonElement itemsElement) && itemsElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (JsonElement item in itemsElement.EnumerateArray())
                        {
                            if (item.TryGetProperty("link", out JsonElement linkElement) && linkElement.ValueKind == JsonValueKind.String)
                            {
                                links.Add(linkElement.GetString() ?? string.Empty);
                            }
                        }
                    }
                    Console.WriteLine($"Found {links.Count} search results.");
                    return links;
                }
            }
            catch (HttpRequestException httpEx)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"HTTP Request Error during Google Search: {httpEx.Message}");
                 Console.WriteLine($"Check your API Key, CSE ID, or network connection.");
                Console.ResetColor();
                // Depending on requirements, you might re-throw or return empty list
                return new List<string>();
            }
            catch (JsonException jsonEx)
            {
                 Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"JSON Parsing Error during Google Search: {jsonEx.Message}");
                Console.ResetColor();
                return new List<string>();
            }
             catch (Exception ex)
            {
                 Console.ForegroundColor = ConsoleColor.Red;
                 Console.WriteLine($"An unexpected error occurred during Google Search: {ex.Message}");
                 Console.ResetColor();
                 return new List<string>();
            }
        }
    }
}