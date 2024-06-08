using Adventure.Assets.PixelEffects;
using Adventure.Battle;
using Adventure.Services;
using Engine;
using RpgMath;
using System.Collections.Generic;
using System.Linq;

namespace Adventure.Skills
{
    class Reanimate : ISkill
    {
        public string Name { get; init; } = "Reanimate";

        public long MpCost { get; init; } = 40;

        public long Amount { get; init; } = 8;

        public bool DefaultTargetPlayers => true;

        public SkillAttackStyle AttackStyle => SkillAttackStyle.Cast;

        public Color CastColor => Color.FromARGB(0xff63c74c);

        public long GetMpCost(bool triggered, bool triggerSpammed) => MpCost;

        public ISkillEffect Apply(IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target, bool triggered, bool triggerSpammed)
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

            if (triggered)
            {
                damage *= 2;
            }

            if (triggerSpammed)
            {
                damage /= 2;
            }

            //Apply resistance
            var resistance = target.Stats.GetResistance(Element.Healing);
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

            var applyEffect = objectResolver.Resolve<Attachment<BattleScene>, IAttachment.Description>(o =>
            {
                var asset = new MagicBubbles();
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
                yield return coroutine.WaitSeconds(MagicBubbles.Duration);
                applyEffect.RequestDestruction();
            }
            coroutine.Run(run());

            return new SkillEffect(true);
        }
    }
}
