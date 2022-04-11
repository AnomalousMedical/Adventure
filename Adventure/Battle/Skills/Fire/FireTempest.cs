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
    class FireTempest : ISkill
    {
        public ISkillEffect Apply(IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target)
        {
            //This one needs a way to hit everything

            target = battleManager.ValidateTarget(attacker, target);
            var resistance = target.Stats.GetResistance(Element.Fire);

            var effect = new SkillEffect();

            if (battleManager.DamageCalculator.MagicalHit(attacker.Stats, target.Stats, resistance, attacker.Stats.MagicAttackPercent))
            {
                var damage = battleManager.DamageCalculator.Magical(attacker.Stats, target.Stats, 64);
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
                    yield return coroutine.WaitSeconds(1.5);
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

            return new SkillEffect(true);
        }

        public string Name => "Fire Tempest";

        public long MpCost => 52;

        public SkillAttackStyle AttackStyle => SkillAttackStyle.Cast;
    }
}
