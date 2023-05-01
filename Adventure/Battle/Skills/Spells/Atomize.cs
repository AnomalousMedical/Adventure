using Adventure.Assets;
using Adventure.Services;
using Engine;
using RpgMath;
using System.Collections.Generic;

namespace Adventure.Battle.Skills
{
    class Atomize : ISkill
    {
        private const int Power = 29;

        public bool QueueFront => true;

        public long GetMpCost(bool triggered, bool triggerSpammed)
        {
            if (triggered)
            {
                return 35;
            }

            if (triggerSpammed)
            {
                return 150;
            }

            return 85;
        }

        public ISkillEffect Apply(IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target, bool triggered, bool triggerSpammed)
        {
            target = battleManager.ValidateTarget(attacker, target);

            var applyEffects = new List<Attachment<BattleScene>>();

            var resistance = Resistance.Normal;

            if (battleManager.DamageCalculator.MagicalHit(attacker.Stats, target.Stats, resistance, attacker.Stats.MagicAttackPercent))
            {
                var damage = battleManager.DamageCalculator.Magical(attacker.Stats, target.Stats, Power);
                damage = battleManager.DamageCalculator.ApplyResistance(damage, resistance);
                damage = battleManager.DamageCalculator.RandomVariation(damage);

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
                applyEffects.Add(applyEffect);
            }
            else
            {
                battleManager.AddDamageNumber(target, "Miss", Color.White);
            }

            var effect = new SkillEffect();
            IEnumerator<YieldAction> run()
            {
                yield return coroutine.WaitSeconds(0.5);
                battleManager.HandleDeath(target);
                foreach (var effect in applyEffects)
                {
                    effect.RequestDestruction();
                }
                effect.Finished = true;
            }
            coroutine.Run(run());

            return effect;
        }

        public string Name => "Atomize";

        public SkillAttackStyle AttackStyle { get; init; } = SkillAttackStyle.Cast;

        static readonly Color Color = Color.FromARGB(0xffe109bb);
        public Color CastColor => Color;
    }
}
