using Azure.Core;
using Azure;
using MenuParser.Domain.ExternalServices.inter;
using MenuParser.Services.impl;
using OpenAI;
using OpenAI.Chat;
using System.Data;

using System.Reflection.Metadata;
using System.Text.Json;
using Microsoft.Extensions.Options;

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

            _chatClient = new(model: "gpt-4o", apiKey: apiKey);
        }

        public async Task<string> GetChatCompletion()
        {


            ChatCompletion chatCompletion = await _chatClient.CompleteChatAsync("Say 'this is a test.'");
            

            string resultCompletion = chatCompletion.Content.First().Text;

            await BreakdownMenuLine("- Migas - Totopos, scrambled eggs, duck fat refried beans, chorizo, queso fresco, cilantro, avocado crema, salsa cruda (gf, contains dairy) $6");

            return resultCompletion;
        }

        public async Task<MenuDto> BreakdownMenuImage(MenuIntelligenceRequest menuIntelligenceRequest)
        {
            var instructions = "Your job is to take a picture and to parse and breakdown a full restaurant menu into a list of sections. Sections have a section name with a list of items that have name, price, and description. Some sections have subsections, those should be treated as their own section.";
            //var prompt = $"{menu}";

            SystemChatMessage systemInstructionsChatMessage = new SystemChatMessage(instructions);
            List<ChatMessage> messages = [systemInstructionsChatMessage];
   
            IFormFile file = menuIntelligenceRequest.file.First();
            using (var stream = file.OpenReadStream())
            {
                BinaryData imageBytes = BinaryData.FromStream(stream);
                UserChatMessage userMenuItemChatMessage = new UserChatMessage(
                ChatMessageContentPart.CreateTextPart("Here is the image."),
                ChatMessageContentPart.CreateImagePart(imageBytes, "image/png"));
                messages.Add(userMenuItemChatMessage);
            }


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

            options.Temperature = 0.7f;

            ChatCompletion completion = await _chatClient.CompleteChatAsync(messages, options);

            using JsonDocument structuredJson = JsonDocument.Parse(completion.Content[0].Text);

            MenuDto menuDto = menuDto = JsonSerializer.Deserialize<MenuDto>(completion.Content[0].Text);

            return menuDto;
        }

        public async Task<MenuDto> MenuItemCategory(MenuDto menuDto)
        {
            var instructions = "Your job is to take a picture and to parse and breakdown a full restaurant menu into a list of sections. " +
                "Sections have a section name with a list of items that have name, price, and description. Some sections have subsections, " +
                "those should be treated as their own section.";

            var prompt = "Your job is to categorize json representation of a menu item which has name, description, and price. The category options are " + 
                "";
            
            

            //SystemChatMessage systemInstructionsChatMessage = new SystemChatMessage(instructions);
            //List<ChatMessage> messages = [systemInstructionsChatMessage];

            //ChatCompletionOptions options = new()
            //{
            //    ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
            //       jsonSchemaFormatName: "menu_item_categorization",
            //       jsonSchema: BinaryData.FromBytes("""
            //            {
            //              "type": "object",
            //              "properties": {
            //                "category": {
            //                  "type": "string"
            //                }
            //              },
            //              "required": [
            //                "category"
            //              ],
            //              "additionalProperties": false
            //            }
            //            """u8.ToArray()),
            //               jsonSchemaIsStrict: true),
            //    Tools = { getCurrentLocationTool, getCurrentWeatherTool },
            //};
        }

        private async Task<ChatMessage> doChatCompletionWithTools()
        {
            ChatClient client = _chatClient;

            List<ChatMessage> messages =
            [
                new UserChatMessage("What's the weather like today?"),
            ];

            ChatCompletionOptions options = new()
            {
                Tools = { getCurrentLocationTool, getCurrentWeatherTool },
            };

            bool requiresAction;

            do
            {
                requiresAction = false;
                ChatCompletion completion = await client.CompleteChatAsync(messages, options);

                switch (completion.FinishReason)
                {
                    case ChatFinishReason.Stop:
                        {
                            // Add the assistant message to the conversation history.
                            messages.Add(new AssistantChatMessage(completion));
                            break;
                        }

                    case ChatFinishReason.ToolCalls:
                        {
                            // First, add the assistant message with tool calls to the conversation history.
                            messages.Add(new AssistantChatMessage(completion));

                            // Then, add a new tool message for each tool call that is resolved.
                            foreach (ChatToolCall toolCall in completion.ToolCalls)
                            {
                                switch (toolCall.FunctionName)
                                {
                                    case nameof(GetCurrentLocation):
                                        {
                                            string toolResult = GetCurrentLocation();
                                            messages.Add(new ToolChatMessage(toolCall.Id, toolResult));
                                            break;
                                        }

                                    case nameof(GetCurrentWeather):
                                        {
                                            // The arguments that the model wants to use to call the function are specified as a
                                            // stringified JSON object based on the schema defined in the tool definition. Note that
                                            // the model may hallucinate arguments too. Consequently, it is important to do the
                                            // appropriate parsing and validation before calling the function.
                                            using JsonDocument argumentsJson = JsonDocument.Parse(toolCall.FunctionArguments);
                                            bool hasLocation = argumentsJson.RootElement.TryGetProperty("location", out JsonElement location);
                                            bool hasUnit = argumentsJson.RootElement.TryGetProperty("unit", out JsonElement unit);

                                            if (!hasLocation)
                                            {
                                                throw new ArgumentNullException(nameof(location), "The location argument is required.");
                                            }

                                            string toolResult = hasUnit
                                                ? GetCurrentWeather(location.GetString(), unit.GetString())
                                                : GetCurrentWeather(location.GetString());
                                            messages.Add(new ToolChatMessage(toolCall.Id, toolResult));
                                            break;
                                        }

                                    default:
                                        {
                                            // Handle other unexpected calls.
                                            throw new NotImplementedException();
                                        }
                                }
                            }

                            requiresAction = true;
                            break;
                        }

                    case ChatFinishReason.Length:
                        throw new NotImplementedException("Incomplete model output due to MaxTokens parameter or token limit exceeded.");

                    case ChatFinishReason.ContentFilter:
                        throw new NotImplementedException("Omitted content due to a content filter flag.");

                    case ChatFinishReason.FunctionCall:
                        throw new NotImplementedException("Deprecated in favor of tool calls.");

                    default:
                        throw new NotImplementedException(completion.FinishReason.ToString());
                }
            } while (requiresAction);

            return messages;
        }

        private static string GetCurrentLocation()
        {
            // Call the location API here.
            return "San Francisco";
        }
         static string GetCurrentWeather(string location, string unit = "celsius")
        {
            // Call the weather API here.
            return $"31 {unit}";
        }
        private static readonly ChatTool getCurrentWeatherTool = ChatTool.CreateFunctionTool(
                functionName: nameof(GetCurrentWeather),
                functionDescription: "Get the current weather in a given location",
                functionParameters: BinaryData.FromBytes("""
                            {
                                "type": "object",
                                "properties": {
                                    "location": {
                                        "type": "string",
                                        "description": "The city and state, e.g. Boston, MA"
                                    },
                                    "unit": {
                                        "type": "string",
                                        "enum": [ "celsius", "fahrenheit" ],
                                        "description": "The temperature unit to use. Infer this from the specified location."
                                    }
                                },
                                "required": [ "location" ]
                            }
                            """u8.ToArray())
                );
        private static readonly ChatTool getCurrentLocationTool = ChatTool.CreateFunctionTool(
                    functionName: nameof(GetCurrentLocation),
                    functionDescription: "Get the user's current location"
                );


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
