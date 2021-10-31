﻿using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SceneTest.Battle
{
    interface ISpell
    {
        void Apply(IBattleManager battleManager, IObjectResolver objectResolver, IBattleTarget attacker, IBattleTarget target);

        bool DefaultTargetPlayers => false;
    }
}
