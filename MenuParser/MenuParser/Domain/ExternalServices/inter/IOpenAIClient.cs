namespace MenuParser.Domain.ExternalServices.inter
{
    public interface IOpenAIClient
    {
        Task<string> GetChatCompletion();
    }
}
