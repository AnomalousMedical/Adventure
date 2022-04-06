using Engine;
using RpgMath;
using Adventure.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Platform;
using SharpGui;

namespace Adventure.Battle.Spells
{
    class Steal : ISpell
    {
        public ISkillEffect Apply(IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target)
        {            
            return objectResolver.Resolve<StealEffect>();
        }

        public string Name => "Steal";

        public long MpCost => 0;
    }

    class StealEffect : ISkillEffect
    {
        private readonly ISharpGui sharpGui;
        private readonly IBattleManager battleManager;
        private readonly IBattleScreenLayout battleScreenLayout;

        private SharpButton button = new SharpButton() { Text = "Grab" };

        public StealEffect(ISharpGui sharpGui, IBattleManager battleManager, IBattleScreenLayout battleScreenLayout)
        {
            this.sharpGui = sharpGui;
            this.battleManager = battleManager;
            this.battleScreenLayout = battleScreenLayout;
            battleManager.AllowActivePlayerGui = false;
        }

        public bool Finished { get; private set; }

        public void Update(Clock clock)
        {
            battleScreenLayout.LayoutBattleMenu(button);

            if (sharpGui.Button(button))
            {
                battleManager.AllowActivePlayerGui = true;
                Finished = true;
            }
        }
    }
}
