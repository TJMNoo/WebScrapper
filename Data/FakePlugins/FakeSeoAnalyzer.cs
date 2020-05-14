using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebScraper.Data.FakePlugins
{
    public class FakeSeoAnalyzer
    {
        public FakeSeoAnalyzer() { }

        public async Task<string> Analyze(string fakeArgs)
        {
            TraverseService a = new TraverseService();

            System.Diagnostics.Debug.Print("Analyzer:\n");
            var pages = a.ScrapeFromRoot("https://crawler-test.com", 20, 1.2);
            await foreach (var page in pages)
            {
                System.Diagnostics.Debug.Print(page);
            }

            return "done";
        }
    }
}
