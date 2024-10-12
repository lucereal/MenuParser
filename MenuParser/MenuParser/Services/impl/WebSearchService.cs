using MenuParser.Domain.ExternalServices.inter;
using MenuParser.Services.inter;
using MenuParser.Domain.Google;

namespace MenuParser.Services.impl
{
    public class WebSearchService : IWebSearchService
    {
        private readonly ILogger<WebSearchService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IGoogleSearchClient _googleSearchClient;

        public WebSearchService(IConfiguration configuration, ILogger<WebSearchService> logger, IGoogleSearchClient googleSearchClient)
        {
            _logger = logger;
            _configuration = configuration;
            _googleSearchClient = googleSearchClient;
        }

        public async Task<SearchResult> Search(string query)
        {
            return await _googleSearchClient.GetGoogleSearchResults(query);
        }
    }
}
