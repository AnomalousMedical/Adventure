using Adventure.Assets;
using Adventure.Services;
using Engine;
using RpgMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Battle.Skills
{
    class Reanimate : ISkill
    {
        public string Name { get; init; } = "Reanimate";

        public long MpCost { get; init; } = 40;

        public long Amount { get; init; } = 8;

        public SkillAttackStyle AttackStyle => SkillAttackStyle.Cast;

        public void Apply(IDamageCalculator damageCalculator, CharacterSheet source, CharacterSheet target)
        {
            if (source.CurrentMp - MpCost < 0)
            {
                return;
            }

            source.CurrentMp -= MpCost;

            if (source.EquippedItems().Any(i => i.AttackElements?.Any(i => i == Element.Piercing || i == Element.Slashing) == true))
            {
                //Mp is taken, but nothing is done if cure can't be cast.
                return;
            }

            if (target.CurrentHp != 0) { return; }

            var damage = damageCalculator.Cure(source, Amount);
            damage = damageCalculator.RandomVariation(damage);

            damage *= -1; //Make it healing

            //Apply resistance
            var resistance = target.GetResistance(RpgMath.Element.Healing);
            damage = damageCalculator.ApplyResistance(damage, resistance);

            target.CurrentHp = damageCalculator.ApplyDamage(damage, target.CurrentHp, target.Hp);
        }

        public ISkillEffect Apply(IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target)
        {
            if (attacker.Stats.AttackElements.Any(i => i == Element.Piercing || i == Element.Slashing))
            {
                battleManager.AddDamageNumber(attacker, "Cannot cast restore magic", Color.Red);
                return new SkillEffect(true);
            }

            if (!battleManager.IsStillValidTarget(target))
            {
                target = battleManager.ValidateTarget(attacker, target);
            }
            var damage = battleManager.DamageCalculator.Cure(attacker.Stats, Amount);
            damage = battleManager.DamageCalculator.RandomVariation(damage);

            damage *= -1; //Make it healing

            //Apply resistance
            var resistance = target.Stats.GetResistance(RpgMath.Element.Healing);
            damage = battleManager.DamageCalculator.ApplyResistance(damage, resistance);

            if (target.IsDead || damage > 0)
            {
                battleManager.AddDamageNumber(target, damage);
            }
            else
            {
                battleManager.AddDamageNumber(target, "Miss", Color.Red);
            }
            target.Resurrect(battleManager.DamageCalculator, damage);
            battleManager.HandleDeath(target);

            var applyEffect = objectResolver.Resolve<Attachment<BattleScene>, Attachment<BattleScene>.Description>(o =>
            {
                var asset = new Assets.PixelEffects.MagicBubbles();
                o.RenderShadow = false;
                o.Sprite = asset.CreateSprite();
                o.SpriteMaterial = asset.CreateMaterial();
                o.Light = new Light
                {
                    Color = Color.FromARGB(0xff63c74c),
                    Length = 2.3f,
                };
                o.LightOffset = new Vector3(0, 0, -0.1f);
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
    }
}
