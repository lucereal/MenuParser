using MenuParser.Domain.Google;

namespace MenuParser.Services.inter
{
    public interface IWebSearchService
    {
        Task<SearchResult> Search(string query);
    }
}
