using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;

namespace WebScraper.Data.ApiData
{
    public class Serp
    {
        public ObjectId Id { get; set; }
        public string Keyword { get; set; }
        public string Url { get; set; }
        public List<string> Dates { get; set; } = new List<string>();
        public List<int> Positions { get; set; } = new List<int>();
        public string UserFk { get; set; }

        public Serp() { }
        public Serp(JToken serp)
        {
            Keyword = (string) serp["keyword"];
            Positions = serp["positions"].ToObject<List<int>>();
            Dates = serp["dates"].ToObject<List<string>>();
            Url = (string) serp["url"];
            UserFk = (string) serp["userFk"];
        }
    }
}
