using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace RpgMath.Tests
{
    public class ArchetypeTests
    {
        private readonly ITestOutputHelper output;

        public ArchetypeTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData(99, 0)]
        [InlineData(0, 99)]
        [InlineData(50, 50)]
        [InlineData(30, 70)]
        [InlineData(70, 30)]
        public void LevelUpStartFighter(int fighterLevels, int magicLevels)
        {
            var levelCalculator = new LevelCalculator();
            var random = new Random(0);
            var character = CharacterSheet.CreateStartingFighter(random);

            for (int i = 0; i < fighterLevels; i++)
            {
                character.LevelUpFighter(levelCalculator);
            }

            for(var i = 0; i < magicLevels; i++)
            {
                character.LevelUpMage(levelCalculator);
            }

            output.WriteLine($"Level {character.Level}");

            output.WriteLine($"Hp {character.Hp}");
            output.WriteLine($"Mp {character.Mp}");

            output.WriteLine($"Attack {character.Attack}");
            output.WriteLine($"Magic Attack {character.MagicAttack}");

            output.WriteLine($"Defense {character.Defense}");
            output.WriteLine($"Magic Defense {character.MagicDefense}");

            output.WriteLine($"Dexterity {character.Dexterity}");
            output.WriteLine($"Luck {character.Luck}");
        }

        [Theory]
        [InlineData(99, 0)]
        [InlineData(0, 99)]
        [InlineData(50, 50)]
        [InlineData(30, 70)]
        [InlineData(70, 30)]
        public void LevelUpStartMage(int fighterLevels, int magicLevels)
        {
            var levelCalculator = new LevelCalculator();
            var random = new Random(0);
            var character = CharacterSheet.CreateStartingMage(random);

            for (int i = 0; i < fighterLevels; i++)
            {
                character.LevelUpFighter(levelCalculator);
            }

            for (var i = 0; i < magicLevels; i++)
            {
                character.LevelUpMage(levelCalculator);
            }

            output.WriteLine($"Level {character.Level}");

            output.WriteLine($"Hp {character.Hp}");
            output.WriteLine($"Mp {character.Mp}");

            output.WriteLine($"Attack {character.Attack}");
            output.WriteLine($"Magic Attack {character.MagicAttack}");

            output.WriteLine($"Defense {character.Defense}");
            output.WriteLine($"Magic Defense {character.MagicDefense}");

            output.WriteLine($"Dexterity {character.Dexterity}");
            output.WriteLine($"Luck {character.Luck}");
        }
    }
}
