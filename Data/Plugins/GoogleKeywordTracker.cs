using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net;
using WebScraper.Data.Engine;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration.UserSecrets;


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

        public List<Result> Rezultati { get; set; } = new List<Result>();

        public List<Found> Pronadeni { get; set; } = new List<Found>();

        public List<Advert> Reklame { get; set; } = new List<Advert>(); 

        public async Task<List<Result>> TrackSomething(string keyword, string userAgent, string location, string enteredpage)
        {
            var responses = Engine.GetGooglePages(keyword, userAgent, location, 2, 10000);
            var position = 0;

            await foreach (var response in responses)
            {
                //kw, position, datetime, url
                //Regularno skrejpanje
                var resultsOnly = response.Doc.GetElementbyId("rso").SelectNodes(".//div[@class='g']");
                if (resultsOnly == null) return new List<Result>();
                foreach (var result in resultsOnly)
                {
                    position++;
                    var divClass = result.GetAttributeValue("class", "");
                    if (divClass != "g") continue;
                    var a = result.SelectSingleNode(".//a");
                    var title = a.SelectSingleNode(".//h3").InnerText;
                    var href = result.SelectSingleNode(".//a").Attributes["href"].Value;
                    
                    //If users page found on google
                    if (enteredpage != null)
                    {
                        if (!enteredpage.StartsWith("https://")) //if http || wwww
                        {
                            if (!enteredpage.StartsWith("http://")) //uri needs http
                            {
                                enteredpage = "http://" + enteredpage;
                            }
                            Uri uri = new Uri(enteredpage);
                            var url = uri.Host;
                            if (url.Contains("www.")) url = url.Substring(4);
                            if (href.Contains(url))
                            {
                                DateTime regdatum = DateTime.Now;
                                Found temp = new Found(position, url, regdatum);
                                Pronadeni.Add(temp);
                            }
                        }
                        else //if https
                        {
                            enteredpage = enteredpage.Substring(8); //remove https part bcs uri needs http
                            enteredpage = "http://" + enteredpage;
                            Uri uri = new Uri(enteredpage);
                            var url = uri.Host;
                            if (url.Contains("www.")) url = url.Substring(4);
                            if (href.Contains(url))
                            {
                                DateTime regdatum = DateTime.Now;;
                                Found temp = new Found(position, url, regdatum);
                                Pronadeni.Add(temp);
                            }
                        }
                    }
                    DateTime datum = DateTime.Now;
                    Result tempres = new Result(keyword, position, href, datum);
                    Rezultati.Add(tempres);
                }

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
                        //Dodaj vrijeme, reklame imaju vrijeme
                        Advert tempres = new Advert(filtereddomain, filtrirana,subject, desc);
                        Reklame.Add(tempres);
                    }
                    //subject(naslov), description line 1, description line 2, url, domain
                }
                else
                {
                    Console.WriteLine("Nema reklama");
                    if(doorstep == null) return new List<Result>();
                }
                //Bekend i ostatak
            }
            return Rezultati;
        }
    }
}