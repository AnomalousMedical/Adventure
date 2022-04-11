using Engine;
using RpgMath;
using Adventure.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Battle.Skills
{
    class Fire : ISkill
    {
        private const Element element = Element.Fire;

        public ISkillEffect Apply(IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target)
        {
            target = battleManager.ValidateTarget(attacker, target);
            var resistance = target.Stats.GetResistance(element);

            var effect = new SkillEffect();

            if (battleManager.DamageCalculator.MagicalHit(attacker.Stats, target.Stats, resistance, attacker.Stats.MagicAttackPercent))
            {
                var damage = battleManager.DamageCalculator.Magical(attacker.Stats, target.Stats, 8);
                damage = battleManager.DamageCalculator.ApplyResistance(damage, resistance);
                damage = battleManager.DamageCalculator.RandomVariation(damage);

                battleManager.AddDamageNumber(target, damage);
                target.ApplyDamage(battleManager.DamageCalculator, damage);

                var applyEffect = objectResolver.Resolve<Attachment<IBattleManager>, Attachment<IBattleManager>.Description>(o =>
                {
                    ISpriteAsset asset = new Assets.PixelEffects.FireSpin();
                    o.RenderShadow = false;
                    o.Sprite = asset.CreateSprite();
                    o.SpriteMaterial = asset.CreateMaterial();
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

        public string Name => "Fire";

        public long MpCost => 4;

        public SkillAttackStyle AttackStyle => SkillAttackStyle.Cast;
    }
}
