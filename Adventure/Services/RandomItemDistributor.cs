using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Services
{
    class RandomItemDistributor<T>
    {
        private readonly IEnumerable<T> source;
        private List<T> items = new List<T>();

        public RandomItemDistributor(IEnumerable<T> source)
        {
            this.source = source;
        }

        public T GetItem(FIRandom random)
        {
            if (items.Count == 0)
            {
                items.AddRange(source);
            }

            var num = random.Next(items.Count);
            var item = items[num];
            items.RemoveAt(num);
            return item;
        }
    }
}
