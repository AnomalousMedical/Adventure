using RpgMath;
using Adventure.Assets;

namespace Adventure
{
    interface IBiome
    {
        string FloorTexture { get; set; }
        string WallTexture { get; set; }
        bool ReflectFloor { get; }
        bool ReflectWall { get; }
        BiomeTreasure Treasure { get; set; }
        ISpriteAsset RestAsset { get; set; }
        string BgMusic { get; set; }
        string BattleMusic { get; set; }
        BiomeEnemy GetEnemy(EnemyType type);
    }

    class Biome : IBiome
    {
        public string FloorTexture { get; set; }

        public string WallTexture { get; set; }

        public bool ReflectFloor { get; set; }

        public bool ReflectWall { get; set; }

        public string BgMusic { get; set; } = "Music/freepd/Rafael Krux - Black Knight.ogg";

        public string BattleMusic { get; set; } = "Music/freepd/Rafael Krux - Hit n Smash.ogg";

        public BiomeEnemy GetEnemy(EnemyType type)
        {
            BiomeEnemy biomeEnemy;
            switch (type)
            {
                case EnemyType.Badass:
                    biomeEnemy = BadassEnemy;
                    break;
                case EnemyType.Peon:
                    biomeEnemy = PeonEnemy;
                    break;
                default:
                    biomeEnemy = RegularEnemy;
                    break;
            }

            return biomeEnemy ?? RegularEnemy;
        }

        public BiomeEnemy RegularEnemy { get; set; }

        /// <summary>
        /// Set this to control the badass version of the enemy separately. You will get a badass enemy
        /// stat-wise no matter what.
        /// </summary>
        public BiomeEnemy BadassEnemy { get; set; }

        /// <summary>
        /// Set this to control the peon version of the enemy separately. You will get a peon enemy
        /// stat-wise no matter what.
        /// </summary>
        public BiomeEnemy PeonEnemy { get; set; }

        /// <summary>
        /// The treasure to use for the biome.
        /// </summary>
        public BiomeTreasure Treasure { get; set; }

        /// <summary>
        /// The tent asset to use for this biome.
        /// </summary>
        public ISpriteAsset RestAsset { get; set; } = new Assets.Original.Tent();
    }
}