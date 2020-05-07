using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace WebScraper.Data
{
    public class BusinessLogicService
    {
        private SelectorService Selector { get; set; }
        private HtmlDocument Doc { get; set; }

        public BusinessLogicService(HtmlDocument doc)
        {
            Doc = doc;
            Selector = new SelectorService(doc);
        }

        public List<string> Apply(Dictionary<string, string> rules)
        {
            List<string> results = new List<string>();
            if (rules.ContainsKey("h1")) results.AddRange(Selector.FilterTag("h1"));
            else if(rules.ContainsKey("p")) results.AddRange(Selector.FilterTag("p"));
            else if (rules.ContainsKey("img")) results.AddRange(Selector.FilterTag("img"));
            else if(rules.ContainsKey("aHrefContains")) results.AddRange(Selector.FilterTagAtrValueContains("a", "href", rules["aHrefContains"]));
            return results;
        }
    }
}
