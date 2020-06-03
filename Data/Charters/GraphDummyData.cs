using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebScraper.Data
{
    public class GraphDummyData
    {
        public List<int> position { get; set; } = new List<int>();
        public List<DateTime> date { get; set; } = new List<DateTime>();

        public GraphDummyData()
        {
            position.Add(23);
            date.Add(new DateTime(2020,5,10));

            position.Add(28);
            date.Add(new DateTime(2020, 5, 11));

            position.Add(15);
            date.Add(new DateTime(2020, 5, 12));

            position.Add(16);
            date.Add(new DateTime(2020, 5, 13));

            position.Add(18);
            date.Add(new DateTime(2020, 5, 14));

            position.Add(10);
            date.Add(new DateTime(2020, 5, 15));

            position.Add(7);
            date.Add(new DateTime(2020, 5, 16));

            position.Add(11);
            date.Add(new DateTime(2020, 5, 17));

            position.Add(13);
            date.Add(new DateTime(2020, 5, 18));
        }

        public void Shuffle()
        {
            Random rng = new Random();
            int n = position.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                var value = position[k];
                position[k] = position[n];
                position[n] = value;
            }
        }
    }
}
