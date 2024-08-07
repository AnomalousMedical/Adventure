﻿using RpgMath;
using Adventure.Assets;
using System.Collections.Generic;
using System;

namespace Adventure
{
    interface IBiome
    {
        string FloorTexture { get; set; }
        string FloorTexture2 { get; set; }
        string WallTexture { get; set; }
        string WallTexture2 { get; set; }
        bool ReflectFloor { get; }
        bool ReflectWall { get; }
        BiomeTreasure Treasure { get; set; }
        ISpriteAsset RestAsset { get; set; }
        ISpriteAsset GateAsset { get; set; }
        ISpriteAsset KeyAsset { get; set; }
        ISpriteAsset TorchAsset { get; set; }
        string BgMusic { get; set; }
        string BgMusicNight { get; set; }
        string BattleMusic { get; set; }
        string BossBattleMusic { get; set; }
        float MapUnitY { get; set; }
        bool RandomizeMapUnitYDirection { get; set; }

        List<BiomeEnemy> RegularEnemies { get; }

        /// <summary>
        /// Set this to control the boss version of the enemy separately. You will get a boss enemy
        /// stat-wise no matter what.
        /// </summary>
        BiomeEnemy BossEnemy { get; set; }

        List<BiomeBackgroundItem> BackgroundItems { get; set; }

        int MaxBackgroundItemRoll { get; set; }
        ISpriteAsset EntranceAsset { get; set; }
        Func<int, FastNoiseLite> CreateNoise { get; set; }
        float CorridorSlopeMultiple { get; set; }
        Zone.Alignment? OverrideAlignment { get; set; }
    }

    class Biome : IBiome
    {
        public string FloorTexture { get; set; }

        public string FloorTexture2 { get; set; }

        public string WallTexture { get; set; }

        public string WallTexture2 { get; set; }

        public bool ReflectFloor { get; set; }

        public bool ReflectWall { get; set; }

        public float MapUnitY { get; set; } = 0.1f;

        public bool RandomizeMapUnitYDirection { get; set; } = true;

        /// <summary>
        /// Can be 1.0 or -1.0. Will only apply if RandomizeMapUnitYDirection is false.
        /// </summary>
        public float CorridorSlopeMultiple { get; set; } = 1.0f;

        public string BgMusic { get; set; } = "Music/opengameart/Youre Perfect Studio - gone_fishin_by_memoraphile_CC0.ogg";

        public string BgMusicNight { get; set; } = "Music/opengameart/Youre Perfect Studio - gone_fishin_by_memoraphile_CC0.ogg";

        public string BattleMusic { get; set; } = "Music/freepd/Rafael Krux - Hit n Smash.ogg";

        public string BossBattleMusic { get; set; } = "Music/freepd/Bryan Teoh - Honor Bound.ogg";

        public List<BiomeEnemy> RegularEnemies { get; set; } = new List<BiomeEnemy>();

        public BiomeEnemy BossEnemy { get; set; }

        /// <summary>
        /// The treasure to use for the biome.
        /// </summary>
        public BiomeTreasure Treasure { get; set; }

        /// <summary>
        /// The tent asset to use for this biome.
        /// </summary>
        public ISpriteAsset RestAsset { get; set; } = new Assets.World.Tent();

        public ISpriteAsset GateAsset { get; set; } = new Assets.World.MetalGate();

        public ISpriteAsset KeyAsset { get; set; } = new Assets.World.RoundKey();

        public ISpriteAsset TorchAsset { get; set; } = new Assets.World.Torch();

        public ISpriteAsset EntranceAsset { get; set; } = new Assets.World.WorldMapSignpost();

        public List<BiomeBackgroundItem> BackgroundItems { get; set; }

        public int MaxBackgroundItemRoll { get; set; } = 100;

        public Func<int, FastNoiseLite> CreateNoise { get; set; }

        public Zone.Alignment? OverrideAlignment { get; set; }
    }

    record BiomeBackgroundItem(int Chance, ISpriteAsset Asset, float ScaleMin = 1.0f, float ScaleRange = 0.0f, float XPlacementRange = 0.5f, float ZPlacementRange = 0.9f, float WorldScale = 0.35f);
}