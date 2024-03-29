﻿using Engine;
using RpgMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Services
{
    interface IGenesysModule
    {
        Persistence.GameState SeedWorld(int seed);
    }

    class GenesysModule : IGenesysModule
    {
        public GenesysModule()
        {
        }

        public Persistence.GameState SeedWorld(int seed)
        {
            var gameState = new Persistence.GameState();
            gameState.World.Seed = seed;
            gameState.World.Level = 17;

            return gameState;
        }
    }
}
