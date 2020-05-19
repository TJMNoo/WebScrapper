using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebScraper.Data.Engine;

namespace WebScraper.Data.Plugins
{
    public class Optimizer
    {
        private ScraperEngine Engine { get; set; } = new ScraperEngine();

        public List<int> TempResults { get; set; } = new List<int>();
        public async Task<string> Optimize(string fakeArgs)
        {
            System.Diagnostics.Debug.Print("Optimizer:\n");
            var responses = Engine.GetDocsFromRoot("https://crawler-test.com", 20, 0);

            int i = 1;
            await foreach (var response in responses)
            {
                System.Diagnostics.Debug.Print("\nOptimizer result  " + i++);
                //System.Diagnostics.Debug.Print(response.Doc.Text);
                TempResults.Add(response.Doc.Text.Length);
            }

            return "done";
        }
    }
}
