using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebScraper.Data.Engine;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace WebScraper.Data.Plugins
{
    public class Result
    {
        public readonly int Position;
        public readonly string Url;
        public readonly string Title;

        public Result(int p, string u, string t)
        {
            Position = p;
            Url = u;
            Title = t;
        }
    }

    public class GoogleKeywordTracker
    {
        private ScraperEngine Engine { get; set; } = new ScraperEngine();
        public List<int> TempResults { get; set; } = new List<int>();

        public async Task<List<Result>> TrackSomething(string keyword, string userAgent, string location)
        {
            var responses = Engine.GetGooglePages(keyword, userAgent, location, 2, 10000);
            int i = 1;
            List<Result> rezultati = new List<Result>();
            int position = 0;

            await foreach (var response in responses)
            {
                var resultsOnly = response.Doc.GetElementbyId("rso").SelectNodes(".//div[@class='g']");
                if (resultsOnly == null) return new List<Result>();
                
                TempResults.Add(response.Doc.Text.Length);
                
                foreach (var result in resultsOnly)
                {
                    position++;
                    string divClass = result.GetAttributeValue("class", "");
                    if (divClass != "g") continue;
                    var a = result.SelectSingleNode(".//a");
                    var title = a.SelectSingleNode(".//h3").InnerText;
                    var href = result.SelectSingleNode(".//a").Attributes["href"].Value;

                    Result r = new Result(position, href, title);
                    rezultati.Add(r);
                }

                //Bekend i ostatak
                foreach (var r in rezultati)
                {
                    Console.WriteLine(r.Position + " | " + r.Title + " | " + r.Url);
                    //System.Diagnostics.Debug.Print(r.pozicija + " | " + r.title + " | " + r.url);
                }
            }
            return rezultati;
        }
    }
}
