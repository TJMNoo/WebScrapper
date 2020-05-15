using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebScraper.Data.Engine;

namespace WebScraper.Data.FakePlugins
{
    public class FakeGoogleKeywordTracker
    {
        private ScraperEngine Engine { get; set; } = new ScraperEngine();
        public List<int> TempResults { get; set; } = new List<int>();
        public async Task<string> Track(string fakeArgs)
        {
            var responses = Engine.GetGooglePages("rentals new york", "us", 2, 10000);
            int i = 1;
            await foreach (var response in responses)
            {
                System.Diagnostics.Debug.Print("\nGoogle Page " + i++);
                //System.Diagnostics.Debug.Print(response.Doc.Text);
                TempResults.Add(response.Doc.Text.Length);
            }
            //save na back etc

            return "done";
        }
    }
}
