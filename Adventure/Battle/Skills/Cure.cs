using Engine;
using Adventure.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RpgMath;
using Adventure.Services;

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

        public ISkillEffect Apply(IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target)
        {
            if(attacker.Stats.AttackElements.Any(i => i == Element.Piercing || i == Element.Slashing))
            {
                battleManager.AddDamageNumber(attacker, "Cannot cast restore magic", Color.Red);
                return new SkillEffect(true);
            }

            target = battleManager.ValidateTarget(attacker, target);
            var damage = battleManager.DamageCalculator.Cure(attacker.Stats, Amount);
            damage = battleManager.DamageCalculator.RandomVariation(damage);

            damage *= -1; //Make it healing

            //Apply resistance
            var resistance = target.Stats.GetResistance(RpgMath.Element.Healing);
            damage = battleManager.DamageCalculator.ApplyResistance(damage, resistance);
            
            battleManager.AddDamageNumber(target, damage);
            target.ApplyDamage(attacker, battleManager.DamageCalculator, damage);
            battleManager.HandleDeath(target);

            var applyEffect = objectResolver.Resolve<Attachment<IBattleManager>, Attachment<IBattleManager>.Description>(o =>
            {
                ISpriteAsset asset = new Assets.PixelEffects.MagicBubbles();
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

        public bool DefaultTargetPlayers => true;

        public string Name { get; init; } = "Cure";

        public long MpCost { get; init; } = 5;

        public long Amount { get; init; } = 5;

        public SkillAttackStyle AttackStyle => SkillAttackStyle.Cast;
    }

    class MegaCure : Cure
    {
        public MegaCure()
        {
            MpCost = 24;
            Amount = 35;
            Name = "Mega Cure";
        }
    }

    class UltraCure : Cure
    {
        public UltraCure()
        {
            MpCost = 64;
            Amount = 130;
            Name = "Ultra Cure";
        }
    }
}
