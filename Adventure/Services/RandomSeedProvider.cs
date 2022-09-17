using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Services
{
    interface ISeedProvider
    {
        int GetSeed();
    }

    class RandomSeedProvider : ISeedProvider
    {
        private Random random = new Random();

        public int GetSeed()
        {
            return random.Next();
        }
    }

    class ConstantSeedProvider : ISeedProvider
    {
        private readonly int seed;

        public ConstantSeedProvider(int seed)
        {
            this.seed = seed;
        }

        public int GetSeed()
        {
            return seed;
        }
    }
}
