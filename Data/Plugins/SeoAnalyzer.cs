using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using WebScraper.Data.Engine;

namespace WebScraper.Data.FakePlugins
{
    public class SeoAnalyzer
    {
        private ScraperEngine Engine { get; set; } = new ScraperEngine();
        private ScraperEngineHelper Helper { get; set; } = new ScraperEngineHelper();
        public Dictionary<string, HttpStatusCode> HealthyLinks { get; set; } = new Dictionary<string, HttpStatusCode>();
        public Dictionary<string, HttpStatusCode> BrokenLinks { get; set; } = new Dictionary<string, HttpStatusCode>();
        public Dictionary<string, HttpStatusCode> RedirectLinks { get; set; } = new Dictionary<string, HttpStatusCode>();
        public Dictionary<string, HttpStatusCode> BlockedLinks { get; set; } = new Dictionary<string, HttpStatusCode>();
        HtmlDocument doc = new HtmlDocument();
        HtmlWeb web = new HtmlWeb();
        public List<string> TempResults { get; set; } = new List<string>();

        public async Task<string> Analyze(string websiteName)
        {
            System.Diagnostics.Debug.Print("Analyzer:\n");
            var responses = Engine.GetHrefsFromRoot(websiteName, 70, 0);
            Helper.SetUrl(websiteName);
            await foreach (var response in responses)
            {
                var url = FormatHrefForAnalyzer(response.Url);
                if (url.Contains("tel:") || url.Contains("mailto:")) continue;
                TempResults.Add(url);
                doc = web.Load(url);
                if (web.StatusCode == HttpStatusCode.OK)
                {
                    if (!HealthyLinks.ContainsKey(url))
                    {
                        HealthyLinks.Add(url, web.StatusCode);
                    }
                }
                else
                {
                    if (!BrokenLinks.ContainsKey(url))
                    {
                        BrokenLinks.Add(url, web.StatusCode);
                    }
                }
            }
            Console.WriteLine("Good links: ");
            HealthyLinks.Select(i => $"{i.Key}: {i.Value}").ToList().ForEach(Console.WriteLine);
            Console.WriteLine("---");
            Console.WriteLine("Bad links: ");
            BrokenLinks.Select(i => $"{i.Key}: {i.Value}").ToList().ForEach(Console.WriteLine);
            return "done";
        }

        public string FormatHrefForAnalyzer(string href)
        {
            Regex checkIfHttps = new Regex(@"^(http|https):\/\/", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            if (checkIfHttps.IsMatch(href) && href.Contains(Helper.WebsiteName)) return href;
            else if (checkIfHttps.IsMatch(href)) return href;

            if (href[0] == '.' && href[1] == '/') return Helper.BaseDomain + href.Substring(1);
            if (href[0] != '/') return Helper.BaseDomain + "/" + href;
            else return Helper.BaseDomain + href;
        }
    }
}
