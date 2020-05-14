namespace WebScraper.Data
{
    public class HealthCheck
    {
        public int CrawledPages { get; set; }
        public int HealthyLinks { get; set; }
        public int BrokenLinks { get; set; }
        public int LinksWithIssues { get; set; }
        public int RedirectLinks { get; set; }
        public int BlockedLinks { get; set; }
    }
}