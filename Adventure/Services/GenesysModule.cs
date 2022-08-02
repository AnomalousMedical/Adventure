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

        Persistence.GameState SeedWorld(int seed);
    }

    class GenesysModule : IGenesysModule
    {
        public GenesysModule()
        {
            var random = new Random();
            this.Seed = random.Next(int.MaxValue);
        }

        public int Seed { get; set; }

        public Persistence.GameState SeedWorld(int seed)
        {
            var gameState = new Persistence.GameState();
            gameState.World.Seed = seed;
            gameState.World.Level = 1;
            var characterRandom = new Random(seed);

            {
                var sheet = CharacterSheet.CreateStartingFighter(characterRandom);
                sheet.Name = "Bob";
                var hero = new Persistence.CharacterData()
                {
                    PlayerSprite = nameof(Assets.Players.FighterPlayerSprite),
                    CharacterSheet = sheet,
                };
                hero.CharacterSheet.Rest();
                gameState.Party.Members.Add(hero);
            }

            {
                var sheet = CharacterSheet.CreateStartingMage(characterRandom);
                sheet.Name = "Magic Joe";
                var hero = new Persistence.CharacterData()
                {
                    PlayerSprite = nameof(Assets.Players.MagePlayerSprite),
                    CharacterSheet = sheet,
                };
                hero.CharacterSheet.Rest();
                gameState.Party.Members.Add(hero);
            }

            {
                var sheet = CharacterSheet.CreateStartingFighter(characterRandom);
                sheet.Name = "Stabby McStabface";
                var hero = new Persistence.CharacterData()
                {
                    PlayerSprite = nameof(Assets.Players.ThiefPlayerSprite),
                    CharacterSheet = sheet,
                };
                hero.CharacterSheet.Rest();
                gameState.Party.Members.Add(hero);
            }

            {
                var sheet = CharacterSheet.CreateStartingMage(characterRandom);
                sheet.Name = "Wendy";
                var hero = new Persistence.CharacterData()
                {
                    PlayerSprite = nameof(Assets.Players.ClericPlayerSprite),
                    CharacterSheet = sheet
                };
                hero.CharacterSheet.Rest();
                gameState.Party.Members.Add(hero);
            }

            return gameState;
        }
    }
}
