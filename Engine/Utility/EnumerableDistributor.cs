using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    public class EnumerableDistributor<T>
    {
        private readonly IEnumerable<T> source;
        private List<T> distributions;

        public EnumerableDistributor(IEnumerable<T> source)
        {
            this.source = source;
        }

        public T GetNext(FIRandom random)
        {
            EnsureDistributions();

            var index = random.Next(distributions.Count);
            var value = distributions[index];
            distributions.RemoveAt(index);
            return value;
        }

        private void EnsureDistributions()
        {
            if (distributions == null || distributions.Count == 0)
            {
                distributions = source.ToList();
            }
        }
    }
}
