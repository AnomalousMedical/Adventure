using Engine;
using RpgMath;
using Adventure.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adventure.Services;

namespace Adventure.Battle.Skills
{
    class BaseBlast : ISkill
    {
        private readonly Element element;

        //TODO: This is the original cost of tier 2 spells, but that was for single target, so this could be adjusted
        public BaseBlast(String name, Element element, long mpCost = 35, SkillAttackStyle attackStyle = SkillAttackStyle.Cast)
        {
            this.Name = name;
            this.element = element;
            this.MpCost = mpCost;
            this.AttackStyle = attackStyle;
        }

        public ISkillEffect Apply(IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target, bool triggered, bool triggerSpammed)
        {
            target = battleManager.ValidateTarget(attacker, target);
            IEnumerable<IBattleTarget> groupTargets = battleManager.GetTargetsInGroup(target).ToArray(); //It is important to make this copy, otherwise enumeration can fail on the death checks

            var applyEffects = new List<Attachment<BattleScene>>();

            foreach (var currentTarget in groupTargets)
            {
                var resistance = currentTarget.Stats.GetResistance(element);

                if (battleManager.DamageCalculator.MagicalHit(attacker.Stats, currentTarget.Stats, resistance, attacker.Stats.MagicAttackPercent))
                {
                    var damage = battleManager.DamageCalculator.Magical(attacker.Stats, currentTarget.Stats, 20);
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

        public string Name { get; }

        public long MpCost { get; }

        public SkillAttackStyle AttackStyle { get; }

        public Color CastColor => ElementColors.GetElementalColor(element);
    }
}
