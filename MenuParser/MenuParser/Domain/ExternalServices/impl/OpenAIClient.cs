using MenuParser.Domain.ExternalServices.inter;
using MenuParser.Services.impl;
using OpenAI;
using OpenAI.Chat;
using System.Data;

using System.Reflection.Metadata;
using System.Text.Json;

namespace MenuParser.Domain.ExternalServices.impl
{
    public class OpenAIClient : IOpenAIClient
    {

        private readonly ILogger<MenuIntelligenceService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string? apiKey;
        private readonly ChatClient _chatClient;
        public OpenAIClient(IConfiguration configuration, ILogger<MenuIntelligenceService> logger) { 
        
            _logger = logger;
            _configuration = configuration;
            apiKey = _configuration["OPENAI_API_KEY"];

            _chatClient = new(model: "gpt-4o-mini", apiKey: apiKey);
        }

        public async Task<string> GetChatCompletion()
        {


            ChatCompletion chatCompletion = await _chatClient.CompleteChatAsync("Say 'this is a test.'");
            

            string resultCompletion = chatCompletion.Content.First().Text;

            await BreakdownMenuLine("- Migas - Totopos, scrambled eggs, duck fat refried beans, chorizo, queso fresco, cilantro, avocado crema, salsa cruda (gf, contains dairy) $6");

            return resultCompletion;
        }

        public async Task<MenuDto> BreakdownMenuFull(string menu)
        {
            var instructions = "Your job is to parse and breakdown a full menu into a list of sections with the following properties: sectionName, sectionListOfMenuItems. Then to breakdown the sectionListOfMenuItems further into properties: name, description, price.";
            var prompt = $"{menu}";

            SystemChatMessage systemInstructionsChatMessage = new SystemChatMessage(instructions);
            UserChatMessage userMenuItemChatMessage = new UserChatMessage(prompt);
            List<ChatMessage> messages = [systemInstructionsChatMessage, userMenuItemChatMessage];


            ChatCompletionOptions options = new()
            {
                ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                   jsonSchemaFormatName: "menu_full_breakdown",
                   jsonSchema: BinaryData.FromBytes("""
                            {
                                    "type": "object",
                                    "properties": {
                                      "sections": {
                                        "type": "array",
                                        "items": {
                                          "type": "object",
                                          "properties": {
                                            "sectionName": {
                                              "type": "string"
                                            },
                                            "sectionListOfMenuItems": {
                                              "type": "array",
                                              "items": {
                                                "type": "object",
                                                "properties": {
                                                  "name": {
                                                    "type": "string"
                                                  },
                                                  "description": {
                                                    "type": "string"
                                                  },
                                                  "price": {
                                                    "type": "string"
                                                  }
                                                },
                                                "required": [
                                                  "name",
                                                  "description",
                                                  "price"
                                                ],"additionalProperties": false
                                              }
                                            }
                                          },
                                          "required": [
                                            "sectionName",
                                            "sectionListOfMenuItems"
                                          ],
                                          "additionalProperties": false
                                        }
                                      }
                                    },
                                    "required": [
                                      "sections"
                                    ],"additionalProperties": false
                                  }
                        """u8.ToArray()),
                           jsonSchemaIsStrict: true)
            };

            ChatCompletion completion = await _chatClient.CompleteChatAsync(messages, options);

            using JsonDocument structuredJson = JsonDocument.Parse(completion.Content[0].Text);

            MenuDto menuDto = menuDto = JsonSerializer.Deserialize<MenuDto>(completion.Content[0].Text);

            return menuDto;
        }
        public async Task<MenuItemDto> BreakdownMenuLine(string menuLine)
        {
            var instructions = "Your job is to parse and breakdown menu items into the following properties: name, description, price.";
            var prompt = $"{menuLine}";

            SystemChatMessage systemInstructionsChatMessage = new SystemChatMessage(instructions);
            UserChatMessage userMenuItemChatMessage = new UserChatMessage(prompt);
            List<ChatMessage> messages = [systemInstructionsChatMessage, userMenuItemChatMessage];
            

            ChatCompletionOptions options = new()
            {
                ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                   jsonSchemaFormatName: "menu_item_breakdown",
                   jsonSchema: BinaryData.FromBytes("""
                        {
                            "type": "object",
                            "properties": {
                                "name": {
                                    "type": "string"
                                },
                               "description": {
                                                "type": "string"
                                            },
                               "price": {
                                                    "type": "string"
                                              }
                               
                            },
                            "required": [
                            "name",
                            "description",
                            "price"
                            ],
                            "additionalProperties": false
                        }
                        """u8.ToArray()),
                           jsonSchemaIsStrict: true)
            };

            ChatCompletion completion = await _chatClient.CompleteChatAsync(messages, options);

            using JsonDocument structuredJson = JsonDocument.Parse(completion.Content[0].Text);

            Console.WriteLine($"name: {structuredJson.RootElement.GetProperty("name").GetString()}");
            Console.WriteLine($"description: {structuredJson.RootElement.GetProperty("description").GetString()}");
            Console.WriteLine($"price: {structuredJson.RootElement.GetProperty("price").GetString()}");

            MenuItemDto menuItemDto = new MenuItemDto();
            menuItemDto.name = structuredJson.RootElement.GetProperty("name").GetString();
            menuItemDto.description = structuredJson.RootElement.GetProperty("description").GetString();
            menuItemDto.price = structuredJson.RootElement.GetProperty("price").GetString();


            return menuItemDto;
        }
        private async Task<string> TestStructuredOutput()
        {
            List<ChatMessage> messages = [new UserChatMessage("How can I solve 8x + 7 = -23?")];

            ChatCompletionOptions options = new()
            {
                ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                    jsonSchemaFormatName: "math_reasoning",
                    jsonSchema: BinaryData.FromBytes("""
                        {
                            "type": "object",
                            "properties": {
                                "steps": {
                                        "type": "array",
                                        "items": {
                                        "type": "object",
                                        "properties": {
                                            "explanation": {
                                            "type": "string"
                                            },
                                            "output": {
                                            "type": "string"
                                            }
                                        },
                                        "required": [
                                            "explanation",
                                            "output"
                                        ],
                                        "additionalProperties": false
                                        }
                                },
                                "final_answer": {
                                    "type": "string"
                                }
                            },
                            "required": [
                            "steps",
                            "final_answer"
                            ],
                            "additionalProperties": false
                        }
                        """u8.ToArray()),
                            jsonSchemaIsStrict: true)
            };

            ChatCompletion completion = await _chatClient.CompleteChatAsync(messages, options);

            using JsonDocument structuredJson = JsonDocument.Parse(completion.Content[0].Text);

            Console.WriteLine($"Final answer: {structuredJson.RootElement.GetProperty("final_answer").GetString()}");
            Console.WriteLine("Reasoning steps:");

            foreach (JsonElement stepElement in structuredJson.RootElement.GetProperty("steps").EnumerateArray())
            {
                Console.WriteLine($"  - Explanation: {stepElement.GetProperty("explanation").GetString()}");
                Console.WriteLine($"    Output: {stepElement.GetProperty("output")}");
            }

            return "";
        }



}
}
