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

        /// <summary>
        /// Remove a possibility from the current round of possibilties. This item will come up again once all options have been used.
        /// </summary>
        /// <param name="remove"></param>
        public void RemoveRoundPossibility(T remove)
        {
            EnsureDistributions();

            distributions.Remove(remove);
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
