﻿using Engine.Platform;
using System.Collections.Generic;

namespace SceneTest.GameOver
{
    interface IGameOverGameState : IGameState
    {
        void Link(IGameState explorationState);
    }
}