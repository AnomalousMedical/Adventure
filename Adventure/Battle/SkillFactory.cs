using Adventure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Battle
{
    interface ISkillFactory
    {
        ISkill CreateSkill(string name);
    }

    class SkillFactory : ISkillFactory
    {
        private readonly ISimpleActivator simpleActivator;

        public SkillFactory(ISimpleActivator simpleActivator)
        {
            this.simpleActivator = simpleActivator;
        }

        public ISkill CreateSkill(String name)
        {
            return simpleActivator.CreateInstance<ISkill>($"Adventure.Battle.Skills.{name}");
        }
    }
}
