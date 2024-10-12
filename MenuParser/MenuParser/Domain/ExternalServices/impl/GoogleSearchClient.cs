using MenuParser.Domain.ExternalServices.inter;
using MenuParser.Domain.Google;
using MenuParser.Services.impl;
using OpenAI.Chat;
using System.Text.Json;
namespace MenuParser.Domain.ExternalServices.impl
{
    public class GoogleSearchClient : IGoogleSearchClient
    {
        private readonly ILogger<GoogleSearchClient> _logger;
        private readonly IConfiguration _configuration;
        private readonly string? apiKey;
        private readonly string? apiCx;
        private readonly string apiUrl = "https://www.googleapis.com/customsearch/v1";
        private HttpClient _httpClient;

        public GoogleSearchClient(IConfiguration configuration, ILogger<GoogleSearchClient> logger, HttpClient httpClient)
        {

            _logger = logger;
            _configuration = configuration;
            apiKey = _configuration["GOOGLE_API_KEY"];
            apiCx = _configuration["GOOGLE_CX"];
            _httpClient = httpClient;
        }

        public async Task<SearchResult> GetGoogleSearchResults(string query)
        {
   
            var requestUri = GetRequestUrl(query);

            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
           
            SearchResult searchResult = JsonSerializer.Deserialize<SearchResult>(responseBody);
            return searchResult;
        }

        private string GetRequestUrl(string query) {
            return apiUrl + $"?key={apiKey}&cx={apiCx}&q={query}";
        }
    }
}
