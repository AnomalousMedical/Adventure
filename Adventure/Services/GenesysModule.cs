﻿using Adventure.Battle.Spells;
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
                    AttackPercent = 100,
                    Attack = 18,
                    Sprite = nameof(Assets.Original.Greatsword01)
                };
                sheet.OffHand = new Equipment
                {
                    Sprite = nameof(Assets.Original.ShieldOfReflection),
                    Defense = 18,
                };
                var hero = new Persistence.CharacterData()
                {
                    PlayerSprite = nameof(Assets.Original.FighterPlayerSprite),
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
                    AttackPercent = 35,
                    MagicAttackPercent = 100,
                    Attack = 9,
                    MagicAttack = 32,
                    TwoHanded = true,
                    Sprite = nameof(Assets.Original.Staff07),
                };
                var hero = new Persistence.CharacterData()
                {
                    PlayerSprite = nameof(Assets.Original.MagePlayerSprite),
                    CharacterSheet = sheet,
                    Spells = new string[] { nameof(Fir), nameof(Fyre), nameof(Meltdown) }
                };
                hero.CharacterSheet.Rest();
                persistence.Party.Members.Add(hero);
            }

            {
                var sheet = CharacterSheet.CreateStartingFighter(characterRandom);
                sheet.Name = "Stabby McStabface";
                sheet.MainHand = new Equipment()
                {
                    AttackPercent = 100,
                    Attack = 18,
                    Sprite = nameof(Assets.Original.DaggerNew),
                };
                sheet.OffHand = new Equipment()
                {
                    AttackPercent = 100,
                    Attack = 18,
                    Sprite = nameof(Assets.Original.DaggerNew),
                };
                var hero = new Persistence.CharacterData()
                {
                    PlayerSprite = nameof(Assets.Original.ThiefPlayerSprite),
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
                    AttackPercent = 100,
                    Attack = 18,
                    MagicAttack = 18,
                    Sprite = nameof(Assets.Original.BattleAxe6),
                };
                var hero = new Persistence.CharacterData()
                {
                    PlayerSprite = nameof(Assets.Original.ClericPlayerSprite),
                    CharacterSheet = sheet,
                    Spells = new String[] { nameof(Cure) }
                };
                hero.CharacterSheet.Rest();
                persistence.Party.Members.Add(hero);
            }

            return persistence;
        }
    }
}
