using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure;
using Microsoft.AspNetCore.Mvc;
using MenuParser.Domain;
using Microsoft.Extensions.Configuration;
using MenuParser.Services.inter;

namespace MenuParser.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class MenuParseController : Controller
    {

        private readonly ILogger<MenuParseController> _logger;
        private readonly IMenuIntelligenceService _menuIntelligenceService;

        public MenuParseController(IConfiguration configuration, ILogger<MenuParseController> logger, IMenuIntelligenceService menuIntelligenceService)
        {
            _logger = logger;
            _menuIntelligenceService = menuIntelligenceService;
         
        }

        [HttpPost(Name = "ParseMenu")]

        public async Task<MenuParseResponse> ParseMenu(MenuParseRequest request)
        {
            
            MenuParseResponse menuParseResponse = new MenuParseResponse();
  
            MenuIntelligenceRequest menuIntelligenceRequest = new MenuIntelligenceRequest();
            menuIntelligenceRequest.file = request.file;
            MenuIntelligenceResponse menuIntelligenceResponse = await _menuIntelligenceService.ParseMenu(menuIntelligenceRequest);
            menuParseResponse.items = menuIntelligenceResponse.menuLines;
            return menuParseResponse;
        }

        [HttpPost(Name = "Breakdown")]

        public async Task<MenuParseResponse> Breakdown(MenuParseRequest request)
        {

            MenuParseResponse menuParseResponse = new MenuParseResponse();

            MenuIntelligenceRequest menuIntelligenceRequest = new MenuIntelligenceRequest();
            menuIntelligenceRequest.file = request.file;
            MenuIntelligenceResponse menuIntelligenceResponse = await _menuIntelligenceService.BreakdownMenu(menuIntelligenceRequest);
            menuParseResponse.items = menuIntelligenceResponse.menuLines;
            return menuParseResponse;
        }
    }
}
