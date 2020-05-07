using System;
using System.Collections.Generic;
using System.IO;
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

        private Dictionary<Regex, bool> RobotsDisallowedUrls { get; set; } = new Dictionary<Regex, bool>();
        private Dictionary<Regex, bool> RobotsAllowedUrls { get; set; } = new Dictionary<Regex, bool>();
        public List<string> AllUrls { get; set; } = new List<string>();
        public List<string> BusinessLogicResults { get; set; } = new List<string>();

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

            LoadRobotsTxt();
        }

        private Regex ConvertRobotsCommandToRegex(string robotsCommand)
        {
            string regexStr = "";
            for (int i = 0; i < robotsCommand.Length; i++)
            {
                if (robotsCommand[i] == '?' || robotsCommand[i] == '/' || robotsCommand[i] == '.') regexStr += "\\" + robotsCommand[i];
                else if (robotsCommand[i] == '*') regexStr += "[a-zA-Z0-9]" + robotsCommand[i];
                else regexStr += robotsCommand[i];
            }
            Regex reg = new Regex(@regexStr, RegexOptions.Compiled);
            return reg;
        }

        private void LoadRobotsTxt()
        {
            RobotsDisallowedUrls.Clear();
            RobotsAllowedUrls.Clear();

            WebClient client = new WebClient();
            client.Headers.Set("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:12.0) Gecko/20100101 Firefox/12.0");
            string str = client.DownloadString(BaseDomain + "/robots.txt");
            if (string.IsNullOrEmpty(str)) return;

            string[] commands = str.Split("\n", StringSplitOptions.RemoveEmptyEntries);

            bool foundUserAgent = false;
            foreach (var command in commands)
            {
                // System.Diagnostics.Debug.Print(command);
                string[] parts = command.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                if (parts[0].ToLower() == "user-agent:" && parts[1] == "*") foundUserAgent = true;
                else if (foundUserAgent && parts[0] == "Disallow:")
                {
                    Regex reg = ConvertRobotsCommandToRegex(parts[1]);
                    RobotsDisallowedUrls[reg] = true;
                }
                else if (foundUserAgent && parts[0] == "Allow:")
                {
                    Regex reg = ConvertRobotsCommandToRegex(parts[1]);
                    RobotsAllowedUrls[reg] = true;
                }
                else if (foundUserAgent && parts[0].ToLower() == "user-agent:") break;
            }
        }

        private bool DisallowedRobotsTxt(string href)
        {
            foreach (var entry in RobotsDisallowedUrls)
                if (entry.Key.IsMatch(href)) return true;
            return false;
        }

        private bool AllowedRobotsTxt(string href)
        {
            foreach(var entry in RobotsAllowedUrls)
                if (entry.Key.IsMatch(href)) return true;
            return false;
        }

        private string FormatHref(string href)
        {
            if (string.IsNullOrEmpty(href) || href[0]=='#' || href.Contains(" ")) return "invalid";
            bool allowed = AllowedRobotsTxt(href);
            if (!allowed && DisallowedRobotsTxt(href)) return "robots.txt disallowed";

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
                            if (neighborUrl != "invalid" && neighborUrl != "robots.txt disallowed")
                            {
                                //System.Diagnostics.Debug.Print("Loading: " + neighborUrl);
                                HtmlDocument doc = new HtmlDocument();
                                try { doc = _web.Load(neighborUrl); }
                                catch { return; }

                                if (_web.StatusCode != HttpStatusCode.OK) return;

                                BusinessLogicService businessLogic = new BusinessLogicService(doc);
                                Dictionary<string, string> rules = new Dictionary<string, string>();
                                rules["h1"] = "all";
                                rules["p"] = "all";
                                rules["img"] = "all";
                                rules["aHrefContains"] = "www.";
                                var results = businessLogic.Apply(rules);
                                BusinessLogicResults.AddRange(results);

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
