using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;

namespace WebScraper.Data.ApiData
{
    public class HealthAnalysis
    {
        public ObjectId Id { get; set; }
        public List<int> NumOfAllLinks { get; set; } = new List<int>();
        public List<int> NumOfHealthyLinks { get; set; } = new List<int>();
        public List<int> NumOfBrokenLinks { get; set; } = new List<int>();
        public List<int> NumOfBlockedLinks { get; set; } = new List<int>();
        public List<int> NumOfRedirectLinks { get; set; } = new List<int>();
        public List<int> NumOfLinksWithIssues { get; set; } = new List<int>();
        public List<int> NumOfImgsWithNoAlt { get; set; } = new List<int>();
        public List<int> NumOfAllTitles { get; set; } = new List<int>();
        public List<int> NumOfHealthyTitles { get; set; } = new List<int>();
        public List<int> NumOfEmptyTitles { get; set; } = new List<int>();
        public List<int> NumOfLongTitles { get; set; } = new List<int>();
        public List<int> NumOfShortTitles { get; set; } = new List<int>();
        public List<int> NumOfAllDescriptions { get; set; } = new List<int>();
        public List<int> NumOfHealthyDescriptions { get; set; } = new List<int>();
        public List<int> NumOfEmptyDescriptions { get; set; } = new List<int>();
        public List<int> NumOfLongDescriptions { get; set; } = new List<int>();
        public List<int> NumOfShortDescriptions { get; set; } = new List<int>();
        public List<string> Dates { get; set; } = new List<string>();
        public string Url { get; set; }
        public string UserFk { get; set; }

        public HealthAnalysis() { }
        public HealthAnalysis(JToken health)
        {
            if (health == null || !health.HasValues) return;
            Url = (string) health["url"];
            UserFk = (string) health["userFk"];
            NumOfAllLinks = health["numOfAllLinks"].ToObject<List<int>>();
            NumOfHealthyLinks = health["numOfHealthyLinks"].ToObject<List<int>>();
            NumOfBrokenLinks = health["numOfBrokenLinks"].ToObject<List<int>>();
            NumOfBlockedLinks = health["numOfBlockedLinks"].ToObject<List<int>>();
            NumOfRedirectLinks = health["numOfRedirectLinks"].ToObject<List<int>>();
            NumOfLinksWithIssues = health["numOfLinksWithIssues"].ToObject<List<int>>();
            NumOfImgsWithNoAlt = health["numOfImgsWithNoAlt"].ToObject<List<int>>();
            NumOfAllTitles = health["numOfAllTitles"].ToObject<List<int>>();
            NumOfHealthyTitles = health["numOfHealthyTitles"].ToObject<List<int>>();
            NumOfEmptyTitles = health["numOfEmptyTitles"].ToObject<List<int>>();
            NumOfLongTitles = health["numOfLongTitles"].ToObject<List<int>>();
            NumOfShortTitles = health["numOfShortTitles"].ToObject<List<int>>();
            NumOfAllDescriptions = health["numOfAllDescriptions"].ToObject<List<int>>();
            NumOfHealthyDescriptions = health["numOfHealthyDescriptions"].ToObject<List<int>>();
            NumOfEmptyDescriptions = health["numOfEmptyDescriptions"].ToObject<List<int>>();
            NumOfLongDescriptions = health["numOfLongDescriptions"].ToObject<List<int>>();
            NumOfShortDescriptions = health["numOfShortDescriptions"].ToObject<List<int>>();
            Dates = health["dates"].ToObject<List<string>>();
        }
    }
}
