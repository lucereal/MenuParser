namespace MenuParser.Domain
{
    public class MenuDto
    {
        public MenuDto()
        {

            sections = new List<Section>();
        }
        public List<Section> sections { get; set; }
        public class Section
        {
            public string sectionName { get; set; }
            public List<MenuItem> sectionListOfMenuItems { get; set; }
        }

        public class MenuItem
        {
            public string name { get; set; }
            public string description { get; set; }
            public string price { get; set; }
        }
    }
}
