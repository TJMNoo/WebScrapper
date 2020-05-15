using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace WebScraper.Data
{
    public class SelectorService
    {
        private HtmlAgilityPack.HtmlDocument _doc;

        public SelectorService(HtmlAgilityPack.HtmlDocument doc)
        {
            _doc = doc;
        }

        public List<string> FilterTag(string tag)
        {
            if (String.IsNullOrEmpty(tag)) return new List<string>();

            List<string> tags = new List<string>();
            var nodes = _doc.DocumentNode.Descendants(tag).ToArray();

            if (nodes == null) return new List<string>();

            var nodeArray = nodes.ToArray();
            foreach (var node in nodeArray) tags.Add(node.OuterHtml);
            return tags;
        }

        public List<string> FilterTagAtrValuePairs(string tag)
        {
            if (String.IsNullOrEmpty(tag)) return new List<string>();

            List<string> attrValuePairs = new List<string>();
            var nodes = _doc.DocumentNode.SelectNodes("//" + tag);
            if (nodes == null) return new List<string>();

            var nodeArray = nodes.ToArray();
            foreach (var node in nodeArray)
            {
                var attributes = node.Attributes;
                string pairs = "";
                for (int i = 0; i < attributes.Count; i++)
                {
                    if (i + 1 != attributes.Count) pairs += attributes[i].Name + ":" + attributes[i].Value + " , ";
                    else pairs += attributes[i].Name + ":" + attributes[i].Value;
                }

                attrValuePairs.Add(pairs);
            }

            return attrValuePairs;
        }

        public List<string> FilterTagAtrValueContains(string tag, string atr, string value)
        {
            if (String.IsNullOrEmpty(tag)) return new List<string>();
            if (String.IsNullOrEmpty(atr)) atr = "";
            if (String.IsNullOrEmpty(value)) value = "";

            List<string> values = new List<string>();
            var nodes = _doc.DocumentNode.SelectNodes("//" + tag);
            if (nodes == null) return new List<string>();

            var nodesArray = nodes.ToArray();
            foreach (var node in nodesArray)
            {
                if (atr == "")
                {
                    foreach (var attribute in node.Attributes)
                        if (attribute.Value.Contains(value))
                            values.Add(attribute.Value);
                }
                else
                {
                    var targetedAttribute = node.Attributes[atr];
                    if (targetedAttribute != null && targetedAttribute.Value.Contains(value))
                        values.Add(targetedAttribute.Value);
                }
            }

            return values;
        }

        public List<string> FilterTagAtr(string tag, string atr)
        {
            if (String.IsNullOrEmpty(tag) || String.IsNullOrEmpty(atr)) return new List<string>();

            List<string> values = new List<string>();
            var nodes = _doc.DocumentNode.SelectNodes("//" + tag);
            if (nodes == null) return new List<string>();

            var nodesArray = nodes.ToArray();
            foreach (var node in nodesArray)
            {
                var targetedAttribute = node.Attributes[atr];
                if (targetedAttribute != null) values.Add(targetedAttribute.Value);
            }

            return values;
        }
    }
}