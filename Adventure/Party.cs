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

        public IEnumerable<Persistence.CharacterData> ActiveCharacters => persistence.Current.Party.Members;

        public int GetAverageLevel()
        {
            int level = 1;
            if (persistence.Current.Party.Members.Count > 0)
            {
                level = (int)persistence.Current.Party.Members.Average(i => i.CharacterSheet.Level);
                if (level < 1)
                {
                    level = 1;
                }
                else if (level > CharacterSheet.MaxLevel)
                {
                    level = CharacterSheet.MaxLevel;
                }
            }
            return level;
        }

        public long Gold
        {
            get
            {
                return persistence.Current.Party.Gold;
            }
            set
            {
                persistence.Current.Party.Gold = value;
            }
        }
    }
}
