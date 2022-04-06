using Engine;
using Adventure.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RpgMath;

namespace Adventure.Battle.Skills
{
    class Cure : ISkill
    {
        public void Apply(IDamageCalculator damageCalculator, CharacterSheet source, CharacterSheet target)
        {
            if(source.CurrentMp - MpCost < 0)
            {
                return;
            }

            source.CurrentMp -= MpCost;

            if(target.CurrentHp == 0)
            {
                return;
            }

            var damage = damageCalculator.Cure(source, 5);
            damage = damageCalculator.RandomVariation(damage);

            damage *= -1; //Make it healing

            //Apply resistance
            var resistance = target.GetResistance(RpgMath.Element.Healing);
            damage = damageCalculator.ApplyResistance(damage, resistance);

            target.CurrentHp = damageCalculator.ApplyDamage(damage, target.CurrentHp, target.Hp);
        }

        public ISkillEffect Apply(IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target)
        {
            target = battleManager.ValidateTarget(attacker, target);
            var damage = battleManager.DamageCalculator.Cure(attacker.Stats, 5);
            damage = battleManager.DamageCalculator.RandomVariation(damage);

            damage *= -1; //Make it healing

            //Apply resistance
            var resistance = target.Stats.GetResistance(RpgMath.Element.Healing);
            damage = battleManager.DamageCalculator.ApplyResistance(damage, resistance);
            
            battleManager.AddDamageNumber(target, damage);
            target.ApplyDamage(battleManager.DamageCalculator, damage);
            battleManager.HandleDeath(target);

            var applyEffect = objectResolver.Resolve<Attachment<IBattleManager>, Attachment<IBattleManager>.Description>(o =>
            {
                ISpriteAsset asset = new Assets.PixelEffects.MagicBubbles();
                o.RenderShadow = false;
                o.Sprite = asset.CreateSprite();
                o.SpriteMaterial = asset.CreateMaterial();
            });
            applyEffect.SetPosition(target.MagicHitLocation, Quaternion.Identity, Vector3.ScaleIdentity);

            IEnumerator<YieldAction> run()
            {
                yield return coroutine.WaitSeconds(1.5);
                applyEffect.RequestDestruction();
            }
            coroutine.Run(run());

            return new SkillEffect(true);
        }

        public bool DefaultTargetPlayers => true;

        public string Name => "Cure";

        public long MpCost => 5;
    }
}
