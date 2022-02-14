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
        string GetLevelName(int level);
    }

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

        public String GetLevelName(int level)
        {
            String adjective;
            if (level < 10)
            {
                adjective = "Busted";
            }
            else if (level < 20)
            {
                adjective = "Rusty";
            }
            else if (level < 30)
            {
                adjective = "Worn";
            }
            else if (level < 40)
            {
                adjective = "Common";
            }
            else if (level < 50)
            {
                adjective = "Deluxe";
            }
            else if (level < 60)
            {
                adjective = "Superior";
            }
            else if (level < 70)
            {
                adjective = "Fine";
            }
            else if (level < 80)
            {
                adjective = "Customized";
            }
            else if (level < 90)
            {
                adjective = "Crafted";
            }
            else
            {
                adjective = "Flawless";
            }

            return adjective;
        }
    }
}
