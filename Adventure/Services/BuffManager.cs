using Engine.Platform;
using RpgMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Services
{
    class BuffManager
    {
        private readonly Persistence persistence;

        public BuffManager(Persistence persistence)
        {
            this.persistence = persistence;
        }

        public void Update(Clock clock)
        {
            foreach(var member in persistence.Current.Party.Members)
            {
                var buffs = member.CharacterSheet.Buffs;
                for (var i = 0; i < buffs.Count;)
                {
                    var buff = buffs[i];
                    buff.TimeRemaining -= clock.DeltaTimeMicro;
                    if (buff.TimeRemaining <= 0)
                    {
                        buffs.RemoveAt(i);
                    }
                    else
                    {
                        ++i;
                    }
                }
            }
        }
    }
}
