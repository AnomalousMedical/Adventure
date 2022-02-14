using RpgMath;
using Adventure.Assets;
using Adventure.Battle;
using Adventure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adventure
{
    class Party
    {
        private readonly Persistence persistence;

        public Party(Persistence persistence)
        {
            this.persistence = persistence;
        }

        public IEnumerable<Persistence.CharacterData> ActiveCharacters => persistence.Party.Members;

        public int GetAverageLevel()
        {
            var level = (int)persistence.Party.Members.Average(i => i.CharacterSheet.Level);
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

        public IEnumerable<CharacterSheet> ActiveCharacterSheets => persistence.Party.Members.Select(i => i.CharacterSheet);

        public long Gold
        {
            get
            {
                return persistence.Party.Gold;
            }
            set
            {
                persistence.Party.Gold = value;
            }
        }
    }
}
