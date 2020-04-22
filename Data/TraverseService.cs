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
        private string BaseDomain { get; set; }
        private Dictionary<string, bool> _visited = new Dictionary<string, bool>();
        private HtmlWeb _web = new HtmlWeb();
        private HtmlDocument _doc = new HtmlDocument();
        public List<string> AllUrls { get; set; } = new List<string>();

        public void SetUrl(string url)
        {
            BaseDomain = url;
        }

        //if <a> href link has prefix 'http://' then it leads to an outside website, otherwise it's relative e.g. '/products/'
        private string FormatHref(string href)
        {
            if (string.IsNullOrEmpty(href) || href[0] == '#') return "invalid";

            Regex checkIfHttps = new Regex(@"^(http|https):\/\/", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            if (checkIfHttps.IsMatch(href)) return "invalid";
            else return BaseDomain + href;
        }

        public void Bfs(int maxWidth)
        {
            _visited.Clear();

            string root = BaseDomain;
            _visited[root] = true;

            Queue<string> urls = new Queue<string>();
            urls.Enqueue(root);

            AllUrls = new List<string>();
            int width = 0;
            while (urls.Count != 0)
            {
                string url = urls.Dequeue();

                System.Diagnostics.Debug.Print(url);
                AllUrls.Add(url);

                if (width++ == maxWidth - 1) break;

                try
                {
                    _doc = _web.Load(url);
                }
                catch
                {
                    continue;
                }

                //Console.WriteLine(url);


                var neighborUrls = _doc.DocumentNode.SelectNodes("//a");
                if (neighborUrls == null) continue;

                foreach (var neighbor in neighborUrls)
                {
                    if (neighbor.Attributes["href"] == null) continue;

                    string href = neighbor.Attributes["href"].Value;
                    string neighborUrl = FormatHref(href);

                    if (neighborUrl != "invalid" && !_visited.ContainsKey(neighborUrl))
                    {
                        _visited[neighborUrl] = true;
                        urls.Enqueue(neighborUrl);
                    }
                }
            }
        }

        public void Dfs()
        {
            return;
        }
    }
}