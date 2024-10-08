﻿namespace MenuParser.Domain
{
    public class MenuIntelligenceResponse
    {
        public MenuIntelligenceResponse()
        {

            menuLines = new List<string>();
            menuItems = new List<MenuItemDto>();
        }
        public List<string> menuLines { get; set; }

        public string fullText { get; set; }

        public string menuContent { get; set; }

        public List<MenuItemDto> menuItems { get; set; }

        public List<string> menuParagraphs { get; set; }

        public MenuDto menuDto { get; set; }

    }
}
