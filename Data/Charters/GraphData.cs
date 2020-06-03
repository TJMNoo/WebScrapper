using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebScraper.Data
{
    public class GraphData
    {
        public List<int> Positions { get; set; }
        public List<DateTime> Dates { get; set; } = new List<DateTime>();

        public GraphData(List<int> positions, List<string> dates)
        {
            Positions = positions;
            foreach (var date in dates) Dates.Add(DateTime.Parse(date));
        }
    }
}
