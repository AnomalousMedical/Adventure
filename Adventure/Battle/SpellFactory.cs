using Adventure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Battle
{
    interface ISpellFactory
    {
        ISpell CreateSpell(string name);
    }

    class SpellFactory : ISpellFactory
    {
        private readonly ISimpleActivator simpleActivator;

        public SpellFactory(ISimpleActivator simpleActivator)
        {
            this.simpleActivator = simpleActivator;
        }

        public ISpell CreateSpell(String name)
        {
            return simpleActivator.CreateInstance<ISpell>($"Adventure.Battle.Spells.{name}");
        }
    }
}
