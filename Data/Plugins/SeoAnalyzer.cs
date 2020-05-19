using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using WebScraper.Data.Engine;

namespace WebScraper.Data.Plugins
{
    public class SeoAnalyzer
    {
        HtmlDocument _doc = new HtmlDocument();
        HtmlWeb _web = new HtmlWeb();
        private ScraperEngine Engine { get; set; } = new ScraperEngine();
        private ScraperEngineHelper Helper { get; set; } = new ScraperEngineHelper();
        public Dictionary<string, HttpStatusCode> AllLinks { get; set; }
        public Dictionary<string, HttpStatusCode> HealthyLinks { get; set; }
        public Dictionary<string, HttpStatusCode> BrokenLinks { get; set; }
        public Dictionary<string, HttpStatusCode> LinksWithIssues { get; set; }
        public Dictionary<string, HttpStatusCode> RedirectLinks { get; set; }
        public Dictionary<string, HttpStatusCode> BlockedLinks { get; set; }
        public List<string> Results { get; set; } = new List<string>();

        public async Task<string> Analyze(string websiteName)
        {
            AllLinks = new Dictionary<string, HttpStatusCode>();
            HealthyLinks = new Dictionary<string, HttpStatusCode>();
            BrokenLinks = new Dictionary<string, HttpStatusCode>();
            LinksWithIssues = new Dictionary<string, HttpStatusCode>();
            BlockedLinks = new Dictionary<string, HttpStatusCode>();
            RedirectLinks = new Dictionary<string, HttpStatusCode>();
            var responses = Engine.GetHrefsFromRoot(websiteName, 100, 0);
            Helper.SetUrl(websiteName);
            await foreach (var response in responses)
            {
                var url = FormatHrefForAnalyzer(response.Url);
                //href includes tel and mailto, we don't want these as our links
                if (url.Contains("tel:") || url.Contains("mailto:")) continue;
                Results.Add(url);
                List<Task> tasks = new List<Task>();
                tasks.Add(Task.Run(() =>
                {
                    _doc = _web.Load(url);
                    if (_web.StatusCode == HttpStatusCode.OK)
                    {
                        if (!HealthyLinks.ContainsKey(url) && !AllLinks.ContainsKey(url))
                        {
                            AllLinks.Add(url, _web.StatusCode);
                            HealthyLinks.Add(url, _web.StatusCode);
                        }
                    }
                    else if (_web.StatusCode == HttpStatusCode.Unauthorized || _web.StatusCode == HttpStatusCode.Forbidden)
                    {
                        if (!BlockedLinks.ContainsKey(url) && !AllLinks.ContainsKey(url))
                        {
                            AllLinks.Add(url, _web.StatusCode);
                            BlockedLinks.Add(url, _web.StatusCode);
                        }
                    }
                    else if (_web.StatusCode == HttpStatusCode.Redirect || _web.StatusCode == HttpStatusCode.Moved)
                    {
                        if (!RedirectLinks.ContainsKey(url) && !AllLinks.ContainsKey(url))
                        {
                            AllLinks.Add(url, _web.StatusCode);
                            RedirectLinks.Add(url, _web.StatusCode);
                        }
                    }
                    else
                    {
                        if (!BrokenLinks.ContainsKey(url) && !AllLinks.ContainsKey(url)) 
                        {
                            AllLinks.Add(url, _web.StatusCode);
                            BrokenLinks.Add(url, _web.StatusCode);
                        }
                    }
                }));
            }
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
