using MenuParser.Domain.ExternalServices.inter;
using MenuParser.Services.impl;
using OpenAI.Chat;

namespace MenuParser.Domain.ExternalServices.impl
{
    public class OpenAIClient : IOpenAIClient
    {

        private readonly ILogger<MenuIntelligenceService> _logger;
        private readonly IConfiguration _configuration;
        public OpenAIClient(IConfiguration configuration, ILogger<MenuIntelligenceService> logger) { 
        
            _logger = logger;
            _configuration = configuration;
        }

       

        public async Task<string> GetChatCompletion()
        {
            string? apiKey = _configuration["OPENAI_API_KEY"];

            ChatClient client = new(model: "gpt-4o-mini", apiKey: apiKey);

            ChatCompletion chatCompletion = await client.CompleteChatAsync("Say 'this is a test.'");

            string resultCompletion = chatCompletion.Content.First().Text;

            return resultCompletion;
        }

}
}
