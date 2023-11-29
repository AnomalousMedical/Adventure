using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RpgMath
{
    [JsonConverter(typeof(JsonStringEnumConverter<StatusEffects>))]
    public enum StatusEffects
    {
        Sleep,
        Fumble,
        AttackFriendly,
        Mute,
        Blind,
        Poison,
        Slow,
        Stop,
        DeathTimer,
        Stone,
        AutoAttackOnly,
        AuraBoost,
        AuraDown,
        Undead
    }

    public class CharacterEffect
    {
        public StatusEffects StatusEffect { get; set; }

        public long TimeRemaining { get; set; }

        public long NextEffectTime { get; set; }

        public long AttackerMagicLevelSum { get; set; }

        public long Power { get; set; }
    }
}
