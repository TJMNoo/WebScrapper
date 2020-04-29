using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace WebScraper.Data
{
    public class TraverseService
    {
        public Action StateHasChangedDelegate { get; set; }
        private string BaseDomain { get; set; }
        private string WebsiteName { get; set; }
        private HtmlWeb _web = new HtmlWeb();
        private HtmlDocument _doc = new HtmlDocument();
        public List<string> AllUrls { get; set; } = new List<string>();

        //parameter url expects http prefix
        public void SetUrl(string url)
        {
            BaseDomain = "";
            int slashCounter = 0;
            foreach (char c in url)
            {
                if (c == '/') slashCounter++;
                if (slashCounter == 3) break;
                BaseDomain += c;
            }

            slashCounter = 0;
            foreach (char c in BaseDomain)
            {
                if (c == '.') break;
                if (slashCounter == 2) WebsiteName += c;
                if (c == '/') slashCounter++;

            }

            _web.PreRequest = delegate (HttpWebRequest webRequest)
            {
                webRequest.Timeout = 10000;
                return true;
            };
        }

        private string FormatHref(string href)
        {
            if (string.IsNullOrEmpty(href) || href[0]=='#' || href.Contains(" ")) return "invalid";

            Regex checkIfHttps = new Regex(@"^(http|https):\/\/", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            if (checkIfHttps.IsMatch(href) && href.Contains(WebsiteName)) return href;
            else if (checkIfHttps.IsMatch(href)) return "invalid"; //outside website

            if (href[0] == '.' && href[1] == '/') return BaseDomain + href.Substring(1);
            if (href[0] != '/') return BaseDomain + "/" + href; //relative href
            else return BaseDomain + href;
        }

        public async Task BfsAsync(int maxWidth)
        {
            Dictionary<string, bool> visited = new Dictionary<string, bool>();
            Queue<HtmlDocument> sites = new Queue<HtmlDocument>();
            AllUrls = new List<string>();
            int width = 0;

            var root = _web.Load(BaseDomain);
            visited["/"] = true;
            sites.Enqueue(root);
            AllUrls.Add(BaseDomain);

            while (sites.Count != 0)
            {
                var url = sites.Dequeue();
                if (url == null) continue;
                //System.Diagnostics.Debug.Print("Root loaded: " + _web.ResponseUri.AbsoluteUri);
                var neighborUrls = url.DocumentNode.SelectNodes("//a[@href]");
                if (neighborUrls == null) continue;

                int visitedNeighborsCount = 0;
                while (width < maxWidth && visitedNeighborsCount < neighborUrls.Count)
                {
                    List<Task> tasks = new List<Task>();
                    for (int i = visitedNeighborsCount; i < neighborUrls.Count; i++, visitedNeighborsCount++)
                    {
                        HtmlNode neighbor = neighborUrls[i];
                        if (width++ >= maxWidth) break;

                        string href = neighbor.GetAttributeValue("href", string.Empty);
                        if (href == string.Empty || visited.ContainsKey(href)) continue;
                        visited[href] = true;

                        tasks.Add(Task.Run(() =>
                        {
                            string neighborUrl = FormatHref(href);

                            System.Diagnostics.Debug.Print(neighborUrl);
                            if (neighborUrl != "invalid")
                            {
                                //System.Diagnostics.Debug.Print("Loading: " + neighborUrl);
                                HtmlDocument doc = new HtmlDocument();
                                try { doc = _web.Load(neighborUrl); }
                                catch { return; }

                                if (_web.StatusCode != HttpStatusCode.OK) return;

                                //System.Diagnostics.Debug.Print("Loaded: " + neighborUrl);
                                AllUrls.Add(neighborUrl);

                                StateHasChangedDelegate?.Invoke();
                                sites.Enqueue(doc);
                            }
                        }));
                    }

                    await Task.WhenAll(tasks);
                    width = AllUrls.Count;
                }
            }
        }

        public void Dfs()
        {
            return;
        }
    }
}
