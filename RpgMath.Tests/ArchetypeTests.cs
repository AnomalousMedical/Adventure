﻿using System;
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
            var random = new Random(0);
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

            output.WriteLine($"Dexterity {character.Dexterity}");
            output.WriteLine($"Luck {character.Luck}");
        }

        [Fact]
        public void LevelUpStartMage()
        {
            var levelCalculator = new LevelCalculator();
            var random = new Random(0);
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

            output.WriteLine($"Dexterity {character.Dexterity}");
            output.WriteLine($"Luck {character.Luck}");
        }
    }
}
