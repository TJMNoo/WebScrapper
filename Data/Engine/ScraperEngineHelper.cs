using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace WebScraper.Data.Engine
{
    public class ScraperEngineHelper
    {
        public string BaseDomain { get; set; }
        public string WebsiteName { get; set; }
        private Dictionary<Regex, bool> RobotsDisallowedUrls { get; set; } = new Dictionary<Regex, bool>();
        private Dictionary<Regex, bool> RobotsAllowedUrls { get; set; } = new Dictionary<Regex, bool>();
        

        //parameter url expects http prefix
        public void SetUrl(string url)
        {
            BaseDomain = "";
            int slashCounter = 0;
            foreach (char c in url)
            {
                if (c == '/') slashCounter++;
                if (slashCounter == 3) break;
                BaseDomain += c;
            }

            slashCounter = 0;
            foreach (char c in BaseDomain)
            {
                if (c == '.') break;
                if (slashCounter == 2) WebsiteName += c;
                if (c == '/') slashCounter++;

            }

            LoadRobotsTxt();
        }

        private void LoadRobotsTxt()
        {
            RobotsDisallowedUrls.Clear();
            RobotsAllowedUrls.Clear();

            WebClient client = new WebClient();
            client.Headers.Set("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:12.0) Gecko/20100101 Firefox/12.0");
            string str = client.DownloadString(BaseDomain + "/robots.txt");
            if (string.IsNullOrEmpty(str)) return;

            string[] commands = str.Split("\n", StringSplitOptions.RemoveEmptyEntries);

            bool foundUserAgent = false;
            foreach (var command in commands)
            {
                // System.Diagnostics.Debug.Print(command);
                string[] parts = command.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                if (parts[0].ToLower() == "user-agent:" && parts[1] == "*") foundUserAgent = true;
                else if (foundUserAgent && parts[0] == "Disallow:")
                {
                    Regex reg = ConvertRobotsCommandToRegex(parts[1]);
                    RobotsDisallowedUrls[reg] = true;
                }
                else if (foundUserAgent && parts[0] == "Allow:")
                {
                    Regex reg = ConvertRobotsCommandToRegex(parts[1]);
                    RobotsAllowedUrls[reg] = true;
                }
                else if (foundUserAgent && parts[0].ToLower() == "user-agent:") break;
            }
        }

        private Regex ConvertRobotsCommandToRegex(string robotsCommand)
        {
            string regexStr = "";
            for (int i = 0; i < robotsCommand.Length; i++)
            {
                if (robotsCommand[i] == '?' || robotsCommand[i] == '/' || robotsCommand[i] == '.') regexStr += "\\" + robotsCommand[i];
                else if (robotsCommand[i] == '*') regexStr += "[a-zA-Z0-9]" + robotsCommand[i];
                else regexStr += robotsCommand[i];
            }
            Regex reg = new Regex(@regexStr, RegexOptions.Compiled);
            return reg;
        }

        private bool DisallowedRobotsTxt(string href)
        {
            foreach (var entry in RobotsDisallowedUrls)
                if (entry.Key.IsMatch(href)) return true;
            return false;
        }

        private bool AllowedRobotsTxt(string href)
        {
            foreach (var entry in RobotsAllowedUrls)
                if (entry.Key.IsMatch(href)) return true;
            return false;
        }

        public string FormatHref(string href)
        {
            if (string.IsNullOrEmpty(href) || href[0] == '#' || href.Contains(" ")) return "invalid";
            bool allowed = AllowedRobotsTxt(href);
            if (!allowed && DisallowedRobotsTxt(href)) return "robots.txt disallowed";

            Regex checkIfHttps = new Regex(@"^(http|https):\/\/", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            if (checkIfHttps.IsMatch(href) && href.Contains(WebsiteName)) return href;
            else if (checkIfHttps.IsMatch(href)) return "invalid"; //outside website

            if (href[0] == '.' && href[1] == '/') return BaseDomain + href.Substring(1);
            if (href[0] != '/') return BaseDomain + "/" + href; //relative href
            else return BaseDomain + href;
        }
    }
}
