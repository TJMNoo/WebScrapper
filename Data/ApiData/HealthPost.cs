using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChartJs.Blazor.ChartJS.Common.Enums;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;

namespace WebScraper.Data.ApiData
{
    public class HealthPost
    {
        public int NumOfAllLinks { get; set; }

        public int NumOfHealthyLinks { get; set; }

        public int NumOfBrokenLinks { get; set; }

        public int NumOfBlockedLinks { get; set; }

        public int NumOfRedirectLinks { get; set; }

        public int NumOfLinksWithIssues { get; set; }

        public int NumOfImgsWithNoAlt { get; set; }

        public int NumOfAllTitles { get; set; }

        public int NumOfHealthyTitles { get; set; }

        public int NumOfEmptyTitles { get; set; }

        public int NumOfLongTitles { get; set; }

        public int NumOfShortTitles { get; set; }

        public int NumOfAllDescriptions { get; set; }

        public int NumOfHealthyDescriptions { get; set; }

        public string Date { get; set; }

        public string Url { get; set; }

        public string Username { get; set; }

        public HealthPost() { }

        public JObject ConvertToJObject()
        {
            JObject data = new JObject();
            data["Date"] = Date;
            data["Username"] = Username;
            data["Url"] = Url;
            data["NumOfAllLinks"] = NumOfAllLinks;
            data["NumOfHealthyLinks"] = NumOfHealthyLinks;
            data["NumOfBrokenLinks"] = NumOfBrokenLinks;
            data["NumOfBlockedLinks"] = NumOfBlockedLinks;
            data["NumOfRedirectLinks"] = NumOfRedirectLinks;
            data["NumOfLinksWithIssues"] = NumOfLinksWithIssues;
            data["NumOfImgsWithNoAlt"] = NumOfImgsWithNoAlt;
            data["NumOfAllTitles"] = NumOfAllTitles;
            data["NumOfHealthyTitles"] = NumOfHealthyTitles;
            data["NumOfEmptyTitles"] = NumOfEmptyTitles;
            data["NumOfLongTitles"] = NumOfLongTitles;
            data["NumOfShortTitles"] = NumOfShortTitles;
            data["NumOfAllDescriptions"] = NumOfAllDescriptions;
            data["NumOfHealthyDescriptions"] = NumOfHealthyDescriptions;

            return data;
        }
    }
}
