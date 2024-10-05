namespace MenuParser.Domain
{
    public class MenuIntelligenceResponse
    {
        public MenuIntelligenceResponse()
        {

            menuLines = new List<string>();
        }
        public List<string> menuLines { get; set; }
    }
}
