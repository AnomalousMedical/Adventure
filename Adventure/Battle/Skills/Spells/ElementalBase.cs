using Adventure.Assets;
using Adventure.Services;
using Engine;
using Engine.Platform;
using RpgMath;
using System.Collections.Generic;
using System.Linq;

namespace Adventure.Battle.Skills
{
    class ElementalBase : ISkill
    {
        private readonly Element element;
        private readonly ISpriteAsset asset;

        public ElementalBase(Element element, ISpriteAsset asset = null)
        {
            this.element = element;
            this.asset = asset ?? new Assets.PixelEffects.FireSpin();
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

            if (BuffAlliesWithElement && target.BattleTargetType == attacker.BattleTargetType)
            {
                //Buff allies if supported and targeting allies
                groupTargets = new[] { target }; //For now just 1 target, but might change

                foreach (var currentTarget in groupTargets)
                {
                    battleManager.AddDamageNumber(currentTarget, $"{element} Attack", Color.White);

                    var applyEffect = objectResolver.Resolve<Attachment<BattleScene>, Attachment<BattleScene>.Description>(o =>
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
                        AttackElements = new[] { element },
                        TimeRemaining = 2 * 60 * Clock.SecondsToMicro,
                        BuffTypeId = BuffTypeId,
                        QueueTurnsFront = true
                    };
                    BuffSpell.UpdateBuffs(buff, currentTarget.Stats.Buffs);
                }
            }
            else
            {
                //Cause Damage

                if (HitGroupOnTrigger && triggered && !triggerSpammed)
                {
                    groupTargets = battleManager.GetTargetsInGroup(target).ToArray(); //It is important to make this copy, otherwise enumeration can fail on the death checks
                }
                else
                {
                    groupTargets = new[] { target };
                }

                foreach (var currentTarget in groupTargets)
                {
                    var resistance = currentTarget.Stats.GetResistance(element);

                    if (battleManager.DamageCalculator.MagicalHit(attacker.Stats, currentTarget.Stats, resistance, attacker.Stats.MagicAttackPercent))
                    {
                        var damage = battleManager.DamageCalculator.Magical(attacker.Stats, currentTarget.Stats, Power);
                        damage = battleManager.DamageCalculator.ApplyResistance(damage, resistance);
                        damage = battleManager.DamageCalculator.RandomVariation(damage);

                        if (triggered)
                        {
                            damage *= 2;
                        }

                        if (triggerSpammed)
                        {
                            damage /= 2;
                        }

                        battleManager.AddDamageNumber(currentTarget, damage);
                        currentTarget.ApplyDamage(attacker, battleManager.DamageCalculator, damage);

                        var applyEffect = objectResolver.Resolve<Attachment<BattleScene>, Attachment<BattleScene>.Description>(o =>
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
                    }
                    else
                    {
                        battleManager.AddDamageNumber(currentTarget, "Miss", Color.White);
                    }
                }
            }
            
            var effect = new SkillEffect();
            IEnumerator<YieldAction> run()
            {
                yield return coroutine.WaitSeconds(0.5);
                foreach (var currentTarget in groupTargets)
                {
                    battleManager.HandleDeath(currentTarget);
                }
                foreach (var effect in applyEffects)
                {
                    effect.RequestDestruction();
                }
                effect.Finished = true;
            }
            coroutine.Run(run());

            return effect;
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
