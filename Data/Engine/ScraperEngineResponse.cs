using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace WebScraper.Data.Engine
{
    public class ScraperEngineResponse
    {
        public int Status { get; set; }
        public HtmlDocument Doc { get; set; }
        public string Url { get; set; }
        public object Optional { get; set; }

        public ScraperEngineResponse(int status, HtmlDocument doc = null, string url = null, object optional = null)
        {
            Status = status;
            Doc = doc;
            Url = url;
            Optional = optional;
        }
    }
}
