using Adventure.Assets;
using Adventure.Services;
using Engine;
using RpgMath;
using System;
using System.Collections.Generic;

namespace Adventure.Battle.Skills
{
    abstract class Base : ISkill
    {
        private readonly Element element;

        public Base(String name, Element element, long mpCost, long power, SkillAttackStyle attackStyle = SkillAttackStyle.Cast)
        {
            this.Name = name;
            this.element = element;
            this.MpCost = mpCost;
            this.AttackStyle = attackStyle;
            this.Power = power;
        }

        public ISkillEffect Apply(IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target, bool triggered, bool triggerSpammed)
        {
            target = battleManager.ValidateTarget(attacker, target);
            var resistance = target.Stats.GetResistance(element);

            var effect = new SkillEffect();

            if (battleManager.DamageCalculator.MagicalHit(attacker.Stats, target.Stats, resistance, attacker.Stats.MagicAttackPercent))
            {
                var damage = battleManager.DamageCalculator.Magical(attacker.Stats, target.Stats, Power);
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

                battleManager.AddDamageNumber(target, damage);
                target.ApplyDamage(attacker, battleManager.DamageCalculator, damage);

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
                applyEffect.SetPosition(target.MagicHitLocation, Quaternion.Identity, Vector3.ScaleIdentity);

                IEnumerator<YieldAction> run()
                {
                    yield return coroutine.WaitSeconds(0.5);
                    battleManager.HandleDeath(target);
                    applyEffect.RequestDestruction();
                    effect.Finished = true;
                }
                coroutine.Run(run());
            }
            else
            {
                battleManager.AddDamageNumber(target, "Miss", Color.White);
                effect.Finished = true;
            }

            return effect;
        }

        public string Name { get; }

        public long MpCost { get; }

        public long Power { get; }

        public SkillAttackStyle AttackStyle { get; }

        public Color CastColor => ElementColors.GetElementalColor(element);
    }
}
