using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebScraper.Data.Engine;

namespace WebScraper.Data.FakePlugins
{
    public class FakeSeoAnalyzer
    {
        private ScraperEngine Engine { get; set; } = new ScraperEngine();
        public List<string> TempResults { get; set; } = new List<string>();

        public async Task<string> Analyze(string fakeArgs)
        {
            System.Diagnostics.Debug.Print("Analyzer:\n");
            var responses = Engine.GetHrefsFromRoot("https://crawler-test.com", 100, 0);
            
            int i = 1;
            await foreach (var response in responses)
            {
                System.Diagnostics.Debug.Print("\n\nAnalyzer result " + i++);
                //System.Diagnostics.Debug.Print(response.Doc.Text + "\n\n");
                TempResults.Add(response.Url);
            }

            return "done";
        }
    }
}
