using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Items
{
    interface INameGenerator
    {
        string Generate(Random random, List<string> section1, string separator1 = " ", List<string> section2 = null, string separator2 = " ", List<string> section3 = null, string separator3 = " ", List<string> section4 = null, string separator4 = " ", List<string> section5 = null);
        NameResult GetLevelName(int level);
        NameResult GetBookLevelName(int level);
    }

    record NameResult(String Adjective, int Level, int Cost);

    class NameGenerator : INameGenerator
    {
        /// <summary>
        /// Generate a name from given lists of strings. This will always call next on the passed random 5 times
        /// no matter how many sections are passed. Each section gets a space in between.
        /// </summary>
        public String Generate(Random random, List<String> section1, string separator1 = " ", List<String> section2 = null, string separator2 = " ", List<String> section3 = null, string separator3 = " ", List<String> section4 = null, string separator4 = " ", List<String> section5 = null)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(section1[random.Next(section1.Count)]);
            AddSection(random, separator1, section2, stringBuilder);
            AddSection(random, separator2, section3, stringBuilder);
            AddSection(random, separator3, section4, stringBuilder);
            AddSection(random, separator4, section5, stringBuilder);

            return stringBuilder.ToString();
        }

        private static void AddSection(Random random, String separator, List<string> section, StringBuilder stringBuilder)
        {
            if (section != null)
            {
                stringBuilder.Append(separator);
                stringBuilder.Append(section[random.Next(section.Count)]);
            }
            else
            {
                random.Next();
            }
        }

        public NameResult GetLevelName(int level)
        {
            if (level < SpellLevels.Busted)
            {
                return new NameResult("Busted", 1, 100);
            }
            else if (level < SpellLevels.Common)
            {
                return new NameResult("Common", 30, 175);
            }
            else if (level < SpellLevels.Superior)
            {
                return new NameResult("Superior", 50, 225);
            }
            else
            {
                return new NameResult("Flawless", 90, 325);
            }
        }

        public NameResult GetBookLevelName(int level)
        {
            if (level < SpellLevels.LittleBookOf)
            {
                return new NameResult("Little Book of", 1, 100);
            }
            else if (level < SpellLevels.BookOf)
            {
                return new NameResult("Book of", 40, 125);
            }
            else 
            {
                return new NameResult("Big Book of", 60, 325);
            }
        }
    }
}
