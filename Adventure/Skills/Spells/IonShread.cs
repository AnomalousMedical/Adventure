using Adventure.Assets;
using Adventure.Assets.SoundEffects;
using Adventure.Battle;
using Adventure.Services;
using Engine;
using RpgMath;
using System.Collections.Generic;

namespace Adventure.Skills.Spells
{
    class IonShread : ISkill
    {
        private const int Power = 111;

        public bool QueueFront => false;

        public long GetMpCost(bool triggered, bool triggerSpammed)
        {
            if (triggered)
            {
                return 50;
            }

            if (triggerSpammed)
            {
                return 250;
            }

            return 100;
        }

        public ISkillEffect Apply(IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target, bool triggered, bool triggerSpammed)
        {
            target = battleManager.ValidateTarget(attacker, target);

            var applyEffects = new List<Attachment<BattleScene>>();
            ApplyDamage(battleManager, objectResolver, attacker, target, coroutine, triggered, triggerSpammed, applyEffects);

            var effect = new SkillEffect();
            IEnumerator<YieldAction> run()
            {
                yield return coroutine.WaitSeconds(0.5);
                CleanupAndHandleDeath(battleManager, target, applyEffects);
                if (attacker.Stats.CanDoublecast)
                {
                    target = battleManager.ValidateTarget(attacker, target);
                    if (target != null)
                    {
                        applyEffects.Clear();
                        ApplyDamage(battleManager, objectResolver, attacker, target, coroutine, triggered, triggerSpammed, applyEffects);
                        yield return coroutine.WaitSeconds(0.5);
                        CleanupAndHandleDeath(battleManager, target, applyEffects);
                    }
                }
                effect.Finished = true;
            }
            coroutine.Run(run());

            return effect;
        }

        private static void CleanupAndHandleDeath(IBattleManager battleManager, IBattleTarget target, List<Attachment<BattleScene>> applyEffects)
        {
            battleManager.HandleDeath(target);
            foreach (var effect in applyEffects)
            {
                effect.RequestDestruction();
            }
        }

        private void ApplyDamage(IBattleManager battleManager, IObjectResolver objectResolver, IBattleTarget attacker, IBattleTarget target, IScopedCoroutine coroutine, bool triggered, bool triggerSpammed, List<Attachment<BattleScene>> applyEffects)
        {
            var damageEffectScale = DamageEffectScaler.GetEffectScale(attacker.Stats.CurrentMp, GetMpCost(triggered, triggerSpammed));

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

                damage = DamageEffectScaler.ApplyEffect(damage, damageEffectScale);

                battleManager.SoundEffectPlayer.PlaySound(IonShreadSpellSoundEffect.Instance);
                battleManager.AddDamageNumber(target, damage);
                target.ApplyDamage(attacker, battleManager.DamageCalculator, damage);

                var applyEffect = objectResolver.Resolve<Attachment<BattleScene>, IAttachment.Description>(o =>
                {
                    ISpriteAsset asset = new Assets.PixelEffects.IonicShreadEffect();
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

                IEnumerator<YieldAction> run()
                {
                    const int numShakes = 5;
                    const double waitTime = 0.075f;
                    var totalSeconds = 0.55;
                    var shook = true;
                    for (var i = 0; i < numShakes; ++i)
                    {
                        target.SetShakePosition(shook);
                        shook = !shook;
                        totalSeconds -= waitTime;
                        yield return coroutine.WaitSeconds(waitTime);
                    }
                    if (numShakes > 0)
                    {
                        target.SetShakePosition(false);
                    }
                }
                coroutine.Run(run());
            }
            else
            {
                battleManager.AddDamageNumber(target, "Miss", Color.White);
            }
        }

        public string Name => "Ion Shread";

        public SkillAttackStyle AttackStyle { get; init; } = SkillAttackStyle.Cast;

        static readonly Color Color = Color.FromARGB(0xffe109bb);
        public Color CastColor => Color;
    }
}
