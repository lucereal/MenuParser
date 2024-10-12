using MenuParser.Domain;
using MenuParser.Domain.Google;
using MenuParser.Services.inter;
using Microsoft.AspNetCore.Mvc;

namespace MenuParser.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class WebSearchController : Controller
    {
        private readonly ILogger<WebSearchController> _logger;
        private readonly IWebSearchService _webSearchService;

        public WebSearchController(IConfiguration configuration, ILogger<WebSearchController> logger, IWebSearchService webSearchService)
        {
            _logger = logger;
            _webSearchService = webSearchService;

        }

        [HttpPost(Name = "WebSearch")]

        public async Task<WebSearchResponse> WebSearch(WebSearchRequest request)
        {

            WebSearchResponse webSearchResponse = new WebSearchResponse();

        
            SearchResult searchResult = await _webSearchService.Search(request.query);

            webSearchResponse.searchResult = searchResult;
            return webSearchResponse;
        }
    }
}
