using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WebScraper.Data.ApiData
{
    public class SerpPost
    {
        public string Date { get; set; }
        public string Keyword { get; set; }
        public string Url { get; set; }
        public int Position { get; set; }
        public string Username { get; set; }

        public SerpPost() { }

        public JObject ConvertToJObject()
        {
            JObject data = new JObject();
            data["Date"] = Date;
            data["Keyword"] = Keyword;
            data["Url"] = Url;
            data["Position"] = Position;
            data["Username"] = Username;
            return data;
        }
    }
}
