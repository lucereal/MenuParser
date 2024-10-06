namespace MenuParser.Domain
{
    public class MenuParseResponse
    {
        public MenuParseResponse()
        {

           items = new List<string>();
            menuItems = new List<MenuItemDto>();
        }
        public List<string> items { get; set; }
        public string fullText { get; set; }

        public List<MenuItemDto> menuItems { get; set; }

        public List<string> menuParagraphs { get; set; }

        public string menuContent { get; set; }

        public MenuDto menuDto { get; set; }
    }
}
