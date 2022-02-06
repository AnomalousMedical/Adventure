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
                    Asset = new Assets.Original.TinyDino(),
                    EnemyCurve = new StandardEnemyCurve()
                },
                BadassEnemy = new BiomeEnemy()
                {
                    Asset = new Assets.Original.TinyDino()
                    {
                        PalletSwap = new Dictionary<uint, uint>
                        {
                            { Assets.Original.TinyDino.Skin, 0xff166416 }
                        }
                    },
                    EnemyCurve = new StandardEnemyCurve()
                },
                PeonEnemy = new BiomeEnemy()
                {
                    Asset =  new Assets.Original.TinyDino()
                    {
                        PalletSwap = new Dictionary<uint, uint>
                        {
                            { Assets.Original.TinyDino.Skin, 0xff168543 }
                        }
                    },
                    EnemyCurve = new StandardEnemyCurve()
                },
                Treasure = new BiomeTreasure()
                {
                    Asset = new Assets.Original.TreasureChest(),
                }
            },
            //Desert
            new Biome
            {
                FloorTexture = "Graphics/Textures/AmbientCG/Ground025_1K",
                WallTexture = "Graphics/Textures/AmbientCG/Rock029_1K",
                RegularEnemy = new BiomeEnemy()
                {
                    Asset = new Assets.Original.Skeleton(),
                    EnemyCurve = new StandardEnemyCurve(),
                    Resistances = new Dictionary<Element, Resistance>
                    {
                        { Element.Healing, Resistance.Absorb },
                        { Element.Fire, Resistance.Weak }
                    }
                },
                BadassEnemy = new BiomeEnemy()
                {
                    Asset = new Assets.Original.Skeleton()
                    {
                        PalletSwap = new Dictionary<uint, uint>
                        {
                            { Assets.Original.Skeleton.Bone, 0xff404040 }
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
                    Asset = new Assets.Original.Skeleton()
                    {
                        PalletSwap = new Dictionary<uint, uint>
                        {
                            { Assets.Original.Skeleton.Bone, 0xffd1cbb6 }
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
                    Asset = new Assets.Original.TreasureChest(),
                }
            },
            //Snowy
            new Biome
            {
                FloorTexture = "Graphics/Textures/AmbientCG/Snow006_1K",
                WallTexture = "Graphics/Textures/AmbientCG/Rock022_1K",
                ReflectFloor = false,
                RegularEnemy = new BiomeEnemy()
                {
                    Asset = new Assets.Original.TinyDino()
                    {
                        PalletSwap = new Dictionary<uint, uint>
                        { 
                            { Assets.Original.TinyDino.Skin, 0xff317a89 }
                        }
                    },
                    EnemyCurve = new StandardEnemyCurve()
                },
                BadassEnemy = new BiomeEnemy()
                {
                    Asset = new Assets.Original.TinyDino()
                    {
                        PalletSwap = new Dictionary<uint, uint>
                        {
                            { Assets.Original.TinyDino.Skin, 0xff024f59 }
                        }
                    },
                    EnemyCurve = new StandardEnemyCurve()
                },
                PeonEnemy = new BiomeEnemy()
                {
                    Asset =  new Assets.Original.TinyDino()
                    {
                        PalletSwap = new Dictionary<uint, uint>
                        {
                            { Assets.Original.TinyDino.Skin, 0xff7babaf }
                        }
                    },
                    EnemyCurve = new StandardEnemyCurve()
                },
                Treasure = new BiomeTreasure()
                {
                    Asset = new Assets.Original.TreasureChest(),
                }
            }
        };

        public IBiome GetBiome(int index)
        {
            return biomes[index];
        }

        public int Count => biomes.Count;
    }
}
