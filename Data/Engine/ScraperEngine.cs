using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace WebScraper.Data.Engine
{
    public class ScraperEngine
    {
        private ScraperEngineHelper Helper { get; set; } = new ScraperEngineHelper(); 
        
        public async IAsyncEnumerable<ScraperEngineResponse> GetGooglePages(string keyword, string location = "us", int pages = 2, int delay = 10000)
        {
            HtmlWeb web = new HtmlWeb();
            web.PreRequest = delegate (HttpWebRequest webRequest) {webRequest.Timeout = 10000; return true;};
            
            List<Task<ScraperEngineResponse>> tasks = new List<Task<ScraperEngineResponse>>();
            for (int i = 0; i < pages; i++)
            {
                //start=0 -> page1, start=1-10 -> page 2, start=11-20 -> page3...
                string nextPage = "https://google.com/search?gl=" + location + "&q=" + keyword + "&start=" + i * 10;
                var task = Task.Run(() =>
                {
                    try
                    {
                        HtmlDocument doc = new HtmlDocument();
                        doc = web.Load(nextPage);
                        return new ScraperEngineResponse(200, doc, nextPage);
                    }
                    catch(Exception e)
                    {
                        return new ScraperEngineResponse(500,null, null, e.Message);
                    }
                });
                yield return await task;
                Thread.Sleep(delay);
            }
        }

        //Gets non-404 HtmlDocument pages starting from root page
        public async IAsyncEnumerable<ScraperEngineResponse> GetDocsFromRoot(string root, int maxPageLimit, int delay)
        {
            Helper.SetUrl(root);
            Dictionary<string, bool> visited = new Dictionary<string, bool>();
            Queue<HtmlDocument> q = new Queue<HtmlDocument>();
            List<HtmlDocument> currentDocs = new List<HtmlDocument>();
            HtmlWeb web = new HtmlWeb();
            web.PreRequest = delegate (HttpWebRequest webRequest) { webRequest.Timeout = 10000; return true; };

            HtmlDocument rootDoc = new HtmlDocument();
            try { rootDoc = web.Load(root); }
            catch { yield break; }
            //visited["/"] = true;
            q.Enqueue(rootDoc);
            currentDocs.Add(rootDoc);

            int width = 0, pagesAdded = 0;
            while (q.Count != 0)
            {
                var url = q.Dequeue();
                if (url == null) continue;
                var neighborUrls = url.DocumentNode.SelectNodes("//a[@href]");
                if (neighborUrls == null) continue;

                int visitedNeighborsCount = 0;
                while (width < maxPageLimit && visitedNeighborsCount < neighborUrls.Count)
                {
                    List<Task<ScraperEngineResponse>> tasks = new List<Task<ScraperEngineResponse>>();
                    for (int i = visitedNeighborsCount; i < neighborUrls.Count; i++, visitedNeighborsCount++)
                    {
                        HtmlNode neighbor = neighborUrls[i];
                        if (width++ >= maxPageLimit) break;

                        string href = neighbor.GetAttributeValue("href", string.Empty);
                        if (href == string.Empty || visited.ContainsKey(href)) continue;
                        visited[href] = true;

                        var ts = new CancellationTokenSource();
                        CancellationToken ct = ts.Token;
                        tasks.Add(Task.Run(() =>
                        {
                            string neighborUrl = Helper.FormatHref(href);

                            HtmlDocument doc = new HtmlDocument();
                            if (neighborUrl != "invalid" && neighborUrl != "robots.txt disallowed")
                            {
                                try { doc = web.Load(neighborUrl); }
                                catch { ts.Cancel(); }
                                if (web.StatusCode != HttpStatusCode.OK) ts.Cancel();
                                currentDocs.Add(doc);
                                q.Enqueue(doc);
                            }

                            return new ScraperEngineResponse(200, doc, neighborUrl);
                        }, ct));
                    }

                    foreach (var response in tasks) yield return await response;
                    pagesAdded += currentDocs.Count;
                    width = pagesAdded;
                    currentDocs.Clear();
                }
            }
        }

        //Gets ALL (string) hrefs starting from root
        public async IAsyncEnumerable<ScraperEngineResponse> GetHrefsFromRoot(string root, int maxHrefLimit, int delay)
        {
            Helper.SetUrl(root);
            Dictionary<string, bool> visited = new Dictionary<string, bool>();
            Queue<HtmlDocument> q = new Queue<HtmlDocument>();
            List<string> currentHrefs = new List<string>();
            HtmlWeb web = new HtmlWeb();
            web.PreRequest = delegate (HttpWebRequest webRequest) { webRequest.Timeout = 10000; return true; };

            HtmlDocument rootDoc = new HtmlDocument();
            try { rootDoc = web.Load(root); }
            catch { yield break; }
            //visited["/"] = true;
            q.Enqueue(rootDoc);
            currentHrefs.Add(root);

            int width = 0, pagesAdded = 0;
            while (q.Count != 0)
            {
                var url = q.Dequeue();
                if (url == null) continue;
                var neighborUrls = url.DocumentNode.SelectNodes("//a[@href]");
                if (neighborUrls == null) continue;

                int visitedNeighborsCount = 0;
                while (width < maxHrefLimit && visitedNeighborsCount < neighborUrls.Count)
                {
                    List<Task<ScraperEngineResponse>> tasks = new List<Task<ScraperEngineResponse>>();
                    for (int i = visitedNeighborsCount; i < neighborUrls.Count; i++, visitedNeighborsCount++)
                    {
                        HtmlNode neighbor = neighborUrls[i];
                        if (width++ >= maxHrefLimit) break;

                        string href = neighbor.GetAttributeValue("href", string.Empty);
                        if (href == string.Empty || visited.ContainsKey(href)) continue;
                        visited[href] = true;
                        currentHrefs.Add(href);

                        var ts = new CancellationTokenSource();
                        CancellationToken ct = ts.Token;
                        tasks.Add(Task.Run(() =>
                        {
                            string neighborUrl = Helper.FormatHref(href);
                            HtmlDocument doc = new HtmlDocument();
                            if (neighborUrl != "invalid" && neighborUrl != "robots.txt disallowed")
                            {
                                try
                                {
                                    doc = web.Load(neighborUrl);
                                    if (web.StatusCode != HttpStatusCode.OK) q.Enqueue(doc);
                                }
                                catch { return new ScraperEngineResponse(200, null, neighborUrl); }
                                
                            }
                            return new ScraperEngineResponse(200, null, neighborUrl);
                        }, ct));
                    }

                    foreach (var task in tasks) yield return await task;

                    pagesAdded += currentHrefs.Count;
                    width = pagesAdded;
                    currentHrefs.Clear();
                }
            }
        }
    }
}
