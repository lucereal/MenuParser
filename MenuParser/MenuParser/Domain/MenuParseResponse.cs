namespace MenuParser.Domain
{
    public class MenuParseResponse
    {
        public MenuParseResponse()
        {

           items = new List<string>();
        }
        public List<string> items { get; set; }
    }
}
