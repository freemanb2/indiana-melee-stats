namespace API_Scraper.API
{
    public class PageInfo
    { 
        public int Total { get; set; }
        public int TotalPages { get; set; }
        public int Page { get; set; }
        public int PerPage { get; set; }
        public string SortBy { get; set; }
        public object Filter { get; set; }
    }
}
