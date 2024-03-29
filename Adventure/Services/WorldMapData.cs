﻿using Adventure.Services;
using Engine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Services
{
    class WorldMapData
    {
        public csIslandMaze Map { get; private set; }

        public WorldMapData(int seed)
        {
            var random = new FIRandom(seed);
            var mapBuilder = new csIslandMaze(random)
            {
                MapX = 149,
                MapY = 149
            };
            mapBuilder.Iterations = 85000;

            mapBuilder.go();
            mapBuilder.makeEdgesEmpty();
            mapBuilder.findIslands();

            Map = mapBuilder;
        }
    }
}
