using RpgMath;
using Adventure.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure
{
    interface IBiomeManager
    {
        int Count { get; }

        IBiome GetBiome(int index);
    }

    class BiomeEnemy
    {
        public ISpriteAsset Asset { get; set; }

        public IEnemyCurve EnemyCurve { get; set; }

        public Dictionary<Element, Resistance> Resistances { get; set; }
    }

    class BiomeTreasure
    {
        public ISpriteAsset Asset { get; set; }
    }

    class BiomeManager : IBiomeManager
    {
        private List<IBiome> biomes = new List<IBiome>()
        {
            //Countryside
            new Biome
            {
                FloorTexture = "Graphics/Textures/AmbientCG/Rocks023_1K",
                WallTexture = "Graphics/Textures/AmbientCG/Ground037_1K",
                RegularEnemy = new BiomeEnemy()
                {
                    Asset = new Assets.Enemies.TinyDino(),
                    EnemyCurve = new StandardEnemyCurve()
                },
                BadassEnemy = new BiomeEnemy()
                {
                    Asset = new Assets.Enemies.TinyDino()
                    {
                        PalletSwap = new Dictionary<uint, uint>
                        {
                            { Assets.Enemies.TinyDino.Skin, 0xff166416 }
                        }
                    },
                    EnemyCurve = new StandardEnemyCurve()
                },
                PeonEnemy = new BiomeEnemy()
                {
                    Asset =  new Assets.Enemies.TinyDino()
                    {
                        PalletSwap = new Dictionary<uint, uint>
                        {
                            { Assets.Enemies.TinyDino.Skin, 0xff168543 }
                        }
                    },
                    EnemyCurve = new StandardEnemyCurve()
                },
                BossEnemy = new BiomeEnemy()
                {
                    Asset =  new Assets.Enemies.TinyDino()
                    {
                        PalletSwap = new Dictionary<uint, uint>
                        {
                            { Assets.Enemies.TinyDino.Skin, 0xffc12935 },
                            { Assets.Enemies.TinyDino.Spine, 0xff9105bd },
                            { Assets.Enemies.TinyDino.Eye, 0xffe28516 },
                        }
                    },
                    EnemyCurve = new StandardEnemyCurve()
                },
                Treasure = new BiomeTreasure()
                {
                    Asset = new Assets.World.TreasureChest(),
                }
            },
            //Desert
            new Biome
            {
                FloorTexture = "Graphics/Textures/AmbientCG/Ground025_1K",
                WallTexture = "Graphics/Textures/AmbientCG/Rock029_1K",
                RegularEnemy = new BiomeEnemy()
                {
                    Asset = new Assets.Enemies.Skeleton(),
                    EnemyCurve = new StandardEnemyCurve(),
                    Resistances = new Dictionary<Element, Resistance>
                    {
                        { Element.Healing, Resistance.Absorb },
                        { Element.Fire, Resistance.Weak },
                        { Element.Piercing, Resistance.Resist },
                        { Element.Bludgeoning, Resistance.Weak }
                    }
                },
                BadassEnemy = new BiomeEnemy()
                {
                    Asset = new Assets.Enemies.Skeleton()
                    {
                        PalletSwap = new Dictionary<uint, uint>
                        {
                            { Assets.Enemies.Skeleton.Bone, 0xff404040 }
                        }
                    },
                    EnemyCurve = new StandardEnemyCurve(),
                    Resistances = new Dictionary<Element, Resistance>
                    {
                        { Element.Healing, Resistance.Absorb },
                        { Element.Fire, Resistance.Weak }
                    }
                },
                PeonEnemy = new BiomeEnemy()
                {
                    Asset = new Assets.Enemies.Skeleton()
                    {
                        PalletSwap = new Dictionary<uint, uint>
                        {
                            { Assets.Enemies.Skeleton.Bone, 0xffd1cbb6 }
                        }
                    },
                    EnemyCurve = new StandardEnemyCurve(),
                    Resistances = new Dictionary<Element, Resistance>
                    {
                        { Element.Healing, Resistance.Absorb },
                        { Element.Fire, Resistance.Weak }
                    }
                },
                Treasure = new BiomeTreasure()
                {
                    Asset = new Assets.World.TreasureChest(),
                },
                BgMusic = "Music/opengameart/Fantasy_Origins - Cavernous_Desert02.ogg",
                BgMusicNight = "Music/opengameart/HorrorPen - Winds Of Stories.ogg"
            },
            //Snowy
            new Biome
            {
                FloorTexture = "Graphics/Textures/AmbientCG/Snow006_1K",
                WallTexture = "Graphics/Textures/AmbientCG/Rock022_1K",
                ReflectFloor = false,
                RegularEnemy = new BiomeEnemy()
                {
                    Asset = new Assets.Enemies.TinyDino()
                    {
                        PalletSwap = new Dictionary<uint, uint>
                        { 
                            { Assets.Enemies.TinyDino.Skin, 0xff317a89 }
                        }
                    },
                    EnemyCurve = new StandardEnemyCurve()
                },
                BadassEnemy = new BiomeEnemy()
                {
                    Asset = new Assets.Enemies.TinyDino()
                    {
                        PalletSwap = new Dictionary<uint, uint>
                        {
                            { Assets.Enemies.TinyDino.Skin, 0xff024f59 }
                        }
                    },
                    EnemyCurve = new StandardEnemyCurve()
                },
                PeonEnemy = new BiomeEnemy()
                {
                    Asset =  new Assets.Enemies.TinyDino()
                    {
                        PalletSwap = new Dictionary<uint, uint>
                        {
                            { Assets.Enemies.TinyDino.Skin, 0xff7babaf }
                        }
                    },
                    EnemyCurve = new StandardEnemyCurve()
                },
                BossEnemy = new BiomeEnemy()
                {
                    Asset =  new Assets.Enemies.TinyDino()
                    {
                        PalletSwap = new Dictionary<uint, uint>
                        {
                            { Assets.Enemies.TinyDino.Skin, 0xff9105bd },
                            { Assets.Enemies.TinyDino.Eye, 0xff2ccdca }
                        }
                    },
                    EnemyCurve = new StandardEnemyCurve()
                },
                Treasure = new BiomeTreasure()
                {
                    Asset = new Assets.World.TreasureChest(),
                },
                BgMusic = "Music/opengameart/congusbongus - Mythica.ogg",
                BgMusicNight = "Music/opengameart/Kistol - Snowfall (Looped ver.).ogg"
            }
        };

        public IBiome GetBiome(int index)
        {
            return biomes[index];
        }

        public int Count => biomes.Count;
    }
}
