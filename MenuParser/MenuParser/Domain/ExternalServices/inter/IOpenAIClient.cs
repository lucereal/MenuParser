namespace MenuParser.Domain.ExternalServices.inter
{
    public interface IOpenAIClient
    {
        Task<string> GetChatCompletion();
        Task<MenuItemDto> BreakdownMenuLine(string menuLine);
        Task<MenuDto> BreakdownMenuFull(string menu);
    }
}
