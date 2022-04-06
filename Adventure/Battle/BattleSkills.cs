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
        void Add(ISkill skill);
        void AddRange(IEnumerable<ISkill> skill);
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

        public void Add(ISkill skill)
        {
            var button = new SharpButton<ISkill>() { Text = skill.Name, UserObject = skill };
            this.skills.Add(button);
        }

        public void AddRange(IEnumerable<ISkill> skills)
        {
            foreach(var skill in skills)
            {
                Add(skill);
            }
        }

        public bool UpdateGui(ISharpGui sharpGui, IScopedCoroutine coroutine, ref BattlePlayer.MenuMode menuMode, Action<IBattleTarget, ISkill> skillSelectedCb)
        {
            var didSomething = false;

            var skillCount = skills.Count;
            if (skillCount > 0)
            {
                var previous = skillCount - 1;
                var next = skills.Count > 1 ? 1 : 0;

                battleScreenLayout.LayoutBattleMenu(skills);

                for (var i = 0; i < skillCount; ++i)
                {
                    if (sharpGui.Button(skills[i], navUp: skills[previous].Id, navDown: skills[next].Id))
                    {
                        var skill = skills[i].UserObject;
                        coroutine.RunTask(async () =>
                        {
                            var target = await battleManager.GetTarget(skill.DefaultTargetPlayers);
                            if (target != null)
                            {
                                skillSelectedCb(target, skill);
                            }
                        });
                        menuMode = BattlePlayer.MenuMode.Root;
                        didSomething = true;
                    }

                    previous = i;
                    next = (i + 2) % skillCount;
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
