using Adventure.Assets;
using Adventure.Assets.SoundEffects;
using Adventure.Services;
using Engine;
using Engine.Platform;
using RpgMath;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Adventure.Battle.Skills
{
    abstract class BuffSpell : ISkill
    {
        public string Name { get; init; }

        public long MpCost { get; init; }

        public long GetMpCost(bool triggered, bool triggerSpammed) => MpCost;

        public SkillAttackStyle AttackStyle => SkillAttackStyle.Cast;

        public bool DefaultTargetPlayers => true;

        public Color CastColor => Color.FromARGB(0xffffff74);

        public bool HealingItemsOnly { get; set; } = true;

        public ISoundEffect SoundEffect { get; set; }

        public void Apply(IDamageCalculator damageCalculator, CharacterSheet source, CharacterSheet target)
        {
            if (source.CurrentMp - MpCost < 0)
            {
                return;
            }

            source.CurrentMp -= MpCost;

            if (HealingItemsOnly && source.EquippedItems().Any(i => i.AttackElements?.Any(i => i == Element.Piercing || i == Element.Slashing) == true))
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

        public ISkillEffect Apply(IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target, bool triggered, bool triggerSpammed)
        {
            if (HealingItemsOnly && attacker.Stats.AttackElements.Any(i => i == Element.Piercing || i == Element.Slashing))
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
                ISpriteAsset asset = new Assets.PixelEffects.BuffEffect();
                o.RenderShadow = false;
                o.Sprite = asset.CreateSprite();
                o.SpriteMaterial = asset.CreateMaterial();
                o.Light = new Light
                {
                    Color = CastColor,
                    Length = 2.3f,
                };
                o.LightOffset = new Vector3(0, 0, -0.1f);
            });
            applyEffect.SetPosition(target.MagicHitLocation, Quaternion.Identity, Vector3.ScaleIdentity);
            if (SoundEffect != null)
            {
                battleManager.SoundEffectPlayer.PlaySound(SoundEffect);
            }

            IEnumerator<YieldAction> run()
            {
                yield return coroutine.WaitSeconds(1.0);
                applyEffect.RequestDestruction();
            }
            coroutine.Run(run());

            return new SkillEffect(true);
        }

        public static void UpdateBuffs(CharacterBuff buff, List<CharacterBuff> activeBuffs)
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

    class PhysicalBuff : BuffSpell
    {
        protected static readonly int Id = 0;

        public override string DamageNumberText => $"+{Amount} Strength and Vitality";

        public override CharacterBuff CreateBuff()
        {
            return new CharacterBuff()
            {
                Strength = Amount,
                Vitality = Amount,
                TimeRemaining = Duration,
                BuffTypeId = Id
            };
        }
    }

    class BattleCry : PhysicalBuff
    {
        public BattleCry()
        {
            Amount = 30;
            Name = "Battle Cry";
            MpCost = 35;
            SoundEffect = WarCrySpellSoundEffect.Instance;
        }
    }

    class WarCry : PhysicalBuff
    {
        public WarCry()
        {
            Amount = 45;
            Name = "War Cry";
            MpCost = 48;
            SoundEffect = WarCrySpellSoundEffect.Instance;
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
                Spirit = Amount,
                TimeRemaining = Duration,
                BuffTypeId = Id
            };
        }
    }

    class Focus : MagicBuff
    {
        public Focus()
        {
            Amount = 20;
            Name = "Focus";
            MpCost = 30;
        }
    }

    class IntenseFocus : MagicBuff
    {
        public IntenseFocus()
        {
            Amount = 45;
            Name = "Intense Focus";
            MpCost = 48;
        }
    }

    class Haste : BuffSpell
    {
        protected static readonly int Id = 4;

        public Haste()
        {
            Amount = 100;
            Name = "Haste";
            MpCost = 87;
            HealingItemsOnly = false;
            Duration = 23 * Clock.SecondsToMicro;
    }

        public override string DamageNumberText => "Haste";

        public override CharacterBuff CreateBuff()
        {
            return new CharacterBuff()
            {
                Dexterity = Amount,
                TimeRemaining = Duration,
                BuffTypeId = Id,
                QueueTurnsFront = true
            };
        }
    }
}
