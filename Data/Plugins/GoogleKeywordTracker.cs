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
        public readonly string Keyword;
        public readonly int Position;
        public readonly string Url;
        public readonly DateTime Date;
        public Result(string k, int p, string u, DateTime d)
        {
            Keyword = k;
            Position = p;
            Url = u;
            Date = d;
        }
    }

    public class Found
    {
        public readonly int Position;
        public readonly string Url;
        public readonly DateTime Date;

        public Found(int p, string u, DateTime d)
        {
            Position = p;
            Url = u;
            Date = d;
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
        private ScraperEngine Engine { get; set; } = new ScraperEngine();
        public List<Advert> Reklame { get; set; } = new List<Advert>();
        public SerpPost NewSerp = new SerpPost();
        private string _feedback { get; set; } = "";
        private JObject _response { get; set; }
        private string Datum { get; set; }
        private int Position { get; set; }
        private string Url { get; set; }
        private bool Found { get; set; }
        private int TempPosition { get; set; }

        public async Task<string> TrackSomething(string keyword, string userAgent, string location, string enteredpage, string username)
        {
            var responses = Engine.GetGooglePages(keyword, userAgent, location, 2, 10000);
            await foreach (var response in responses)
            {
                var resultsOnly = response.Doc.GetElementbyId("rso").SelectNodes(".//div[@class='g']");//kw, position, datetime, url
                if (resultsOnly == null) return null;
                foreach (var result in resultsOnly)
                {
                    Position++;
                    var divClass = result.GetAttributeValue("class", "");
                    if (divClass != "g") continue;
                    Url = result.SelectSingleNode(".//a").Attributes["href"].Value;
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
                            
                            if (Url.Contains(temp))
                            {
                                Found = true;
                                NewSerp.Position = Position;
                                Datum = DateTime.Now.ToString("yyyy-MM-dd");
                                NewSerp.Date = Datum;
                                Console.WriteLine(NewSerp.Date);
                                NewSerp.Url = Url;
                                Console.WriteLine(NewSerp.Url);
                                NewSerp.Keyword = keyword;
                                Console.WriteLine(NewSerp.Keyword);
                                Console.WriteLine(NewSerp.Position);
                                NewSerp.Username = username;
                                Console.WriteLine(NewSerp.Username);
                                var data = NewSerp.ConvertToJObject();
                                RequestService requester = new RequestService();
                                string dataStr = data.ToString(Newtonsoft.Json.Formatting.None);
                                _response = requester.Post("https://webscraperapi.herokuapp.com/api/serp", dataStr, "application/json");
                                _feedback = (string)_response["message"];
                                break;
                            }
                            else
                            {
                                Found = false;
                            }
                        }
                        else //if https
                        {
                            enteredpage = enteredpage.Substring(8); //remove https part bcs uri needs http
                            enteredpage = "http://" + enteredpage;
                            Uri uri = new Uri(enteredpage);
                            var temp = uri.Host;
                            if (temp.Contains("www.")) temp = temp.Substring(4);
                            if (Url.Contains(temp))
                            {
                                Found = true;
                                NewSerp.Position = Position;
                                Datum = DateTime.Now.ToString("yyyy-MM-dd");
                                NewSerp.Date = Datum;
                                Console.WriteLine(NewSerp.Date);
                                NewSerp.Url = Url;
                                Console.WriteLine(NewSerp.Url);
                                NewSerp.Keyword = keyword;
                                Console.WriteLine(NewSerp.Keyword);
                                Console.WriteLine(NewSerp.Position);
                                NewSerp.Username = username;
                                Console.WriteLine(NewSerp.Username);
                                var data = NewSerp.ConvertToJObject();
                                RequestService requester = new RequestService();
                                string dataStr = data.ToString(Newtonsoft.Json.Formatting.None);
                                _response = requester.Post("https://webscraperapi.herokuapp.com/api/serp", dataStr, "application/json");
                                _feedback = (string)_response["message"];
                                break;
                            }
                            else
                            {
                                Found = false;
                            }
                        }
                    }
                    Console.WriteLine(Url);
                }
                Datum = DateTime.Now.ToString("yyyy-MM-dd");
                NewSerp.Date = Datum;
                Console.WriteLine(NewSerp.Date);
                NewSerp.Url = Url;
                Console.WriteLine(NewSerp.Url);
                NewSerp.Keyword = keyword;
                Console.WriteLine(NewSerp.Keyword);
                Console.WriteLine(NewSerp.Position);
                NewSerp.Username = username;
                Console.WriteLine(NewSerp.Username);
                var data3 = NewSerp.ConvertToJObject();
                RequestService requester3 = new RequestService();
                string dataStr3 = data3.ToString(Newtonsoft.Json.Formatting.None);
                _response = requester3.Post("https://webscraperapi.herokuapp.com/api/serp", dataStr3, "application/json");
                _feedback = (string)_response["message"];
                
                var doorstep = response.Doc.GetElementbyId("tads");
                if (doorstep != null)
                {
                    HtmlNodeCollection topads;
                    try
                    {
                        topads = response.Doc.GetElementbyId("tads").SelectNodes("//li[@class='ads-ad']");
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
                                string[] subdomain = filtereddomain.Split(new char[] {'.'}); //split string 
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
                        if (!filtereddomain.StartsWith("https://"))
                        {
                            filtereddomain = "https://" + filtereddomain;
                        }
                        DateTime date = DateTime.Now;
                        Console.WriteLine("Domain: " + filtrirana);
                        Console.WriteLine("Filtered URL: " + filtereddomain);
                        var subject = adresult.SelectSingleNode(".//h3").InnerText;
                        WebUtility.HtmlDecode(subject);
                        Console.WriteLine("Naslov: " + subject);
                        var desc = adresult.SelectSingleNode(".//div[@class='ads-creative']").InnerText;
                        Console.WriteLine("Desc: " + desc);
                        Console.WriteLine("\n");
                        Advert tempres = new Advert(filtereddomain, filtrirana,subject, desc);
                        Reklame.Add(tempres);
                    }
                }
                else
                {
                    Console.WriteLine("Nema reklama");
                    if(doorstep == null) return null;
                }
                //Bekend i ostatak
            }
            return "done";
        }
    }
}