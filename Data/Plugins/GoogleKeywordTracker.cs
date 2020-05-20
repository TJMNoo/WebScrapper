using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Policy;
using System.Text.RegularExpressions;
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

        public List<Result> Rezultatici { get; set; } = new List<Result>(); //lista za pronadene rezultate

        public async Task<List<Result>> TrackSomething(string keyword, string userAgent, string location, string enteredpage)
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
                    if (enteredpage != null)
                    {
                        if (!enteredpage.StartsWith("https://")) //ako je http, wwww
                        {
                            if (!enteredpage.StartsWith("http://")) //uri needs http
                            {
                                enteredpage = "http://" + enteredpage;
                            }
                            Uri uri = new Uri(enteredpage);
                            string url = uri.Host;
                            if (url.Contains("www.")) url = url.Substring(4);
                            if (href.Contains(url))
                            {
                                Result s = new Result(position, href, title);
                                Rezultatici.Add(s);
                            }
                        }
                        else //ako je https
                        {
                            enteredpage = enteredpage.Substring(8);
                            enteredpage = "http://" + enteredpage;
                            Uri uri = new Uri(enteredpage);
                            string url = uri.Host;
                            if (url.Contains("www.")) url = url.Substring(4);
                            if (href.Contains(url))
                            {
                                Result s = new Result(position, href, title);
                                Rezultatici.Add(s);
                            }
                        }
                    }
                    
                    Result r = new Result(position, href, title);
                    rezultati.Add(r);
                }

                //Bekend i ostatak
                foreach (var r in rezultati)
                {
                    Console.WriteLine(r.Position + " | " + r.Title + " | " + r.Url);
                }

                foreach (var r in Rezultatici)
                {
                    Console.WriteLine("Pronadeno je na googleu: " + r.Position + " | " + r.Title + " | " + r.Url);
                }
            }
            return rezultati;
        }
    }
}