namespace MenuParser.Domain.Google
{
        public class SearchResult
        {
            public string kind { get; set; }
            public Url url { get; set; }
            public Queries queries { get; set; }
            public Context context { get; set; }
            public SearchInformation searchInformation { get; set; }
            public List<Item> items { get; set; }
        }

        public class Url
        {
            public string type { get; set; }
            public string template { get; set; }
        }

        public class Queries
        {
            public List<Request> request { get; set; }
            public List<NextPage> nextPage { get; set; }
        }

        public class Request
        {
            public string title { get; set; }
            public string totalResults { get; set; }
            public string searchTerms { get; set; }
            public int count { get; set; }
            public int startIndex { get; set; }
            public string inputEncoding { get; set; }
            public string outputEncoding { get; set; }
            public string safe { get; set; }
            public string cx { get; set; }
        }

        public class NextPage
        {
            public string title { get; set; }
            public string totalResults { get; set; }
            public string searchTerms { get; set; }
            public int count { get; set; }
            public int startIndex { get; set; }
            public string inputEncoding { get; set; }
            public string outputEncoding { get; set; }
            public string safe { get; set; }
            public string cx { get; set; }
        }

        public class Context
        {
            public string title { get; set; }
        }

        public class SearchInformation
        {
            public double searchTime { get; set; }
            public string formattedSearchTime { get; set; }
            public string totalResults { get; set; }
            public string formattedTotalResults { get; set; }
        }

        public class Item
        {
            public string kind { get; set; }
            public string title { get; set; }
            public string htmlTitle { get; set; }
            public string link { get; set; }
            public string displayLink { get; set; }
            public string snippet { get; set; }
            public string htmlSnippet { get; set; }
            public string formattedUrl { get; set; }
            public string htmlFormattedUrl { get; set; }
            public PageMap pageMap { get; set; }
        }

        public class PageMap
        {
            public List<CseThumbnail> cseThumbnail { get; set; }
            public List<MetaTag> metaTags { get; set; }
            public List<CseImage> cseImage { get; set; }
        }

        public class CseThumbnail
        {
            public string src { get; set; }
            public string width { get; set; }
            public string height { get; set; }
        }

        public class MetaTag
        {
            public string ogImage { get; set; }
            public string ogType { get; set; }
            public string ogImageWidth { get; set; }
            public string twitterCard { get; set; }
            public string articlePublishedTime { get; set; }
            public string ogSiteName { get; set; }
            public string ogTitle { get; set; }
            public string ogImageHeight { get; set; }
            public string ogUpdatedTime { get; set; }
            public string ogDescription { get; set; }
            public string twitterCreator { get; set; }
            public string twitterImage { get; set; }
            public string articleModifiedTime { get; set; }
            public string viewport { get; set; }
            public string ogUrl { get; set; }
        }

        public class CseImage
        {
            public string src { get; set; }
        }
    }


