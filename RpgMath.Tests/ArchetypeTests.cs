using Engine;
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

        [Fact]
        public void LevelUpStartFighter()
        {
            var levelCalculator = new LevelCalculator();
            var random = new FIRandom(0);
            var character = CharacterSheet.CreateStartingFighter(random);

            for (int i = 0; i < 100; i++)
            {
                character.LevelUp(levelCalculator);
            }

            output.WriteLine($"Level {character.Level}");

            output.WriteLine($"Hp {character.Hp}");
            output.WriteLine($"Mp {character.Mp}");

            output.WriteLine($"Attack {character.Attack}");
            output.WriteLine($"Magic Attack {character.MagicAttack}");

            output.WriteLine($"Defense {character.Defense}");
            output.WriteLine($"Magic Defense {character.MagicDefense}");

            output.WriteLine($"Dexterity {character.TotalDexterity}");
            output.WriteLine($"Luck {character.TotalLuck}");
        }

        [Fact]
        public void LevelUpStartMage()
        {
            var levelCalculator = new LevelCalculator();
            var random = new FIRandom(0);
            var character = CharacterSheet.CreateStartingMage(random);

            for (int i = 0; i < 100; i++)
            {
                character.LevelUp(levelCalculator);
            }

            output.WriteLine($"Level {character.Level}");

            output.WriteLine($"Hp {character.Hp}");
            output.WriteLine($"Mp {character.Mp}");

            output.WriteLine($"Attack {character.Attack}");
            output.WriteLine($"Magic Attack {character.MagicAttack}");

            output.WriteLine($"Defense {character.Defense}");
            output.WriteLine($"Magic Defense {character.MagicDefense}");

            output.WriteLine($"Dexterity {character.TotalDexterity}");
            output.WriteLine($"Luck {character.TotalLuck}");
        }

        [Fact]
        public void LevelUpStartThief()
        {
            var levelCalculator = new LevelCalculator();
            var random = new FIRandom(0);
            var character = CharacterSheet.CreateStartingThief(random);

            for (int i = 0; i < 100; i++)
            {
                character.LevelUp(levelCalculator);
            }

            output.WriteLine($"Level {character.Level}");

            output.WriteLine($"Hp {character.Hp}");
            output.WriteLine($"Mp {character.Mp}");

            output.WriteLine($"Attack {character.Attack}");
            output.WriteLine($"Magic Attack {character.MagicAttack}");

            output.WriteLine($"Defense {character.Defense}");
            output.WriteLine($"Magic Defense {character.MagicDefense}");

            output.WriteLine($"Dexterity {character.TotalDexterity}");
            output.WriteLine($"Luck {character.TotalLuck}");
        }

        [Fact]
        public void LevelUpStartSage()
        {
            var levelCalculator = new LevelCalculator();
            var random = new FIRandom(0);
            var character = CharacterSheet.CreateStartingSage(random);

            for (int i = 0; i < 100; i++)
            {
                character.LevelUp(levelCalculator);
            }

            output.WriteLine($"Level {character.Level}");

            output.WriteLine($"Hp {character.Hp}");
            output.WriteLine($"Mp {character.Mp}");

            output.WriteLine($"Attack {character.Attack}");
            output.WriteLine($"Magic Attack {character.MagicAttack}");

            output.WriteLine($"Defense {character.Defense}");
            output.WriteLine($"Magic Defense {character.MagicDefense}");

            output.WriteLine($"Dexterity {character.TotalDexterity}");
            output.WriteLine($"Luck {character.TotalLuck}");
        }
    }
}
