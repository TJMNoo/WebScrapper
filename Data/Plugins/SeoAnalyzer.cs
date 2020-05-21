using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.CodeAnalysis;
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
        public Dictionary<string, string> ImgsWithNoAlt { get; set; }

        public async Task<string> Analyze(string websiteName)
        {
            await AnalyzeLinks(websiteName);
            await AnalyzeImgs(websiteName);
            return "done";
        }

        public async Task<string> AnalyzeLinks(string websiteName)
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
                    if (!AllLinks.ContainsKey(url)) AllLinks.Add(url, _web.StatusCode);

                    switch (_web.StatusCode)
                    {
                        case HttpStatusCode.OK:
                        {
                            if (!HealthyLinks.ContainsKey(url))
                            {
                                HealthyLinks.Add(url, _web.StatusCode);
                            }

                            break;
                        }
                        case HttpStatusCode.Unauthorized:
                        case HttpStatusCode.Forbidden:
                        {
                            if (!BlockedLinks.ContainsKey(url))
                            {
                                BlockedLinks.Add(url, _web.StatusCode);
                            }

                            break;
                        }
                        case HttpStatusCode.Redirect:
                        case HttpStatusCode.Moved:
                        {
                            if (!RedirectLinks.ContainsKey(url))
                            {
                                RedirectLinks.Add(url, _web.StatusCode);
                            }

                            break;
                        }
                        default:
                        {
                            if (!BrokenLinks.ContainsKey(url))
                            {
                                BrokenLinks.Add(url, _web.StatusCode);
                            }

                            break;
                        }
                    }
                }));
            }

            return "done";
        }

        public string FormatHrefForAnalyzer(string href)
        {
            Regex checkIfHttps = new Regex(@"^(http|https):\/\/", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            if (checkIfHttps.IsMatch(href)) return href;

            if (href.StartsWith("//")) return href.Substring(2);
            if (href[0] == '.' && href[1] == '/') return Helper.BaseDomain + href.Substring(1);
            if (href[0] != '/') return Helper.BaseDomain + "/" + href;
            else return Helper.BaseDomain + href;
        }

        public async Task<string> AnalyzeImgs(string websiteName)
        {
            ImgsWithNoAlt = new Dictionary<string, string>();
            var responses = Engine.GetDocsFromRoot(websiteName, 100, 0);
            await foreach (var response in responses)
            {
                var results = response.Doc.DocumentNode.SelectNodes("//img[@alt='']");
                if (results == null) return "done";
                foreach (var result in results)
                {
                    if (result.OuterHtml == null) continue;
                    if (!ImgsWithNoAlt.ContainsKey(result.OuterHtml)) ImgsWithNoAlt.Add(result.OuterHtml, response.Url);
                }
            }

            return "done";
        }
    }
}