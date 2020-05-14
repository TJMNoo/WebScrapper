using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebScraper.Data.FakePlugins
{
    public class FakeGoogleKeywordTracker
    {
        public FakeGoogleKeywordTracker() { }

        public async Task<string> Track()
        {
            TraverseService a = new TraverseService();

            //googlePages ce zapravo biti lista HtmlDocuments, ne lista izfiltriranih website titles kao sad
            //ovaj plugin ce preuzeti posao filtriranja
            var googlePages = a.ScrapeGoogleResults("kek", "us", 2);
            int i = 1;
            await foreach (var page in googlePages)
            {
                System.Diagnostics.Debug.Print("\nGoogle Page " + i++);
                foreach (var result in page)
                {
                    System.Diagnostics.Debug.Print(result);
                }
            }
            //save na back etc

            return "done";
        }
    }
}
