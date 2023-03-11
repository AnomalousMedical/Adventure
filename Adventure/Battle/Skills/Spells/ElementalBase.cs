using Adventure.Assets;
using Adventure.Services;
using Engine;
using RpgMath;
using System.Collections.Generic;
using System.Linq;

namespace Adventure.Battle.Skills
{
    class ElementalBase : ISkill
    {
        private readonly Element element;

        public ElementalBase(Element element)
        {
            this.element = element;
        }

        public long GetMpCost(bool triggered, bool triggerSpammed)
        {
            if(triggered || triggerSpammed)
            {
                return TriggeredMpCost;
            }

            return MpCost;
        }

        public ISkillEffect Apply(IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target, bool triggered, bool triggerSpammed)
        {
            target = battleManager.ValidateTarget(attacker, target);

            IEnumerable<IBattleTarget> groupTargets;

            if (HitGroupOnTrigger && triggered && !triggerSpammed)
            {
                groupTargets = battleManager.GetTargetsInGroup(target).ToArray(); //It is important to make this copy, otherwise enumeration can fail on the death checks
            }
            else
            {
                groupTargets = new[] { target };
            }

            var applyEffects = new List<Attachment<BattleScene>>();

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
                        ISpriteAsset asset = new Assets.PixelEffects.FireSpin();
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
    }
}
