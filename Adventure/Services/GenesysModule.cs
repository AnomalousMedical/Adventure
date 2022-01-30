using Adventure.Battle.Spells;
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
        int Seed { get; set; }

        Persistence SeedWorld(int seed);
    }

    class GenesysModule : IGenesysModule
    {
        public GenesysModule()
        {
            var random = new Random();
            this.Seed = random.Next(int.MaxValue);
        }

        public int Seed { get; set; }

        public Persistence SeedWorld(int seed)
        {
            var persistence = new Persistence();
            persistence.World.Seed = seed;

            {
                var hero = new Persistence.CharacterData()
                {
                    PlayerSprite = nameof(Assets.Original.FighterPlayerSprite),
                    CharacterSheet = new CharacterSheet()
                    {
                        Name = "Bob",
                        Level = 1,
                        MainHand = new Equipment()
                        {
                            AttackPercent = 100,
                            Attack = 18
                        }
                    },
                    PrimaryHandAsset = nameof(Assets.Original.Greatsword01),
                    SecondaryHandAsset = nameof(Assets.Original.ShieldOfReflection)
                };
                hero.CharacterSheet.Rest();
                persistence.Party.Members.Add(hero);
            }

            {
                var hero = new Persistence.CharacterData()
                {
                    PlayerSprite = nameof(Assets.Original.MagePlayerSprite),
                    CharacterSheet = new CharacterSheet()
                    {
                        Name = "Magic Joe",
                        Level = 1,
                        MainHand = new Equipment()
                        {
                            AttackPercent = 35,
                            MagicAttackPercent = 100,
                            Attack = 9
                        }
                    },
                    PrimaryHandAsset = nameof(Assets.Original.Staff07),
                    Spells = new string[] { nameof(Fir), nameof(Fyre), nameof(Meltdown) }
                };
                hero.CharacterSheet.Rest();
                persistence.Party.Members.Add(hero);
            }

            {
                var hero = new Persistence.CharacterData()
                {
                    PlayerSprite = nameof(Assets.Original.ThiefPlayerSprite),
                    CharacterSheet = new CharacterSheet()
                    {
                        Name = "Stabby McStabface",
                        Level = 1,
                        MainHand = new Equipment()
                        {
                            AttackPercent = 100,
                            Attack = 18
                        }
                    },
                    PrimaryHandAsset = nameof(Assets.Original.DaggerNew),
                    SecondaryHandAsset = nameof(Assets.Original.DaggerNew)
                };
                hero.CharacterSheet.Rest();
                persistence.Party.Members.Add(hero);
            }

            {
                var hero = new Persistence.CharacterData()
                {
                    PlayerSprite = nameof(Assets.Original.ClericPlayerSprite),
                    CharacterSheet = new CharacterSheet()
                    {
                        Name = "Wendy",
                        Level = 1,
                        MainHand = new Equipment()
                        {
                            AttackPercent = 100,
                            Attack = 25
                        }
                    },
                    PrimaryHandAsset = nameof(Assets.Original.BattleAxe6),
                    Spells = new String[] { nameof(Cure) }
                };
                hero.CharacterSheet.Rest();
                persistence.Party.Members.Add(hero);
            }

            return persistence;
        }
    }
}
