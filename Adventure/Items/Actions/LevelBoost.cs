using Adventure.Assets.PixelEffects;
using Adventure.Assets.SoundEffects;
using Adventure.Assets;
using Adventure.Battle;
using Adventure.Services;
using Adventure.Skills;
using Engine;
using RpgMath;
using System.Collections.Generic;

namespace Adventure.Items.Actions
{
    internal class LevelBoost(ILevelCalculator levelCalculator) : IInventoryAction
    {
        private static Color CastColor => Color.FromARGB(0xffff0ee4);

        public void Use(InventoryItem item, Inventory inventory, CharacterSheet attacker, CharacterSheet target)
        {
            inventory.Items.Remove(item);

            LevelUpSheet(item, target);
        }

        public ISkillEffect Use(InventoryItem item, Inventory inventory, CharacterSheet attacker, CharacterSheet target, CharacterMenuPositionService characterMenuPositionService, IObjectResolver objectResolver, IScopedCoroutine coroutine, CameraMover cameraMover, ISoundEffectPlayer soundEffectPlayer)
        {
            inventory.Items.Remove(item);

            LevelUpSheet(item, target);

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
                        ISpriteAsset asset = new StatBoostEffect();
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
            inventory.Items.Remove(item); //If this is used on an enemy it does nothing, but its still lost

            if (target.BattleTargetType == BattleTargetType.Player)
            {
                var characterSheet = target.Stats as CharacterSheet;
                LevelUpSheet(item, characterSheet);

                battleManager.SoundEffectPlayer.PlaySound(CureSpellSoundEffect.Instance);

                var applyEffect = objectResolver.Resolve<Attachment<BattleScene>, IAttachment.Description>(o =>
                {
                    var asset = new StatBoostEffect();
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

        private void LevelUpSheet(InventoryItem item, CharacterSheet characterSheet)
        {
            for (int i = 0; i < item.Number; ++i)
            {
                characterSheet.LevelUp(levelCalculator);
            }
        }
    }
}
