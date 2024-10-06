using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Microsoft.Extensions.Configuration;
using Azure.Core;
using MenuParser.Domain;
using MenuParser.Services.inter;
using MenuParser.Domain.ExternalServices.inter;

namespace MenuParser.Services.impl
{
    public class MenuIntelligenceService : IMenuIntelligenceService
    {


        string endpoint = "https://muonreceiptparser.cognitiveservices.azure.com/";



        private readonly ILogger<MenuIntelligenceService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IOpenAIClient _openAIClient;

        public MenuIntelligenceService(IConfiguration configuration, ILogger<MenuIntelligenceService> logger, IOpenAIClient openAIClient)
        {
            _logger = logger;

            _configuration = configuration;
            _openAIClient = openAIClient;
        }
        public async Task<MenuIntelligenceResponse> ParseMenu(MenuIntelligenceRequest request)
        {
            MenuIntelligenceResponse menuIntelligenceResponse = new MenuIntelligenceResponse();
            string? apiKey = _configuration["DocumentIntelligenceApiKey"];

            //AzureKeyCredential credential = new AzureKeyCredential(apiKey);
            //DocumentAnalysisClient client = new DocumentAnalysisClient(new Uri(endpoint), credential);

            //sample document

            AnalyzeDocumentOperation? operation = null;

            if (apiKey != null)
            {
                var credential = new AzureKeyCredential(apiKey);
                var client = new DocumentAnalysisClient(new Uri(endpoint), credential);

                IFormFile file = request.file.First();
                using (var stream = file.OpenReadStream())
                {
                    operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-receipt", stream);
                }
            }

            //AnalyzeDocumentOperation operation = await client.AnalyzeDocumentFromUriAsync(WaitUntil.Completed, "prebuilt-read", fileUri);

            if (operation != null)
            {
                AnalyzeResult result = operation.Value;

                List<string> menuLines = new List<string>();
                foreach (DocumentPage page in result.Pages)
                {
                    Console.WriteLine($"Document Page {page.PageNumber} has {page.Lines.Count} line(s), {page.Words.Count} word(s),");

                    for (int i = 0; i < page.Lines.Count; i++)
                    {
                        DocumentLine line = page.Lines[i];
                        Console.WriteLine($"  Line {i} has content: '{line.Content}'.");

                        menuLines.Add(line.Content);
                        //Console.WriteLine($"    Its bounding box is:");
                        //Console.WriteLine($"      Upper left => X: {line.BoundingPolygon[0].X}, Y= {line.BoundingPolygon[0].Y}");
                        //Console.WriteLine($"      Upper right => X: {line.BoundingPolygon[1].X}, Y= {line.BoundingPolygon[1].Y}");
                        //Console.WriteLine($"      Lower right => X: {line.BoundingPolygon[2].X}, Y= {line.BoundingPolygon[2].Y}");
                        //Console.WriteLine($"      Lower left => X: {line.BoundingPolygon[3].X}, Y= {line.BoundingPolygon[3].Y}");
                    }
                }

                menuIntelligenceResponse.menuLines = menuLines;
                menuIntelligenceResponse.fullText = string.Join("\n", menuLines);

                foreach (DocumentStyle style in result.Styles)
                {
                    // Check the style and style confidence to see if text is handwritten.
                    // Note that value '0.8' is used as an example.

                    bool isHandwritten = style.IsHandwritten.HasValue && style.IsHandwritten == true;

                    if (isHandwritten && style.Confidence > 0.8)
                    {
                        Console.WriteLine($"Handwritten content found:");

                        foreach (DocumentSpan span in style.Spans)
                        {
                            Console.WriteLine($"  Content: {result.Content.Substring(span.Index, span.Length)}");
                        }
                    }
                }

                foreach (DocumentLanguage language in result.Languages)
                {
                    Console.WriteLine($"  Found language '{language.Locale}' with confidence {language.Confidence}.");
                }

            }


            return menuIntelligenceResponse;
        }


        public async Task<MenuIntelligenceResponse> BreakdownMenuItem(MenuIntelligenceRequest request)
        {
            MenuIntelligenceResponse menuIntelligenceResponse = new MenuIntelligenceResponse();

            MenuIntelligenceResponse menu = await ParseMenu(request);
            
            MenuItemDto menuItemDto = await _openAIClient.BreakdownMenuLine(menu.menuLines[1]);

            menuIntelligenceResponse.menuItems.Add(menuItemDto);
            menuIntelligenceResponse.fullText = menu.fullText;
            menuIntelligenceResponse.menuLines = menu.menuLines;

            return menuIntelligenceResponse;
        }

        public async Task<MenuIntelligenceResponse> BreakdownMenuFull(MenuIntelligenceRequest request)
        {
            MenuIntelligenceResponse menuIntelligenceResponse = new MenuIntelligenceResponse();

            MenuIntelligenceResponse menu = await ParseMenu(request);

            MenuDto menuDto = await _openAIClient.BreakdownMenuFull(menu.fullText);

          
            menuIntelligenceResponse.menuDto = menuDto;
            menuIntelligenceResponse.fullText = menu.fullText;
            menuIntelligenceResponse.menuLines = menu.menuLines;

            return menuIntelligenceResponse;
        }


    }
}
