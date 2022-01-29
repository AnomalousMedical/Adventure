﻿using RpgMath;
using Adventure.Assets;
using Adventure.Battle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure
{
    class Character
    {
        public CharacterSheet CharacterSheet { get; set; }

        public IPlayerSprite PlayerSprite { get; set; }

        public ISpriteAsset PrimaryHandAsset { get; set; }

        public ISpriteAsset SecondaryHandAsset { get; set; }

        public IEnumerable<ISpell> Spells { get; set; }
    }

    static class CharacterExtensions
    {
        public static int GetAverageLevel(this IEnumerable<CharacterSheet> characters)
        {
            var level = (int)characters.Average(i => i.Level);
            if (level < 1)
            {
                level = 1;
            }
            else if (level > CharacterSheet.MaxLevel)
            {
                level = CharacterSheet.MaxLevel;
            }
            return level;
        }
    }
}