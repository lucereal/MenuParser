using MenuParser.Domain.Google;

namespace MenuParser.Domain.ExternalServices.inter
{
    public interface IGoogleSearchClient
    {
        Task<SearchResult> GetGoogleSearchResults(string query);
    }
}
