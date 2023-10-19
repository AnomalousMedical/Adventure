using Engine;
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
        Persistence.GameState SeedWorld(int seed);
    }

    class GenesysModule : IGenesysModule
    {
        public GenesysModule()
        {
        }

        public Persistence.GameState SeedWorld(int seed)
        {
            var gameState = new Persistence.GameState();
            gameState.World.Seed = seed;
            gameState.World.Level = 1;
            var characterRandom = new FIRandom(seed);

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

            return gameState;
        }
    }
}
