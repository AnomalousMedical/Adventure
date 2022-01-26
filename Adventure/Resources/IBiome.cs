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

        BiomeEnemy GetEnemy(EnemyType type);
    }
}