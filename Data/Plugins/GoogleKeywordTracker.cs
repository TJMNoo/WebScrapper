using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net;
using WebScraper.Data.Engine;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration.UserSecrets;
using Newtonsoft.Json.Linq;
using WebScraper.Data.ApiData;


namespace WebScraper.Data.Plugins
{
    public class Result
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public int Position { get; set; }
        public bool Found { get; set; }

        public Result(string url, string title, int position, bool found = false)
        {
            Url = url;
            Title = title;
            Position = position;
            Found = found;
        }
    }
    public class Advert
    {
        public readonly string Url;
        public readonly string Domain;
        public readonly string Subject;
        public readonly string Description;

        public Advert(string u, string d, string s, string desc)
        {
            Url = u;
            Domain = d;
            Subject = s;
            Description = desc;
        }
    }
    
    public class GoogleKeywordTracker
    {
        public Action StateHasChangedDelegate { get; set; }
        private ScraperEngine Engine { get; set; } = new ScraperEngine();
        public List<Advert> Reklame { get; set; } = new List<Advert>();
        public List<Result> Organic { get; set; } = new List<Result>();
        public List<SerpPost> OrganicFound { get; set; } = new List<SerpPost>();
        private int Position { get; set; } = 0;
        private bool Found { get; set; }
        
        public async Task<List<SerpPost>> TrackSomething(string keyword, string userAgent, string location, string enteredpage, string username)
        {
            var responses = Engine.GetGooglePages(keyword, userAgent, location, 4, 5000);
            await foreach (var response in responses)
            {
                GiveAds(response);

                var resultsOnly = response.Doc.GetElementbyId("rso").SelectNodes(".//div[@class='g']");//kw, position, datetime, url
                if (resultsOnly == null) return null;
                foreach (var result in resultsOnly)
                {
                    Position++;
                    var divClass = result.GetAttributeValue("class", "");
                    if (divClass != "g") continue;

                    var a = result.SelectSingleNode(".//a");
                    var url = a.Attributes["href"].Value;
                    var title = a.SelectSingleNode(".//h3").InnerText;
                    Organic.Add(new Result(url,title,Position));
                    StateHasChangedDelegate?.Invoke();

                    if (enteredpage != null) //Checks if entered URL is on Google search 
                    {
                        if (!enteredpage.StartsWith("https://")) //if http || wwww
                        {
                            if (!enteredpage.StartsWith("http://")) //uri needs http
                            {
                                enteredpage = "http://" + enteredpage;
                            }
                            Uri uri = new Uri(enteredpage);
                            var temp = uri.Host;
                            if (temp.Contains("www.")) temp = temp.Substring(4);
                            
                            if (url.Contains(temp))
                            {
                                Found = true;
                                string date = DateTime.Now.ToString("yyyy-MM-dd");
                                Organic[Organic.Count - 1].Found = true;
                                OrganicFound.Add(new SerpPost(date, keyword, enteredpage, Position, username));
                            }
                        }
                        else //if https
                        {
                            enteredpage = enteredpage.Substring(8); //remove https part bcs uri needs http
                            enteredpage = "http://" + enteredpage;
                            Uri uri = new Uri(enteredpage);
                            var temp = uri.Host;
                            if (temp.Contains("www.")) temp = temp.Substring(4);
                            if (url.Contains(temp))
                            {
                                Found = true;
                                string date = DateTime.Now.ToString("yyyy-MM-dd");
                                Organic[Organic.Count - 1].Found = true;
                                OrganicFound.Add(new SerpPost(date, keyword, enteredpage, Position, username));
                            }
                        }
                    }
                }
            }
            if (OrganicFound.Count == 0)
            {
                string date = DateTime.Now.ToString("yyyy-MM-dd");
                OrganicFound.Add(new SerpPost(date, keyword, enteredpage, 101, username));
            }
            return OrganicFound;
        }

        private void GiveAds(ScraperEngineResponse response)
        {
            var doorstep = response.Doc.GetElementbyId("tads");
            if (doorstep != null)
            {
                HtmlNodeCollection topads;
                try
                {
                    topads = response.Doc.GetElementbyId("tads").SelectNodes("//li[@class='ads-ad']");
                    if (topads == null) return;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                foreach (var adresult in topads)
                {
                    var divClass = adresult.GetAttributeValue("class", "");
                    if (divClass != "ads-ad") continue;
                    var url = adresult.SelectSingleNode(".//cite[@class='UdQCqe']").InnerText;
                    var domain = url;
                    var filtrirana = "";
                    var filtereddomain = "";
                    if (domain != null)
                    {
                        if (!domain.StartsWith("https://")) //if domain http || www
                        {
                            if (!domain.StartsWith("http://")) //uri needs http
                            {
                                domain = "http://" + domain;
                            }
                            Uri uri = new Uri(domain);
                            filtereddomain = uri.Host; //e.g. http://www.example.com
                            string[] subdomain = filtereddomain.Split(new char[] { '.' }); //split string 
                            if (filtereddomain.Contains("www."))
                            {
                                var count = subdomain[0].Length; //remove www
                                filtrirana = filtereddomain.Remove(0, count + 1);
                            }
                            else
                            {
                                var count = subdomain[0].Length;
                                filtrirana = filtereddomain.Remove(0, count + 1);
                            }
                        }
                        else //if domain https
                        {
                            domain = domain.Substring(8);
                            domain = "http://" + domain;
                            Uri uri = new Uri(domain);
                            filtereddomain = uri.Host;
                            if (filtereddomain.Contains("www.")) filtereddomain = filtereddomain.Substring(4);
                            Console.WriteLine(filtereddomain);
                        }
                    }
                    if (!filtereddomain.StartsWith("https://")) filtereddomain = "https://" + filtereddomain;

                    var subject = adresult.SelectSingleNode(".//h3").InnerText;
                    WebUtility.HtmlDecode(subject);
                    var desc = adresult.SelectSingleNode(".//div[@class='ads-creative']").InnerText;
                    Advert tempres = new Advert(filtereddomain, filtrirana, subject, desc);
                    Reklame.Add(tempres);
                    StateHasChangedDelegate?.Invoke();
                }
            }
        }
    }
}