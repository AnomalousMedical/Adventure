using Adventure.Assets;
using Adventure.Assets.PixelEffects;
using Adventure.Assets.SoundEffects;
using Adventure.Services;
using Engine;
using RpgMath;
using System.Collections.Generic;
using System.Linq;

namespace Adventure.Battle.Skills
{
    class Cure : ISkill
    {
        public long GetMpCost(bool triggered, bool triggerSpammed) => MpCost;

        public void Apply(IDamageCalculator damageCalculator, CharacterSheet source, CharacterSheet target)
        {
            if(source.CurrentMp - MpCost < 0)
            {
                return;
            }

            source.CurrentMp -= MpCost;

            if (source.EquippedItems().Any(i => i.AttackElements?.Any(i => i == Element.Piercing || i == Element.Slashing) == true))
            {
                //Mp is taken, but nothing is done if cure can't be cast.
                return;
            }

            if(target.CurrentHp == 0)
            {
                return;
            }

            var damage = damageCalculator.Cure(source, Amount);
            damage = damageCalculator.RandomVariation(damage);

            damage *= -1; //Make it healing

            //Apply resistance
            var resistance = target.GetResistance(RpgMath.Element.Healing);
            damage = damageCalculator.ApplyResistance(damage, resistance);

            target.CurrentHp = damageCalculator.ApplyDamage(damage, target.CurrentHp, target.Hp);
        }

        public ISkillEffect Apply(IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target, bool triggered, bool triggerSpammed)
        {
            if(attacker.Stats.AttackElements.Any(i => i == Element.Piercing || i == Element.Slashing))
            {
                battleManager.AddDamageNumber(attacker, "Cannot cast restore magic", Color.Red);
                return new SkillEffect(true);
            }

            target = battleManager.ValidateTarget(attacker, target);
            IEnumerable<IBattleTarget> targets;
            if (attacker.Stats.CanCureAll && triggered)
            {
                targets = battleManager.GetTargetsInGroup(target).Where(i => !i.IsDead).ToArray(); //It is important to make this copy, otherwise enumeration can fail on the death checks
            }
            else
            {
                targets = new[] { target };
            }

            var applyEffects = new List<Attachment<BattleScene>>();

            battleManager.SoundEffectPlayer.PlaySound(CureSpellSoundEffect.Instance);

            foreach (var currentTarget in targets)
            {
                var damage = battleManager.DamageCalculator.Cure(attacker.Stats, Amount);
                damage = battleManager.DamageCalculator.RandomVariation(damage);

                damage *= -1; //Make it healing
                if(currentTarget != target)
                {
                    damage /= 15;
                }

                if (triggerSpammed)
                {
                    damage /= 2;
                }

                //Apply resistance
                var resistance = target.Stats.GetResistance(RpgMath.Element.Healing);
                damage = battleManager.DamageCalculator.ApplyResistance(damage, resistance);

                battleManager.AddDamageNumber(currentTarget, damage);
                currentTarget.ApplyDamage(attacker, battleManager.DamageCalculator, damage);

                var applyEffect = objectResolver.Resolve<Attachment<BattleScene>, Attachment<BattleScene>.Description>(o =>
                {
                    ISpriteAsset asset = new MagicBubbles();
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

            var effect = new SkillEffect();
            IEnumerator<YieldAction> run()
            {
                yield return coroutine.WaitSeconds(MagicBubbles.Duration);
                foreach (var currentTarget in targets)
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

        public bool DefaultTargetPlayers => true;

        public string Name { get; init; } = "Cure";

        public long MpCost { get; init; } = 17;

        public long Amount { get; init; } = 4;

        public SkillAttackStyle AttackStyle => SkillAttackStyle.Cast;

        public Color CastColor => Color.FromARGB(0xff63c74c);
    }

    class MegaCure : Cure
    {
        public MegaCure()
        {
            MpCost = 36;
            Amount = 35;
            Name = "Mega Cure";
        }
    }

    class UltraCure : Cure
    {
        public UltraCure()
        {
            MpCost = 62;
            Amount = 65;
            Name = "Ultra Cure";
        }
    }
}
