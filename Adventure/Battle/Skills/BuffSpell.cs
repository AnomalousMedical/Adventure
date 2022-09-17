using Adventure.Assets;
using Adventure.Services;
using Engine;
using Engine.Platform;
using RpgMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Battle.Skills
{
    abstract class BuffSpell : ISkill
    {
        public string Name { get; init; }

        public long MpCost { get; init; }

        public SkillAttackStyle AttackStyle => SkillAttackStyle.Cast;

        public bool DefaultTargetPlayers => true;

        public void Apply(IDamageCalculator damageCalculator, CharacterSheet source, CharacterSheet target)
        {
            if (source.CurrentMp - MpCost < 0)
            {
                return;
            }

            source.CurrentMp -= MpCost;

            if (source.EquippedItems().Any(i => i.AttackElements?.Any(i => i == Element.Piercing || i == Element.Slashing) == true))
            {
                //Mp is taken, but nothing is done if cure can't be cast.
                return;
            }

            if (target.CurrentHp == 0)
            {
                return;
            }

            var buff = CreateBuff();
            var activeBuffs = target.Buffs;
            UpdateBuffs(buff, activeBuffs);
        }

        public ISkillEffect Apply(IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target)
        {
            if (attacker.Stats.AttackElements.Any(i => i == Element.Piercing || i == Element.Slashing))
            {
                battleManager.AddDamageNumber(attacker, "Cannot cast restore magic", Color.Red);
                return new SkillEffect(true);
            }

            target = battleManager.ValidateTarget(attacker, target);

            var buff = CreateBuff();
            var activeBuffs = target.Stats.Buffs;
            UpdateBuffs(buff, activeBuffs);

            battleManager.AddDamageNumber(target, DamageNumberText, Color.White);

            var applyEffect = objectResolver.Resolve<Attachment<BattleScene>, Attachment<BattleScene>.Description>(o =>
            {
                ISpriteAsset asset = new Assets.PixelEffects.MagicBubbles();
                o.RenderShadow = false;
                o.Sprite = asset.CreateSprite();
                o.SpriteMaterial = asset.CreateMaterial();
                o.Light = new Light
                {
                    Color = Color.FromARGB(0xff63c74c),
                    Length = 2.3f,
                };
                o.LightOffset = new Vector3(0, 0, -0.1f);
            });
            applyEffect.SetPosition(target.MagicHitLocation, Quaternion.Identity, Vector3.ScaleIdentity);

            IEnumerator<YieldAction> run()
            {
                yield return coroutine.WaitSeconds(1.5);
                applyEffect.RequestDestruction();
            }
            coroutine.Run(run());

            return new SkillEffect(true);
        }

        private static void UpdateBuffs(CharacterBuff buff, List<CharacterBuff> activeBuffs)
        {
            var count = activeBuffs.Count;
            for (var i = 0; i < count; i++)
            {
                var activeBuff = activeBuffs[i];
                if (activeBuff.BuffTypeId == buff.BuffTypeId)
                {
                    activeBuffs[i] = buff;
                    return;
                }
            }

            //If nothing was added above, add the new buff
            activeBuffs.Add(buff);
        }

        public abstract CharacterBuff CreateBuff();

        public abstract String DamageNumberText { get; }

        public int Amount { get; set; }

        public long Duration { get; set; } = 2 * 60 * Clock.SecondsToMicro;
    }

    class StrengthBuff : BuffSpell
    {
        protected static readonly int Id = 0;

        public override string DamageNumberText => $"+{Amount} Strength";

        public override CharacterBuff CreateBuff()
        {
            return new CharacterBuff()
            {
                Strength = Amount,
                TimeRemaining = Duration,
                BuffTypeId = Id
            };
        }
    }

    class MegaStrength : StrengthBuff
    {
        public MegaStrength()
        {
            Amount = 20;
            Name = "Mega Strength";
            MpCost = 25;
        }
    }

    class UltraStrength : StrengthBuff
    {
        public UltraStrength()
        {
            Amount = 45;
            Name = "Ultra Strength";
            MpCost = 65;
        }
    }

    class MagicBuff : BuffSpell
    {
        protected static readonly int Id = 1;

        public override string DamageNumberText => $"+{Amount} Magic";

        public override CharacterBuff CreateBuff()
        {
            return new CharacterBuff()
            {
                Magic = Amount,
                TimeRemaining = Duration,
                BuffTypeId = Id
            };
        }
    }

    class MegaMagic : MagicBuff
    {
        public MegaMagic()
        {
            Amount = 20;
            Name = "Mega Magic";
            MpCost = 25;
        }
    }

    class UltraMagic : MagicBuff
    {
        public UltraMagic()
        {
            Amount = 45;
            Name = "Ultra Magic";
            MpCost = 65;
        }
    }

    class VitalityBuff : BuffSpell
    {
        protected static readonly int Id = 2;

        public override string DamageNumberText => $"+{Amount} Vitality";

        public override CharacterBuff CreateBuff()
        {
            return new CharacterBuff()
            {
                Vitality = Amount,
                TimeRemaining = Duration,
                BuffTypeId = Id
            };
        }
    }

    class MegaVitality : VitalityBuff
    {
        public MegaVitality()
        {
            Amount = 20;
            Name = "Mega Vitality";
            MpCost = 25;
        }
    }

    class UltraVitality : VitalityBuff
    {
        public UltraVitality()
        {
            Amount = 45;
            Name = "Ultra Vitality";
            MpCost = 65;
        }
    }

    class SpiritBuff : BuffSpell
    {
        protected static readonly int Id = 3;

        public override string DamageNumberText => $"+{Amount} Spirit";

        public override CharacterBuff CreateBuff()
        {
            return new CharacterBuff()
            {
                Spirit = Amount,
                TimeRemaining = Duration,
                BuffTypeId = Id
            };
        }
    }

    class MegaSpirit : SpiritBuff
    {
        public MegaSpirit()
        {
            Amount = 20;
            Name = "Mega Spirit";
            MpCost = 25;
        }
    }

    class UltraSpirit : SpiritBuff
    {
        public UltraSpirit()
        {
            Amount = 45;
            Name = "Ultra Spirit";
            MpCost = 65;
        }
    }

    class Haste : BuffSpell
    {
        protected static readonly int Id = 4;

        public Haste()
        {
            Amount = 100;
            Name = "Haste";
            MpCost = 65;
        }

        public override string DamageNumberText => "Haste";

        public override CharacterBuff CreateBuff()
        {
            return new CharacterBuff()
            {
                Dexterity = Amount,
                TimeRemaining = Duration,
                BuffTypeId = Id
            };
        }
    }
}
