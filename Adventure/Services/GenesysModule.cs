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
            var characterRandom = new Random(seed);

            {
                var sheet = CharacterSheet.CreateStartingFighter(characterRandom);
                sheet.Name = "Bob";
                sheet.MainHand = new Equipment()
                {
                    Name = "Busted Sword",
                    AttackPercent = 100,
                    Attack = 18,
                    Sprite = nameof(Assets.Equipment.Greatsword01)
                };
                var hero = new Persistence.CharacterData()
                {
                    PlayerSprite = nameof(Assets.Players.FighterPlayerSprite),
                    CharacterSheet = sheet,
                };
                hero.CharacterSheet.Rest();
                persistence.Party.Members.Add(hero);
            }

            {
                var sheet = CharacterSheet.CreateStartingMage(characterRandom);
                sheet.Name = "Magic Joe";
                sheet.MainHand = new Equipment()
                {
                    Name = "Traveler's Staff",
                    AttackPercent = 35,
                    MagicAttackPercent = 100,
                    Attack = 9,
                    MagicAttack = 20,
                    TwoHanded = true,
                    Sprite = nameof(Assets.Equipment.Staff07),
                    Spells = new string[] { nameof(Fir) }
                };
                var hero = new Persistence.CharacterData()
                {
                    PlayerSprite = nameof(Assets.Players.MagePlayerSprite),
                    CharacterSheet = sheet,
                };
                hero.CharacterSheet.Rest();
                persistence.Party.Members.Add(hero);
            }

            {
                var sheet = CharacterSheet.CreateStartingFighter(characterRandom);
                sheet.Name = "Stabby McStabface";
                sheet.MainHand = new Equipment()
                {
                    Name = "Niles",
                    AttackPercent = 100,
                    Attack = 16,
                    Sprite = nameof(Assets.Equipment.DaggerNew),
                };
                sheet.OffHand = new Equipment()
                {
                    Name = "Cutty",
                    AttackPercent = 100,
                    Attack = 16,
                    Sprite = nameof(Assets.Equipment.DaggerNew),
                };
                var hero = new Persistence.CharacterData()
                {
                    PlayerSprite = nameof(Assets.Players.ThiefPlayerSprite),
                    CharacterSheet = sheet,
                };
                hero.CharacterSheet.Rest();
                persistence.Party.Members.Add(hero);
            }

            {
                var sheet = CharacterSheet.CreateStartingMage(characterRandom);
                sheet.Name = "Wendy";
                sheet.MainHand = new Equipment()
                {
                    Name = "Woodcutter's Axe",
                    AttackPercent = 100,
                    Attack = 18,
                    MagicAttack = 7,
                    Sprite = nameof(Assets.Equipment.BattleAxe6),
                    Spells = new String[] { nameof(Cure) }
                };
                var hero = new Persistence.CharacterData()
                {
                    PlayerSprite = nameof(Assets.Players.ClericPlayerSprite),
                    CharacterSheet = sheet
                };
                hero.CharacterSheet.Rest();
                persistence.Party.Members.Add(hero);
            }

            return persistence;
        }
    }
}
