using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
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

    public class TitleDescCheck
    {
        public readonly string Type;
        public readonly string TitleDesc;
        public readonly string Url;

        public TitleDescCheck(string t, string ti, string u)
        {
            Type = t;
            TitleDesc = ti;
            Url = u;
        }
    }

    public class SeoAnalyzer
    {
        public Action StateHasChangedDelegate { get; set; }
        HtmlDocument _doc = new HtmlDocument();
        HtmlWeb _web = new HtmlWeb();
        private ScraperEngine Engine { get; set; } = new ScraperEngine();
        private ScraperEngineHelper Helper { get; set; } = new ScraperEngineHelper();
        public List<string> Results { get; set; } = new List<string>();
        public List<LinkCheck> AllLinks { get; set; } = new List<LinkCheck>();
        public List<LinkCheck> HealthyLinks { get; set; } = new List<LinkCheck>();
        public List<LinkCheck> BrokenLinks { get; set; } = new List<LinkCheck>();
        public List<LinkCheck> BlockedLinks { get; set; } = new List<LinkCheck>();
        public List<LinkCheck> RedirectLinks { get; set; } = new List<LinkCheck>();
        public List<LinkCheck> LinksWithIssues { get; set; } = new List<LinkCheck>();
        public Dictionary<string, string> ImgsWithNoAlt { get; set; } = new Dictionary<string, string>();
        public List<TitleDescCheck> AllTitles { get; set; } = new List<TitleDescCheck>();
        public List<TitleDescCheck> HealthyTitles { get; set; } = new List<TitleDescCheck>();
        public List<TitleDescCheck> EmptyTitles { get; set; } = new List<TitleDescCheck>();
        public List<TitleDescCheck> LongTitles { get; set; } = new List<TitleDescCheck>();
        public List<TitleDescCheck> ShortTitles { get; set; } = new List<TitleDescCheck>();
        public List<TitleDescCheck> AllDescriptions { get; set; } = new List<TitleDescCheck>();
        public List<TitleDescCheck> HealthyDescriptions { get; set; } = new List<TitleDescCheck>();
        public List<TitleDescCheck> EmptyDescriptions { get; set; } = new List<TitleDescCheck>();
        public List<TitleDescCheck> LongDescriptions { get; set; } = new List<TitleDescCheck>();
        public List<TitleDescCheck> ShortDescriptions { get; set; } = new List<TitleDescCheck>();
        public IAsyncEnumerable<ScraperEngineResponse> Responses;
        public IAsyncEnumerable<ScraperEngineResponse> ResponsesHref;

        public async Task<string> Analyze(string websiteName)
        {
            Responses = Engine.GetDocsFromRoot(websiteName, 100, 0);
            ResponsesHref = Engine.GetHrefsFromRoot(websiteName, 100, 0);
            //await AnalyzeTitles();
            //Console.WriteLine("titles ok");
            //await AnalyzeDescriptions();
            //Console.WriteLine("desc ok");
            List<Task> tasks = new List<Task>();
            tasks.Add(Task.Run(() => AnalyzeLinks(websiteName)));
            tasks.Add(Task.Run(AnalyzeOther));
            await Task.WhenAll(tasks);

            //await AnalyzeLinks(websiteName);
            //await AnalyzeOther();
            //Console.WriteLine("links ok");
            //await AnalyzeImgs();
            //Console.WriteLine("imgs ok");

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

            Helper.SetUrl(websiteName);
            await foreach (var response in ResponsesHref)
            {
                var url = FormatHrefForAnalyzer(response.Url);
                //href includes tel and mailto, we don't want these as our links
                if (url.Contains("tel:") || url.Contains("mailto:") || url.Contains("callto:")) continue;
                List<Task> tasks = new List<Task>();
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        _doc = _web.Load(url);
                    }
                    catch (Exception e)
                    {
                        if (!Results.Contains(url))
                        {
                            Results.Add(url);
                            LinkCheck result = new LinkCheck("Has Issues", response.Url, url, _web.StatusCode);
                            AllLinks.Add(result);
                            LinksWithIssues.Add(result);
                        }
                        return;
                    }

                    switch (_web.StatusCode)
                    {
                        case HttpStatusCode.NoContent:
                        case HttpStatusCode.IMUsed:
                        case HttpStatusCode.Accepted:
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
                        case HttpStatusCode.FailedDependency:
                        case HttpStatusCode.MethodNotAllowed:
                        case HttpStatusCode.ExpectationFailed:
                        case HttpStatusCode.NotAcceptable:
                        case HttpStatusCode.Forbidden:
                        case HttpStatusCode.NetworkAuthenticationRequired:
                        case HttpStatusCode.TooManyRequests:
                        case HttpStatusCode.UnavailableForLegalReasons:
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
                        case HttpStatusCode.Gone:
                        case HttpStatusCode.InternalServerError:
                        case HttpStatusCode.GatewayTimeout:
                        case HttpStatusCode.MisdirectedRequest:
                        case HttpStatusCode.NotImplemented:
                        case HttpStatusCode.ServiceUnavailable:
                        {
                            if (!Results.Contains(url))
                            {
                                Results.Add(url);
                                LinkCheck result = new LinkCheck("Has Issues", response.Url, url, _web.StatusCode);
                                AllLinks.Add(result);
                                LinksWithIssues.Add(result);
                            }

                            break;
                        }
                        case HttpStatusCode.Ambiguous:
                        case HttpStatusCode.Redirect:
                        case HttpStatusCode.Moved:
                        case HttpStatusCode.PermanentRedirect:
                        case HttpStatusCode.TemporaryRedirect:
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
                    StateHasChangedDelegate?.Invoke();
                }));
            }

            return "done";
        }

        public async Task<string> AnalyzeOther()
        {
            AllTitles = new List<TitleDescCheck>();
            HealthyTitles = new List<TitleDescCheck>();
            EmptyTitles = new List<TitleDescCheck>();
            LongTitles = new List<TitleDescCheck>();
            ShortTitles = new List<TitleDescCheck>();

            AllDescriptions = new List<TitleDescCheck>();
            HealthyDescriptions = new List<TitleDescCheck>();
            EmptyDescriptions = new List<TitleDescCheck>();
            LongDescriptions = new List<TitleDescCheck>();
            ShortDescriptions = new List<TitleDescCheck>();

            ImgsWithNoAlt = new Dictionary<string, string>();

            List<Task> tasks = new List<Task>();
            await foreach (var response in Responses)
            {
                tasks.Add(Task.Run(() => TitlesHelper(response)));
                tasks.Add(Task.Run(() => DescriptionsHelper(response)));
                tasks.Add(Task.Run(() => ImgsHelper(response)));
            }
            await Task.WhenAll(tasks);

            return "done";
        }

        public void DescriptionsHelper(ScraperEngineResponse response)
        {
            var results = response.Doc.DocumentNode.SelectNodes("//meta[@name='description']");
            if (results != null)
            {
                foreach (var result in results)
                {
                    var innerText = result.GetAttributeValue("content", "none");
                    if (innerText.Equals("none") || innerText.Equals(""))
                    {
                        TitleDescCheck description =
                            new TitleDescCheck("Missing description", innerText, response.Url);
                        EmptyDescriptions.Add(description);
                        AllDescriptions.Add(description);
                    }
                    else if (innerText.Length > 160)
                    {
                        TitleDescCheck description = new TitleDescCheck("Too long", innerText, response.Url);
                        LongDescriptions.Add(description);
                        AllDescriptions.Add(description);
                    }
                    else if (innerText.Length <= 50)
                    {
                        TitleDescCheck description = new TitleDescCheck("Too short", innerText, response.Url);
                        ShortDescriptions.Add(description);
                        AllDescriptions.Add(description);
                    }
                    else
                    {
                        TitleDescCheck description = new TitleDescCheck("Good", innerText, response.Url);
                        HealthyDescriptions.Add(description);
                        AllDescriptions.Add(description);
                    }
                    StateHasChangedDelegate?.Invoke();
                }
            }
        }

        public void TitlesHelper(ScraperEngineResponse response)
        {
            var results = response.Doc.DocumentNode.SelectNodes("//title");
            if (results != null)
            {
                foreach (var result in results)
                {
                    if (result.OuterHtml == null || result.InnerText == "")
                    {
                        TitleDescCheck title = new TitleDescCheck("Missing title", result.InnerText, response.Url);
                        EmptyTitles.Add(title);
                        AllTitles.Add(title);
                    }
                    else if (result.InnerText.Length >= 60)
                    {
                        TitleDescCheck title = new TitleDescCheck("Too long", result.InnerText, response.Url);
                        LongTitles.Add(title);
                        AllTitles.Add(title);
                    }
                    else if (result.InnerText.Length <= 40)
                    {
                        TitleDescCheck title = new TitleDescCheck("Too short", result.InnerText, response.Url);
                        ShortTitles.Add(title);
                        AllTitles.Add(title);
                    }
                    else
                    {
                        TitleDescCheck title = new TitleDescCheck("Good", result.InnerText, response.Url);
                        HealthyTitles.Add(title);
                        AllTitles.Add(title);
                    }
                    StateHasChangedDelegate?.Invoke();
                }
            }
        }

        public void ImgsHelper(ScraperEngineResponse response)
        {
            //var results = response.Doc.DocumentNode.SelectNodes("//img[@alt='']");
            var results = response.Doc.DocumentNode.SelectNodes("//img[not(@alt)] | //img[@alt='']");
            if (results != null)
            {
                List<Task> tasks = new List<Task>();
                tasks.Add(Task.Run(() =>
                {
                    foreach (var result in results)
                    {
                        if (result.OuterHtml == null) continue;
                        if (!ImgsWithNoAlt.ContainsKey(result.OuterHtml))
                            ImgsWithNoAlt.Add(result.OuterHtml, response.Url);
                    }
                    StateHasChangedDelegate?.Invoke();
                }));
            }
        }

        public async Task<string> AnalyzeImgs()
        {
            ImgsWithNoAlt = new Dictionary<string, string>();

            await foreach (var response in Responses)
            {
                //var results = response.Doc.DocumentNode.SelectNodes("//img[@alt='']");
                var results = response.Doc.DocumentNode.SelectNodes("//img[not(@alt)] | //img[@alt='']");
                if (results != null)
                {
                    List<Task> tasks = new List<Task>();
                    tasks.Add(Task.Run(() =>
                    {
                        foreach (var result in results)
                        {
                            if (result.OuterHtml == null) continue;
                            if (!ImgsWithNoAlt.ContainsKey(result.OuterHtml))
                                ImgsWithNoAlt.Add(result.OuterHtml, response.Url);
                        }
                    }));
                }
            }

            return "done";
        }

        public async Task<string> AnalyzeTitles()
        {
            AllTitles = new List<TitleDescCheck>();
            HealthyTitles = new List<TitleDescCheck>();
            EmptyTitles = new List<TitleDescCheck>();
            LongTitles = new List<TitleDescCheck>();
            ShortTitles = new List<TitleDescCheck>();

            await foreach (var response in Responses)
            {
                var results = response.Doc.DocumentNode.SelectNodes("//title");
                if (results != null)
                {
                    foreach (var result in results)
                    {
                        if (result.OuterHtml == null || result.InnerText == "")
                        {
                            TitleDescCheck title = new TitleDescCheck("Missing title", result.InnerText, response.Url);
                            EmptyTitles.Add(title);
                            AllTitles.Add(title);
                        }
                        else if (result.InnerText.Length >= 60)
                        {
                            TitleDescCheck title = new TitleDescCheck("Too long", result.InnerText, response.Url);
                            LongTitles.Add(title);
                            AllTitles.Add(title);
                        }
                        else if (result.InnerText.Length <= 40)
                        {
                            TitleDescCheck title = new TitleDescCheck("Too short", result.InnerText, response.Url);
                            ShortTitles.Add(title);
                            AllTitles.Add(title);
                        }
                        else
                        {
                            TitleDescCheck title = new TitleDescCheck("Good", result.InnerText, response.Url);
                            HealthyTitles.Add(title);
                            AllTitles.Add(title);
                        }
                    }
                }
            }

            return "done";
        }

        public async Task<string> AnalyzeDescriptions()
        {
            AllDescriptions = new List<TitleDescCheck>();
            HealthyDescriptions = new List<TitleDescCheck>();
            EmptyDescriptions = new List<TitleDescCheck>();
            LongDescriptions = new List<TitleDescCheck>();
            ShortDescriptions = new List<TitleDescCheck>();

            await foreach (var response in Responses)
            {
                var results = response.Doc.DocumentNode.SelectNodes("//meta[@name='description']");
                if (results != null)
                {
                    foreach (var result in results)
                    {
                        var innerText = result.GetAttributeValue("content", "none");
                        if (innerText.Equals("none") || innerText.Equals(""))
                        {
                            TitleDescCheck description =
                                new TitleDescCheck("Missing description", innerText, response.Url);
                            EmptyDescriptions.Add(description);
                            AllDescriptions.Add(description);
                        }
                        else if (innerText.Length > 160)
                        {
                            TitleDescCheck description = new TitleDescCheck("Too long", innerText, response.Url);
                            LongDescriptions.Add(description);
                            AllDescriptions.Add(description);
                        }
                        else if (innerText.Length <= 50)
                        {
                            TitleDescCheck description = new TitleDescCheck("Too short", innerText, response.Url);
                            ShortDescriptions.Add(description);
                            AllDescriptions.Add(description);
                        }
                        else
                        {
                            TitleDescCheck description = new TitleDescCheck("Good", innerText, response.Url);
                            HealthyDescriptions.Add(description);
                            AllDescriptions.Add(description);
                        }
                    }
                }
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
    }
}