using Engine;
using Engine.Platform;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Battle
{
    interface IBattleSkills
    {
        void AddSpell(ISkill skill);
        void AddSpells(IEnumerable<ISkill> skill);
        bool UpdateGui(ISharpGui sharpGui, IScopedCoroutine coroutine, ref BattlePlayer.MenuMode menuMode, Action<IBattleTarget, ISkill> skillSelectedCb);
    }

    class BattleSkills : IBattleSkills
    {
        private readonly IBattleScreenLayout battleScreenLayout;
        private readonly IBattleManager battleManager;


        private List<SharpButton<ISkill>> skills = new List<SharpButton<ISkill>>();

        public BattleSkills(IBattleScreenLayout battleScreenLayout, IBattleManager battleManager)
        {
            this.battleScreenLayout = battleScreenLayout;
            this.battleManager = battleManager;
        }

        public void AddSpell(ISkill skill)
        {
            var button = new SharpButton<ISkill>() { Text = skill.Name, UserObject = skill };
            this.skills.Add(button);
        }

        public void AddSpells(IEnumerable<ISkill> skills)
        {
            foreach(var spell in skills)
            {
                AddSpell(spell);
            }
        }

        public bool UpdateGui(ISharpGui sharpGui, IScopedCoroutine coroutine, ref BattlePlayer.MenuMode menuMode, Action<IBattleTarget, ISkill> skillSelectedCb)
        {
            var didSomething = false;

            var spellCount = skills.Count;
            if (spellCount > 0)
            {
                var previous = spellCount - 1;
                var next = skills.Count > 1 ? 1 : 0;

                battleScreenLayout.LayoutBattleMenu(skills);

                for (var i = 0; i < spellCount; ++i)
                {
                    if (sharpGui.Button(skills[i], navUp: skills[previous].Id, navDown: skills[next].Id))
                    {
                        var spell = skills[i].UserObject;
                        coroutine.RunTask(async () =>
                        {
                            var target = await battleManager.GetTarget(spell.DefaultTargetPlayers);
                            if (target != null)
                            {
                                skillSelectedCb(target, spell);
                            }
                        });
                        menuMode = BattlePlayer.MenuMode.Root;
                        didSomething = true;
                    }

                    previous = i;
                    next = (i + 2) % spellCount;
                }
            }

            if (!didSomething && sharpGui.IsStandardBackPressed())
            {
                menuMode = BattlePlayer.MenuMode.Root;
            }

            return didSomething;
        }
    }
}
