using MenuParser.Domain;

namespace MenuParser.Services.inter
{
    public interface IMenuIntelligenceService
    {
        Task<MenuIntelligenceResponse> ParseMenu(MenuIntelligenceRequest request);

        Task<MenuIntelligenceResponse> BreakdownMenuItem(MenuIntelligenceRequest request);

        Task<MenuIntelligenceResponse> BreakdownMenuFull(MenuIntelligenceRequest request);
    }
}
