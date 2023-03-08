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
    class BaseTempest : ISkill
    {
        private readonly Element element;

        //TODO: The mp cost could be double too, need to balance mp
        public BaseTempest(String name, Element element, long mpCost = 52, SkillAttackStyle attackStyle = SkillAttackStyle.Cast)
        {
            this.Name = name;
            this.element = element;
            this.MpCost = mpCost;
            this.AttackStyle = attackStyle;
        }

        public ISkillEffect Apply(IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target, bool triggered, bool triggerSpammed)
        {
            target = battleManager.ValidateTarget(attacker, target);
            var groupTargets = battleManager.GetTargetsInGroup(target);
            var hitTargets = new List<IBattleTarget>(10);

            var applyEffects = new List<Attachment<BattleScene>>();

            foreach (var currentTarget in groupTargets)
            {
                var resistance = currentTarget.Stats.GetResistance(element);

                if (battleManager.DamageCalculator.MagicalHit(attacker.Stats, currentTarget.Stats, resistance, attacker.Stats.MagicAttackPercent))
                {
                    hitTargets.Add(currentTarget);
                    ApplyDamage(battleManager, attacker, currentTarget, resistance);

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
                foreach (var currentTarget in hitTargets)
                {
                    var resistance = currentTarget.Stats.GetResistance(element);
                    ApplyDamage(battleManager, attacker, currentTarget, resistance);
                }

                yield return coroutine.WaitSeconds(0.5);
                foreach (var effect in applyEffects)
                {
                    effect.RequestDestruction();
                }
                foreach (var currentTarget in hitTargets)
                {
                    battleManager.HandleDeath(currentTarget);
                }
                effect.Finished = true;
            }
            coroutine.Run(run());

            return effect;
        }

        private static void ApplyDamage(IBattleManager battleManager, IBattleTarget attacker, IBattleTarget currentTarget, Resistance resistance)
        {
            var damage = battleManager.DamageCalculator.Magical(attacker.Stats, currentTarget.Stats, 20);
            damage = battleManager.DamageCalculator.ApplyResistance(damage, resistance);
            damage = battleManager.DamageCalculator.RandomVariation(damage);

            battleManager.AddDamageNumber(currentTarget, damage);
            currentTarget.ApplyDamage(attacker, battleManager.DamageCalculator, damage);
        }

        public string Name { get; }

        public long MpCost { get; }

        public SkillAttackStyle AttackStyle { get; }

        public Color CastColor => ElementColors.GetElementalColor(element);
    }
}
