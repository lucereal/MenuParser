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

        [HttpPost(Name = "ParseMenuGpt")]

        public async Task<MenuParseResponse> ParseMenuGpt(MenuParseRequest request)
        {


            MenuParseResponse menuParseResponse = new MenuParseResponse();

            MenuIntelligenceRequest menuIntelligenceRequest = new MenuIntelligenceRequest();
            menuIntelligenceRequest.file = request.file;
            MenuIntelligenceResponse menuIntelligenceResponse = await _menuIntelligenceService.ParseMenuOpenAIVision(menuIntelligenceRequest);
    
            menuParseResponse.menuDto = menuIntelligenceResponse.menuDto;
            return menuParseResponse;
        }

        [HttpPost(Name = "MenuCategory")]
        public async Task<MenuParseResponse> MenuCategory(MenuParseRequest request)
        {


            MenuParseResponse menuParseResponse = new MenuParseResponse();

            MenuIntelligenceRequest menuIntelligenceRequest = new MenuIntelligenceRequest();
            menuIntelligenceRequest.file = request.file;
            MenuIntelligenceResponse menuIntelligenceResponse = await _menuIntelligenceService.MenuCategoryAssignment(menuIntelligenceRequest);

            menuParseResponse.menuDto = menuIntelligenceResponse.menuDto;
            return menuParseResponse;
        }

        [HttpPost(Name = "ParseMenu")]

        public async Task<MenuParseResponse> ParseMenu(MenuParseRequest request)
        {
            
            MenuParseResponse menuParseResponse = new MenuParseResponse();
  
            MenuIntelligenceRequest menuIntelligenceRequest = new MenuIntelligenceRequest();
            menuIntelligenceRequest.file = request.file;
            MenuIntelligenceResponse menuIntelligenceResponse = await _menuIntelligenceService.ParseMenu(menuIntelligenceRequest);
            menuParseResponse.items = menuIntelligenceResponse.menuLines;
            menuParseResponse.fullText = menuIntelligenceResponse.fullText;
            menuParseResponse.menuParagraphs = menuIntelligenceResponse.menuParagraphs;
            menuParseResponse.menuContent = menuIntelligenceResponse.menuContent;
            return menuParseResponse;
        }

        [HttpPost(Name = "BreakdownItem")]

        public async Task<MenuParseResponse> BreakdownItem(MenuParseRequest request)
        {

            MenuParseResponse menuParseResponse = new MenuParseResponse();

            MenuIntelligenceRequest menuIntelligenceRequest = new MenuIntelligenceRequest();
            menuIntelligenceRequest.file = request.file;
            MenuIntelligenceResponse menuIntelligenceResponse = await _menuIntelligenceService.BreakdownMenuItem(menuIntelligenceRequest);
            menuParseResponse.items = menuIntelligenceResponse.menuLines;
            menuParseResponse.fullText = menuIntelligenceResponse.fullText;
            menuParseResponse.menuItems = menuIntelligenceResponse.menuItems;
            return menuParseResponse;
        }

        [HttpPost(Name = "BreakdownFull")]

        public async Task<MenuParseResponse> BreakdownFull(MenuParseRequest request)
        {

            MenuParseResponse menuParseResponse = new MenuParseResponse();

            MenuIntelligenceRequest menuIntelligenceRequest = new MenuIntelligenceRequest();
            menuIntelligenceRequest.file = request.file;
            MenuIntelligenceResponse menuIntelligenceResponse = await _menuIntelligenceService.BreakdownMenuFull(menuIntelligenceRequest);
            menuParseResponse.items = menuIntelligenceResponse.menuLines;
            menuParseResponse.fullText = menuIntelligenceResponse.fullText;
            menuParseResponse.menuItems = menuIntelligenceResponse.menuItems;
            menuParseResponse.menuDto = menuIntelligenceResponse.menuDto;
            return menuParseResponse;
        }



    }
}
