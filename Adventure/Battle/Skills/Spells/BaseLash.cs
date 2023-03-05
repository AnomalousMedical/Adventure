using Engine;
using RpgMath;
using Adventure.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Platform;
using Adventure.Services;

namespace Adventure.Battle.Skills
{
    class BaseLash : ISkill
    {
        private readonly Element element;

        //TODO: This mp cost was just made up, it might need more balancing
        public BaseLash(String name, Element element, long mpCost = 40, SkillAttackStyle attackStyle = SkillAttackStyle.Cast)
        {
            this.Name = name;
            this.element = element;
            this.MpCost = mpCost;
            this.AttackStyle = attackStyle;
        }

        public ISkillEffect Apply(IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target)
        {
            target = battleManager.ValidateTarget(attacker, target);
            var resistance = target.Stats.GetResistance(element);

            var effect = new SkillEffect();

            if (battleManager.DamageCalculator.MagicalHit(attacker.Stats, target.Stats, resistance, attacker.Stats.MagicAttackPercent))
            {
                ApplyDamage(battleManager, attacker, target, resistance);

                var applyEffect = objectResolver.Resolve<Attachment<BattleScene>, Attachment<BattleScene>.Description>(o =>
                {
                    ISpriteAsset asset = new Assets.PixelEffects.FireSpin();
                    o.RenderShadow = false;
                    o.Sprite = asset.CreateSprite();
                    o.SpriteMaterial = asset.CreateMaterial();
                    o.Light = new Light
                    {
                        Color = ElementColors.GetElementalColor(element),
                        Length = 2.3f,
                    };
                    o.LightOffset = new Vector3(0, 0, -0.1f);
                });
                applyEffect.SetPosition(target.MagicHitLocation, Quaternion.Identity, Vector3.ScaleIdentity);

                IEnumerator<YieldAction> run()
                {
                    yield return coroutine.WaitSeconds(0.5);
                    ApplyDamage(battleManager, attacker, target, resistance);

                    yield return coroutine.WaitSeconds(0.5);
                    applyEffect.RequestDestruction();
                    battleManager.HandleDeath(target);
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

        private static void ApplyDamage(IBattleManager battleManager, IBattleTarget attacker, IBattleTarget target, Resistance resistance)
        {
            var damage = battleManager.DamageCalculator.Magical(attacker.Stats, target.Stats, 20);
            damage = battleManager.DamageCalculator.ApplyResistance(damage, resistance);
            damage = battleManager.DamageCalculator.RandomVariation(damage);

            battleManager.AddDamageNumber(target, damage);
            target.ApplyDamage(attacker, battleManager.DamageCalculator, damage);
        }

        public string Name { get; }

        public long MpCost { get; }

        public SkillAttackStyle AttackStyle { get; }
    }
}
