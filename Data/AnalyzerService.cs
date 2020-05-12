using System;
using System.Collections.Generic;
using System.Net;
using HtmlAgilityPack;

namespace WebScraper.Data
{
    public class AnalyzerService
    {
        public bool CheckIfBroken(string pageUrl)
        {
            var web = new HtmlWeb();
            web.Load(pageUrl);
            //to do: staviti sve moguce codeove u listu za bolji if
            if (web.StatusCode == HttpStatusCode.NotFound || web.StatusCode == HttpStatusCode.BadRequest)
            {
                BrokenLinks(pageUrl);
                return true;
            }
            return false;
        }
        
        public List<string> BrokenLinks(string link)
        {
            if (String.IsNullOrEmpty(link)) return new List<string>();
            List<string> links = new List<string>();
            links.Add(link);
            Console.WriteLine(link + "ovo je broken!");
            return links;
        }
    }
}