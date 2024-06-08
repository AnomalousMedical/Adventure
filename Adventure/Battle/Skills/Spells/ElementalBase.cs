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
    class ElementalBase : ISkill
    {
        private readonly Element element;
        private readonly ISpriteAsset asset;
        private readonly ISoundEffect soundEffect;

        public ElementalBase(Element element, ISpriteAsset asset, ISoundEffect soundEffect)
        {
            this.element = element;
            this.asset = asset;
            this.soundEffect = soundEffect;
        }

        public long GetMpCost(bool triggered, bool triggerSpammed)
        {
            if(triggered || triggerSpammed)
            {
                return TriggeredMpCost;
            }

            return MpCost;
        }

        protected static readonly int BuffTypeId = 1000;

        public ISkillEffect Apply(IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target, bool triggered, bool triggerSpammed)
        {
            IEnumerable<IBattleTarget> groupTargets;
            var applyEffects = new List<Attachment<BattleScene>>();

            target = battleManager.ValidateTarget(attacker, target);
            var isBuff = BuffAlliesWithElement && target.BattleTargetType == attacker.BattleTargetType;

            if (isBuff)
            {
                //Buff allies if supported
                if (attacker.Stats.CanDoublecast && triggered && !triggerSpammed)
                {
                    groupTargets = battleManager.GetTargetsInGroup(target).Where(i => !i.IsDead).ToArray(); //It is important to make this copy, otherwise enumeration can fail on the death checks
                }
                else
                {
                    groupTargets = new[] { target };
                }

                foreach (var currentTarget in groupTargets)
                {
                    battleManager.AddDamageNumber(currentTarget, $"{element} Attack", Color.White);

                    var applyEffect = objectResolver.Resolve<Attachment<BattleScene>, IAttachment.Description>(o =>
                    {
                        ISpriteAsset asset = this.asset;
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
                    applyEffect.SetPosition(currentTarget.MagicHitLocation, Quaternion.Identity, Vector3.ScaleIdentity);
                    applyEffects.Add(applyEffect);

                    var buff = new CharacterBuff()
                    {
                        Name = Name,
                        AttackElements = new[] { element },
                        TimeRemaining = 2 * 60 * Clock.SecondsToMicro,
                        BuffTypeId = BuffTypeId
                    };
                    currentTarget.Stats.UpdateBuffs(buff);
                }
            }
            else
            {
                groupTargets = ApplyDamage(battleManager, objectResolver, attacker, target, coroutine, triggered, triggerSpammed, applyEffects);
            }

            var effect = new SkillEffect();
            IEnumerator<YieldAction> run()
            {
                yield return coroutine.WaitSeconds(0.5);
                CleanupAndHandleDeath(battleManager, groupTargets, applyEffects);
                if (!isBuff)
                {
                    if (attacker.Stats.CanDoublecast)
                    {
                        target = battleManager.ValidateTarget(attacker, target);
                        if (target != null)
                        {
                            applyEffects.Clear();
                            groupTargets = ApplyDamage(battleManager, objectResolver, attacker, target, coroutine, triggered, triggerSpammed, applyEffects);
                            if (groupTargets.Any())
                            {
                                yield return coroutine.WaitSeconds(0.5);
                                CleanupAndHandleDeath(battleManager, groupTargets, applyEffects);
                            }
                        }
                    }
                }
                effect.Finished = true;
            }
            coroutine.Run(run());

            return effect;
        }

        private static void CleanupAndHandleDeath(IBattleManager battleManager, IEnumerable<IBattleTarget> groupTargets, List<Attachment<BattleScene>> applyEffects)
        {
            foreach (var currentTarget in groupTargets)
            {
                battleManager.HandleDeath(currentTarget);
            }
            foreach (var applyEffect in applyEffects)
            {
                applyEffect.RequestDestruction();
            }
        }

        private IEnumerable<IBattleTarget> ApplyDamage(IBattleManager battleManager, IObjectResolver objectResolver, IBattleTarget attacker, IBattleTarget target, IScopedCoroutine coroutine, bool triggered, bool triggerSpammed, List<Attachment<BattleScene>> applyEffects)
        {
            IEnumerable<IBattleTarget> groupTargets;
            //Cause Damage

            if (HitGroupOnTrigger && triggered && !triggerSpammed)
            {
                groupTargets = battleManager.GetTargetsInGroup(target).ToArray(); //It is important to make this copy, otherwise enumeration can fail on the death checks
            }
            else
            {
                groupTargets = new[] { target };
            }

            if (groupTargets.Any())
            {
                battleManager.SoundEffectPlayer.PlaySound(soundEffect);
            }

            foreach (var currentTarget in groupTargets)
            {
                var resistance = currentTarget.Stats.GetResistance(element);

                if (battleManager.DamageCalculator.MagicalHit(attacker.Stats, currentTarget.Stats, resistance, attacker.Stats.MagicAttackPercent))
                {
                    var damage = battleManager.DamageCalculator.Magical(attacker.Stats, currentTarget.Stats, Power);
                    var originalDamage = damage;
                    damage = battleManager.DamageCalculator.ApplyResistance(damage, resistance);
                    var effectScale = Vector3.ScaleIdentity;
                    var numShakes = 1;
                    var createExtraSpellHits = false;
                    if(damage < originalDamage)
                    {
                        effectScale *= 0.75f;
                        numShakes = 0;
                    }
                    else if(damage > originalDamage)
                    {
                        numShakes = 5;
                        createExtraSpellHits = true;
                    }
                    //Intentionally unaltered if the same

                    damage = battleManager.DamageCalculator.RandomVariation(damage);

                    if (triggered)
                    {
                        damage *= 2;
                    }

                    if (triggerSpammed)
                    {
                        damage /= 2;
                        numShakes = 0;
                    }

                    battleManager.AddDamageNumber(currentTarget, damage);
                    currentTarget.ApplyDamage(attacker, battleManager.DamageCalculator, damage);

                    var applyEffect = objectResolver.Resolve<Attachment<BattleScene>, IAttachment.Description>(o =>
                    {
                        ISpriteAsset asset = this.asset;
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
                    applyEffect.SetPosition(currentTarget.MagicHitLocation, Quaternion.Identity, effectScale);
                    applyEffects.Add(applyEffect);

                    if (createExtraSpellHits)
                    {
                        Vector3 randomOffset;

                        applyEffect = objectResolver.Resolve<Attachment<BattleScene>, IAttachment.Description>(o =>
                        {
                            ISpriteAsset asset = this.asset;
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
                        randomOffset = new Vector3(((float)Random.Shared.NextDouble() - 0.5f) * 2.0f, (float)Random.Shared.NextDouble(), (float)Random.Shared.NextDouble() * -0.2f - 0.1f);
                        applyEffect.SetPosition(currentTarget.MagicHitLocation + randomOffset, Quaternion.Identity, effectScale);
                        applyEffects.Add(applyEffect);

                        applyEffect = objectResolver.Resolve<Attachment<BattleScene>, IAttachment.Description>(o =>
                        {
                            ISpriteAsset asset = this.asset;
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
                        randomOffset = new Vector3(((float)Random.Shared.NextDouble() - 0.5f) * 2.0f, (float)Random.Shared.NextDouble(), (float)Random.Shared.NextDouble() * 0.2f + 0.1f);
                        applyEffect.SetPosition(currentTarget.MagicHitLocation + randomOffset, Quaternion.Identity, effectScale);
                        applyEffects.Add(applyEffect);
                    }

                    IEnumerator<YieldAction> run()
                    {
                        const double waitTime = 0.075f;
                        var totalSeconds = 0.55;
                        var shook = true;
                        for (var i = 0; i < numShakes; ++i)
                        {
                            currentTarget.SetShakePosition(shook);
                            shook = !shook;
                            totalSeconds -= waitTime;
                            yield return coroutine.WaitSeconds(waitTime);
                        }
                        if (numShakes > 0)
                        {
                            currentTarget.SetShakePosition(false);
                        }
                    }
                    coroutine.Run(run());
                }
                else
                {
                    battleManager.AddDamageNumber(currentTarget, "Miss", Color.White);
                }
            }

            return groupTargets;
        }

        public string Name { get; init; }

        public long MpCost { get; init; }

        public long TriggeredMpCost { get; init; }

        public SkillAttackStyle AttackStyle { get; init; } = SkillAttackStyle.Cast;

        public long Power { get; init; }

        public Color CastColor => ElementColors.GetElementalColor(element);

        public bool HitGroupOnTrigger { get; init; }

        public bool BuffAlliesWithElement { get; set; }
    }
}
