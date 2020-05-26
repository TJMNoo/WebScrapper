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
    public class LinkCheck
    {
        public readonly string Type;
        public readonly string FoundAt;
        public readonly string Url;
        public readonly HttpStatusCode Status;
        
        public LinkCheck(string t, string f, string u, HttpStatusCode s)
        {
            Type = t;
            FoundAt = f;
            Url = u;
            Status = s;
        }
    }
    
    public class TitleCheck
    {
        public readonly string Type;
        public readonly string Title;
        public readonly string Url;
        
        public TitleCheck(string t, string ti, string u)
        {
            Type = t;
            Title = ti;
            Url = u;
        }
    }
    public class SeoAnalyzer
    {
        HtmlDocument _doc = new HtmlDocument();
        HtmlWeb _web = new HtmlWeb();
        private ScraperEngine Engine { get; set; } = new ScraperEngine();
        private ScraperEngineHelper Helper { get; set; } = new ScraperEngineHelper();
        public List<string> Results { get; set; } = new List<string>();
        public List<LinkCheck> AllLinks { get; set; }
        public List<LinkCheck> HealthyLinks { get; set; }
        public List<LinkCheck> BrokenLinks { get; set; }
        public List<LinkCheck> BlockedLinks { get; set; }
        public List<LinkCheck> RedirectLinks { get; set; }
        public List<LinkCheck> LinksWithIssues { get; set; }
        public Dictionary<string, string> ImgsWithNoAlt { get; set; }
        
        public List<TitleCheck> AllTitles { get; set; }
        public List<TitleCheck> NoTitles { get; set; }
        public List<TitleCheck> TooLongTitles { get; set; }
        public List<TitleCheck> TooShortTitles { get; set; }



        public async Task<string> Analyze(string websiteName)
        {
            await AnalyzeLinks(websiteName);
            await AnalyzeImgs(websiteName);
            await AnalyzeTitles(websiteName);
            return "done";
        }

        public async Task<string> AnalyzeLinks(string websiteName)
        {
            AllLinks = new List<LinkCheck>();
            HealthyLinks = new List<LinkCheck>();
            BrokenLinks = new List<LinkCheck>();
            BlockedLinks = new List<LinkCheck>();
            RedirectLinks = new List<LinkCheck>();
            LinksWithIssues = new List<LinkCheck>();
            
            var responses = Engine.GetHrefsFromRoot(websiteName, 100, 0);
            Helper.SetUrl(websiteName);
            await foreach (var response in responses)
            {
                var url = FormatHrefForAnalyzer(response.Url);
                //href includes tel and mailto, we don't want these as our links
                if (url.Contains("tel:") || url.Contains("mailto:")) continue;
                List<Task> tasks = new List<Task>();
                tasks.Add(Task.Run(() =>
                {
                    _doc = _web.Load(url);

                    switch (_web.StatusCode)
                    {
                        case HttpStatusCode.OK:
                        {
                            if (!Results.Contains(url))
                            {
                                Results.Add(url);
                                LinkCheck result = new LinkCheck("Healthy", response.Url, url, _web.StatusCode);
                                AllLinks.Add(result);
                                HealthyLinks.Add(result);
                            }
                            break;
                        }
                        case HttpStatusCode.Unauthorized:
                        case HttpStatusCode.Forbidden:
                        {
                            if (!Results.Contains(url))
                            {
                                Results.Add(url);
                                LinkCheck result = new LinkCheck("Blocked", response.Url, url, _web.StatusCode);
                                AllLinks.Add(result);
                                BlockedLinks.Add(result);
                            }

                            break;
                        }
                        case HttpStatusCode.Redirect:
                        case HttpStatusCode.Moved:
                        {
                            if (!Results.Contains(url))
                            {
                                Results.Add(url);
                                LinkCheck result = new LinkCheck("Redirect", response.Url, url, _web.StatusCode);
                                AllLinks.Add(result);
                                RedirectLinks.Add(result);
                            }

                            break;
                        }
                        default:
                        {
                            if (!Results.Contains(url))
                            {
                                Results.Add(url);
                                LinkCheck result = new LinkCheck("Broken", response.Url, url, _web.StatusCode);
                                AllLinks.Add(result);
                                BrokenLinks.Add(result);
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
        
        public async Task<string> AnalyzeTitles(string websiteName)
        {
            AllTitles = new List<TitleCheck>();
            NoTitles = new List<TitleCheck>();
            TooLongTitles = new List<TitleCheck>();
            TooShortTitles = new List<TitleCheck>();

            var responses = Engine.GetDocsFromRoot(websiteName, 100, 0);
            await foreach (var response in responses)
            {
                var results = response.Doc.DocumentNode.SelectNodes("//title");
                if (results == null) return "done";
                foreach (var result in results)
                {
                    if (result.OuterHtml == null || result.InnerText == "")
                    {
                        TitleCheck title = new TitleCheck("Missing title", result.InnerText, response.Url);
                        NoTitles.Add(title);
                        AllTitles.Add(title);
                    }
                    else if (result.InnerText.Length >= 60)
                    {
                        TitleCheck title = new TitleCheck("Too long", result.InnerText, response.Url);
                        TooLongTitles.Add(title);
                        AllTitles.Add(title);
                    }
                    else if (result.InnerText.Length <= 40)
                    {
                        TitleCheck title = new TitleCheck("Too short", result.InnerText, response.Url);
                        TooShortTitles.Add(title);
                        AllTitles.Add(title);
                    }
                }
            }

            return "done";
        }
    }
}