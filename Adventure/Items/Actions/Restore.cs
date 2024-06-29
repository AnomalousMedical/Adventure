using Adventure.Assets;
using Adventure.Assets.PixelEffects;
using Adventure.Assets.SoundEffects;
using Adventure.Battle;
using Adventure.Services;
using Adventure.Skills;
using Engine;
using RpgMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Items.Actions
{
    class RestoreHp : IInventoryAction
    {
        public Color CastColor => Color.FromARGB(0xff63c74c);

        public void Use(InventoryItem item, Inventory inventory, CharacterSheet attacker, CharacterSheet target)
        {
            inventory.Items.Remove(item);

            if (target.CurrentHp == 0) { return; }

            target.CurrentHp += item.Number.Value + (long)(item.Number.Value * attacker.TotalItemUsageBonus);
            if(target.CurrentHp > target.Hp)
            {
                target.CurrentHp = target.Hp;
            }
        }

        public ISkillEffect Use(InventoryItem item, Inventory inventory, CharacterSheet attacker, CharacterSheet target, CharacterMenuPositionService characterMenuPositionService, IObjectResolver objectResolver, IScopedCoroutine coroutine, CameraMover cameraMover, ISoundEffectPlayer soundEffectPlayer)
        {
            inventory.Items.Remove(item);

            if (target.CurrentHp == 0)
            {
                return null;
            }

            target.CurrentHp += item.Number.Value + (long)(item.Number.Value * attacker.TotalItemUsageBonus);
            if (target.CurrentHp > target.Hp)
            {
                target.CurrentHp = target.Hp;
            }

            //Effect
            if (characterMenuPositionService.TryGetEntry(target, out var characterEntry))
            {
                cameraMover.SetInterpolatedGoalPosition(characterEntry.CameraPosition, characterEntry.CameraRotation);
                characterEntry.FaceCamera();

                var skillEffect = new CallbackSkillEffect(c => cameraMover.SetInterpolatedGoalPosition(characterEntry.CameraPosition, characterEntry.CameraRotation));
                IEnumerator<YieldAction> run()
                {
                    yield return coroutine.WaitSeconds(0.3f);

                    var applyEffects = new List<IAttachment>();

                    soundEffectPlayer.PlaySound(CureSpellSoundEffect.Instance);

                    var attachmentType = typeof(Attachment<>).MakeGenericType(characterMenuPositionService.ActiveTrackerType);
                    var applyEffect = objectResolver.Resolve<IAttachment, IAttachment.Description>(attachmentType, o =>
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

                    applyEffect.SetPosition(characterEntry.MagicHitLocation, Quaternion.Identity, characterEntry.Scale);
                    applyEffects.Add(applyEffect);

                    yield return coroutine.WaitSeconds(MagicBubbles.Duration);
                    foreach (var effect in applyEffects)
                    {
                        effect.RequestDestruction();
                    }
                    skillEffect.Finished = true;
                }
                coroutine.Run(run());

                return skillEffect;
            }
            else
            {
                return null;
            }
        }

        public void Use(InventoryItem item, Inventory inventory, IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target)
        {
            inventory.Items.Remove(item);

            target = battleManager.ValidateTarget(attacker, target);
            var damage = item.Number.Value + (long)(item.Number.Value * attacker.Stats.TotalItemUsageBonus);

            damage *= -1; //Make it healing

            //Apply resistance
            var resistance = target.Stats.GetResistance(RpgMath.Element.Healing);
            damage = battleManager.DamageCalculator.ApplyResistance(damage, resistance);

            battleManager.AddDamageNumber(target, damage);
            target.ApplyDamage(attacker, battleManager.DamageCalculator, damage);
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
        }
    }

    class RestoreMp : IInventoryAction
    {
        public Color CastColor => Color.FromARGB(0xff12e4ff);

        public void Use(InventoryItem item, Inventory inventory, CharacterSheet attacker, CharacterSheet target)
        {
            inventory.Items.Remove(item);

            if (target.CurrentHp == 0) { return; }

            target.CurrentMp += item.Number.Value + (long)(item.Number.Value * attacker.TotalItemUsageBonus);
            if (target.CurrentMp > target.Mp)
            {
                target.CurrentMp = target.Mp;
            }
        }

        public ISkillEffect Use(InventoryItem item, Inventory inventory, CharacterSheet attacker, CharacterSheet target, CharacterMenuPositionService characterMenuPositionService, IObjectResolver objectResolver, IScopedCoroutine coroutine, CameraMover cameraMover, ISoundEffectPlayer soundEffectPlayer)
        {
            inventory.Items.Remove(item);

            if (target.CurrentHp == 0)
            {
                return null;
            }

            target.CurrentMp += item.Number.Value + (long)(item.Number.Value * attacker.TotalItemUsageBonus);
            if (target.CurrentMp > target.Mp)
            {
                target.CurrentMp = target.Mp;
            }

            //Effect
            if (characterMenuPositionService.TryGetEntry(target, out var characterEntry))
            {
                cameraMover.SetInterpolatedGoalPosition(characterEntry.CameraPosition, characterEntry.CameraRotation);
                characterEntry.FaceCamera();

                var skillEffect = new CallbackSkillEffect(c => cameraMover.SetInterpolatedGoalPosition(characterEntry.CameraPosition, characterEntry.CameraRotation));
                IEnumerator<YieldAction> run()
                {
                    yield return coroutine.WaitSeconds(0.3f);

                    var applyEffects = new List<IAttachment>();

                    soundEffectPlayer.PlaySound(CureSpellSoundEffect.Instance);

                    var attachmentType = typeof(Attachment<>).MakeGenericType(characterMenuPositionService.ActiveTrackerType);
                    var applyEffect = objectResolver.Resolve<IAttachment, IAttachment.Description>(attachmentType, o =>
                    {
                        ISpriteAsset asset = new RestoreMpEffect();
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

                    applyEffect.SetPosition(characterEntry.MagicHitLocation, Quaternion.Identity, characterEntry.Scale);
                    applyEffects.Add(applyEffect);

                    yield return coroutine.WaitSeconds(RestoreMpEffect.Duration);
                    foreach (var effect in applyEffects)
                    {
                        effect.RequestDestruction();
                    }
                    skillEffect.Finished = true;
                }
                coroutine.Run(run());

                return skillEffect;
            }
            else
            {
                return null;
            }
        }

        public void Use(InventoryItem item, Inventory inventory, IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target)
        {
            inventory.Items.Remove(item);

            target = battleManager.ValidateTarget(attacker, target);
            var damage = item.Number.Value + (long)(item.Number.Value * attacker.Stats.TotalItemUsageBonus);

            damage *= -1; //Make it healing

            //Apply resistance
            var resistance = target.Stats.GetResistance(RpgMath.Element.MpRestore);
            damage = battleManager.DamageCalculator.ApplyResistance(damage, resistance);

            battleManager.AddDamageNumber(target, damage);
            target.TakeMp(damage);
            battleManager.HandleDeath(target);

            var applyEffect = objectResolver.Resolve<Attachment<BattleScene>, IAttachment.Description>(o =>
            {
                var asset = new RestoreMpEffect();
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
                yield return coroutine.WaitSeconds(RestoreMpEffect.Duration);
                applyEffect.RequestDestruction();
            }
            coroutine.Run(run());
        }
    }

    class Revive : IInventoryAction
    {
        public bool AllowTargetChange => false;

        public Color CastColor => Color.FromARGB(0xff63c74c);

        public void Use(InventoryItem item, Inventory inventory, CharacterSheet attacker, CharacterSheet target)
        {
            inventory.Items.Remove(item);

            if (target.CurrentHp != 0) { return; }

            target.CurrentHp += GetStartHp(target.Hp, item.Number.Value + (long)(item.Number.Value * attacker.TotalItemUsageBonus));
        }

        public ISkillEffect Use(InventoryItem item, Inventory inventory, CharacterSheet attacker, CharacterSheet target, CharacterMenuPositionService characterMenuPositionService, IObjectResolver objectResolver, IScopedCoroutine coroutine, CameraMover cameraMover, ISoundEffectPlayer soundEffectPlayer)
        {
            inventory.Items.Remove(item);

            if (target.CurrentHp != 0)
            {
                return null;
            }

            target.CurrentHp += GetStartHp(target.Hp, item.Number.Value + (long)(item.Number.Value * attacker.TotalItemUsageBonus));

            //Effect
            if (characterMenuPositionService.TryGetEntry(target, out var characterEntry))
            {
                cameraMover.SetInterpolatedGoalPosition(characterEntry.CameraPosition, characterEntry.CameraRotation);
                characterEntry.FaceCamera();

                var skillEffect = new CallbackSkillEffect(c => cameraMover.SetInterpolatedGoalPosition(characterEntry.CameraPosition, characterEntry.CameraRotation));
                IEnumerator<YieldAction> run()
                {
                    yield return coroutine.WaitSeconds(0.3f);

                    var applyEffects = new List<IAttachment>();

                    soundEffectPlayer.PlaySound(CureSpellSoundEffect.Instance);

                    var attachmentType = typeof(Attachment<>).MakeGenericType(characterMenuPositionService.ActiveTrackerType);
                    var applyEffect = objectResolver.Resolve<IAttachment, IAttachment.Description>(attachmentType, o =>
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

                    applyEffect.SetPosition(characterEntry.MagicHitLocation, Quaternion.Identity, characterEntry.Scale);
                    applyEffects.Add(applyEffect);

                    yield return coroutine.WaitSeconds(MagicBubbles.Duration);
                    foreach (var effect in applyEffects)
                    {
                        effect.RequestDestruction();
                    }
                    skillEffect.Finished = true;
                }
                coroutine.Run(run());

                return skillEffect;
            }
            else
            {
                return null;
            }
        }

        private long GetStartHp(long maxHp, long value)
        {
            return Math.Min((long)(maxHp * value * 0.01f), maxHp);
        }

        public void Use(InventoryItem item, Inventory inventory, IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target)
        {
            inventory.Items.Remove(item);

            if (!battleManager.IsStillValidTarget(target))
            {
                target = battleManager.ValidateTarget(attacker, target);
            }
            var damage = GetStartHp(target.Stats.Hp, item.Number.Value + (long)(item.Number.Value * attacker.Stats.TotalItemUsageBonus));

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
        }
    }
}
