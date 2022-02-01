﻿using RpgMath;
using Adventure.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure
{
    class Biome : IBiome
    {
        public string FloorTexture { get; set; }

        public string WallTexture { get; set; }

        public bool ReflectFloor { get; set; }

        public bool ReflectWall { get; set; }

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
                    Asset = new Assets.Original.TinyDino(),
                    EnemyCurve = new StandardEnemyCurve()
                },
                PeonEnemy = new BiomeEnemy()
                {
                    Asset =  new Assets.Original.TinyDino(),
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
