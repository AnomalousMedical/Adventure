using Adventure.Assets;
using Adventure.Assets.PixelEffects;
using Adventure.Assets.SoundEffects;
using Adventure.Battle;
using Adventure.Services;
using Engine;
using Engine.Platform;
using RpgMath;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Adventure.Skills
{
    abstract class BuffSpell : ISkill
    {
        public string Name { get; init; }

        public long MpCost { get; init; }

        public long GetMpCost(bool triggered, bool triggerSpammed) => MpCost;

        public SkillAttackStyle AttackStyle => SkillAttackStyle.Cast;

        public bool DefaultTargetPlayers => true;

        public Color CastColor { get; init; }

        public bool HealingItemsOnly { get; init; } = true;

        public ISoundEffect SoundEffect { get; init; }

        public bool MultiTarget { get; init; }

        public bool UseInField => true;

        public ISpriteAsset EffectSpriteAsset { get; init; }

        public ISkillEffect Apply(IDamageCalculator damageCalculator, CharacterSheet source, CharacterSheet target, CharacterMenuPositionService characterMenuPositionService, IObjectResolver objectResolver, IScopedCoroutine coroutine, CameraMover cameraMover, ISoundEffectPlayer soundEffectPlayer)
        {
            if (source.CurrentMp == 0)
            {
                return null;
            }

            if (HealingItemsOnly && source.EquippedItems().Any(i => i.AttackElements?.Any(i => i == Element.Piercing || i == Element.Slashing) == true))
            {
                //Mp is taken, but nothing is done if cure can't be cast.
                return null;
            }

            if (target.CurrentHp <= 0)
            {
                return null;
            }

            var effectScale = DamageEffectScaler.GetEffectScale(source.CurrentMp, MpCost);

            source.CurrentMp -= MpCost;
            if(source.CurrentMp < 0)
            {
                source.CurrentMp = 0;
            }

            var buff = CreateBuff(DamageEffectScaler.ApplyEffect(Amount, effectScale));
            target.UpdateBuffs(buff);

            //Effect
            if (characterMenuPositionService.TryGetEntry(target, out var characterEntry))
            {
                cameraMover.SetInterpolatedGoalPosition(characterEntry.CameraPosition, characterEntry.CameraRotation);
                characterEntry.FaceCamera();

                var skillEffect = new CallbackSkillEffect(c => cameraMover.SetInterpolatedGoalPosition(characterEntry.CameraPosition, characterEntry.CameraRotation));
                IEnumerator<YieldAction> run()
                {
                    yield return coroutine.WaitSeconds(0.3f);

                    var applyEffects = new List<IAttachment>();

                    if (SoundEffect != null)
                    {
                        soundEffectPlayer.PlaySound(SoundEffect);
                    }

                    var attachmentType = typeof(Attachment<>).MakeGenericType(characterMenuPositionService.ActiveTrackerType);
                    var applyEffect = objectResolver.Resolve<IAttachment, IAttachment.Description>(attachmentType, o =>
                    {
                        o.RenderShadow = false;
                        o.Sprite = EffectSpriteAsset.CreateSprite();
                        o.SpriteMaterial = EffectSpriteAsset.CreateMaterial();
                        o.Light = new Light
                        {
                            Color = CastColor,
                            Length = 2.3f,
                        };
                        o.LightOffset = new Vector3(0, 0, -0.1f);
                    });

                    applyEffect.SetPosition(characterEntry.MagicHitLocation, Quaternion.Identity, characterEntry.Scale);
                    applyEffects.Add(applyEffect);

                    yield return coroutine.WaitSeconds(1.0);
                    foreach (var effect in applyEffects)
                    {
                        effect.RequestDestruction();
                    }
                    skillEffect.Finished = true;
                }
                coroutine.Run(run());

                return skillEffect;
            }
            else
            {
                return null;
            }
        }

        public ISkillEffect Apply(IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target, bool triggered, bool triggerSpammed)
        {
            var effectScale = DamageEffectScaler.GetEffectScale(attacker.Stats.CurrentMp, GetMpCost(triggered, triggerSpammed));

            if (HealingItemsOnly && attacker.Stats.AttackElements.Any(i => i == Element.Piercing || i == Element.Slashing))
            {
                battleManager.AddDamageNumber(attacker, "Cannot cast restore magic", Color.Red);
                return new SkillEffect(true);
            }

            var applyEffects = new List<Attachment<BattleScene>>();

            target = battleManager.ValidateTarget(attacker, target);
            IEnumerable<IBattleTarget> targets;
            if (MultiTarget && triggered)
            {
                targets = battleManager.GetTargetsInGroup(target).Where(i => !i.IsDead).ToArray(); //It is important to make this copy, otherwise enumeration can fail on the death checks
            }
            else
            {
                targets = new[] { target };
            }

            foreach (var currentTarget in targets)
            {
                var scaledAmount = DamageEffectScaler.ApplyEffect(Amount, effectScale);
                var buff = CreateBuff(scaledAmount);
                currentTarget.Stats.UpdateBuffs(buff);

                battleManager.AddDamageNumber(currentTarget, String.Format(DamageNumberText, scaledAmount), Color.White);

                var applyEffect = objectResolver.Resolve<Attachment<BattleScene>, IAttachment.Description>(o =>
                {
                    o.RenderShadow = false;
                    o.Sprite = EffectSpriteAsset.CreateSprite();
                    o.SpriteMaterial = EffectSpriteAsset.CreateMaterial();
                    o.Light = new Light
                    {
                        Color = CastColor,
                        Length = 2.3f,
                    };
                    o.LightOffset = new Vector3(0, 0, -0.1f);
                });
                applyEffect.SetPosition(currentTarget.MagicHitLocation, Quaternion.Identity, Vector3.ScaleIdentity);
                applyEffects.Add(applyEffect);
            }

            if (SoundEffect != null)
            {
                battleManager.SoundEffectPlayer.PlaySound(SoundEffect);
            }

            IEnumerator<YieldAction> run()
            {
                yield return coroutine.WaitSeconds(1.0);
                foreach (var applyEffect in applyEffects)
                {
                    applyEffect.RequestDestruction();
                }
            }
            coroutine.Run(run());

            return new SkillEffect(true);
        }

        public abstract CharacterBuff CreateBuff(int scaledAmount);

        public abstract String DamageNumberText { get; }

        public int Amount { get; set; }

        public long Duration { get; set; } = 5 * 60 * Clock.SecondsToMicro;
    }

    class PhysicalBuff : BuffSpell
    {
        protected static readonly int Id = 0;

        public override string DamageNumberText => "+{0} Strength and Vitality";

        public override CharacterBuff CreateBuff(int scaledAmount)
        {
            return new CharacterBuff()
            {
                Name = Name,
                Strength = scaledAmount,
                Vitality = scaledAmount,
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
            EffectSpriteAsset = new BuffEffect();
            CastColor = Color.FromARGB(0xffde2609);
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
            EffectSpriteAsset = new BuffEffect();
            MultiTarget = true;
            CastColor = Color.FromARGB(0xffde2609);
        }
    }

    class MagicBuff : BuffSpell
    {
        protected static readonly int Id = 1;

        public override string DamageNumberText => "+{0} Magic and Spirit";

        public override CharacterBuff CreateBuff(int scaledAmount)
        {
            return new CharacterBuff()
            {
                Name = Name,
                Magic = scaledAmount,
                Spirit = scaledAmount,
                TimeRemaining = Duration,
                BuffTypeId = Id
            };
        }
    }

    class Focus : MagicBuff
    {
        public Focus()
        {
            Amount = 30;
            Name = "Focus";
            MpCost = 35;
            SoundEffect = FocusSpellSoundEffect.Instance;
            EffectSpriteAsset = new BuffMagicEffect();
            CastColor = Color.FromARGB(0xffe109bb);
        }
    }

    class IntenseFocus : MagicBuff
    {
        public IntenseFocus()
        {
            Amount = 45;
            Name = "Intense Focus";
            MpCost = 48;
            MultiTarget = true;
            SoundEffect = FocusSpellSoundEffect.Instance;
            EffectSpriteAsset = new BuffMagicEffect();
            CastColor = Color.FromARGB(0xffe109bb);
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
            Duration = 2 * 60 * Clock.SecondsToMicro;
            EffectSpriteAsset = new BuffHasteEffect();
            SoundEffect = HasteSpellSoundEffect.Instance;
            CastColor = Color.FromARGB(0xffffff74);
        }

        public override string DamageNumberText => "Haste";

        public override CharacterBuff CreateBuff(int scaledAmount)
        {
            return new CharacterBuff()
            {
                Name = Name,
                Dexterity = scaledAmount,
                TimeRemaining = Duration,
                BuffTypeId = Id,
                QueueTurnsFront = true
            };
        }
    }
}
