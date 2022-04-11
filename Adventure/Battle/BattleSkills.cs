using Adventure.Battle.Skills;
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
        public bool UpdateGui(ISharpGui sharpGui, IScopedCoroutine coroutine, ref BattlePlayer.MenuMode menuMode, Action<IBattleTarget, ISkill> skillSelectedCb);
        void Clear();
    }

    class BattleSkills : IBattleSkills
    {
        private static readonly ISkill BackSkill = new Fire(); //This instance of fire is the back button

        private readonly IBattleScreenLayout battleScreenLayout;
        private readonly IBattleManager battleManager;
        private readonly IScaleHelper scaleHelper;
        private ButtonColumn skillButtons = new ButtonColumn(4);
        private List<ISkill> skills = new List<ISkill>();

        public BattleSkills(IBattleScreenLayout battleScreenLayout, IBattleManager battleManager, IScaleHelper scaleHelper)
        {
            this.battleScreenLayout = battleScreenLayout;
            this.battleManager = battleManager;
            this.scaleHelper = scaleHelper;
        }

        public void Add(ISkill skill)
        {
            this.skills.Add(skill);
        }

        public void AddRange(IEnumerable<ISkill> skills)
        {
            this.skills.AddRange(skills);
        }

        public void Clear()
        {
            skills.Clear();
        }

        public bool UpdateGui(ISharpGui sharpGui, IScopedCoroutine coroutine, ref BattlePlayer.MenuMode menuMode, Action<IBattleTarget, ISkill> skillSelectedCb)
        {
            var didSomething = false;

            skillButtons.StealFocus(sharpGui);

            skillButtons.Margin = scaleHelper.Scaled(10);
            skillButtons.MaxWidth = scaleHelper.Scaled(900);
            skillButtons.Bottom = battleScreenLayout.DynamicButtonBottom;
            var skill = skillButtons.Show(sharpGui, skills.Select(i => new ButtonColumnItem<ISkill>(i.Name, i)).Append(new ButtonColumnItem<ISkill>("Back", BackSkill)), skills.Count + 1, s => battleScreenLayout.DynamicButtonLocation(s));
            if (skill != null)
            {
                if (skill == BackSkill)
                {
                    menuMode = BattlePlayer.MenuMode.Root;
                }
                else
                {
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
            }

            if (!didSomething && sharpGui.IsStandardBackPressed())
            {
                menuMode = BattlePlayer.MenuMode.Root;
            }

            return didSomething;
        }
    }
}
