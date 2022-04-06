using DiligentEngine;
using DiligentEngine.RT;
using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using RpgMath;
using Adventure.Assets;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adventure.Items;

namespace Adventure.Battle
{
    class BattlePlayer : IDisposable, IBattleTarget
    {
        private readonly IPlayerSprite playerSpriteInfo;
        private readonly RTInstances<IBattleManager> rtInstances;
        private readonly IDestructionRequest destructionRequest;
        private readonly IScopedCoroutine coroutine;
        private readonly IScaleHelper scaleHelper;
        private readonly IBattleScreenLayout battleScreenLayout;
        private readonly ICharacterTimer characterTimer;
        private readonly IBattleManager battleManager;
        private readonly ITurnTimer turnTimer;
        private readonly IObjectResolver objectResolver;
        private CharacterSheet characterSheet;
        private Inventory inventory;
        private readonly SpriteInstanceFactory spriteInstanceFactory;

        private readonly TLASBuildInstanceData tlasData;
        private SpriteInstance spriteInstance;
        private bool disposed = false;
        private int primaryHand;
        private int secondaryHand;
        private GamepadId gamepadId;
        private FrameEventSprite sprite;

        private Attachment<IBattleManager> mainHandItem;
        private Attachment<IBattleManager> offHandItem;
        private Attachment<IBattleManager> castEffect;

        private SharpButton attackButton = new SharpButton() { Text = "Attack" };
        private SharpButton skillsButton = new SharpButton() { Text = "Skills" };
        private SharpButton itemButton = new SharpButton() { Text = "Item" };
        private SharpButton defendButton = new SharpButton() { Text = "Defend" };

        private SharpProgressHorizontal turnProgress = new SharpProgressHorizontal();
        private SharpText name = new SharpText() { Color = Color.White };
        private SharpText currentHp = new SharpText() { Color = Color.White };
        private SharpText currentMp = new SharpText() { Color = Color.White };
        private ILayoutItem infoRowLayout;

        private IBattleSkills skills;
        private readonly BattleItemMenu itemMenu;
        private readonly IXpCalculator xpCalculator;
        private readonly ILevelCalculator levelCalculator;
        private readonly IAssetFactory assetFactory;

        public IBattleStats Stats => this.characterSheet;

        private Vector3 currentPosition;
        private Quaternion currentOrientation;
        private Vector3 currentScale;

        public Vector3 DamageDisplayLocation => this.currentPosition;

        public Vector3 CursorDisplayLocation => this.currentPosition + new Vector3(-0.5f * currentScale.x, 0.5f * currentScale.y, 0f);

        public Vector3 MeleeAttackLocation => this.currentPosition - new Vector3(sprite.BaseScale.x * 0.5f, 0, 0);

        public Vector3 MagicHitLocation => this.currentPosition + new Vector3(0f, 0f, -0.1f);

        public BattleTargetType BattleTargetType => BattleTargetType.Player;

        public ICharacterTimer CharacterTimer => characterTimer;

        public bool IsDead => characterSheet.CurrentHp == 0;

        public int BaseDexterity { get; internal set; }

        private Vector3 startPosition;

        public class Description : SceneObjectDesc
        {
            public int PrimaryHand = Player.RightHand;
            public int SecondaryHand = Player.LeftHand;
            public EventLayers EventLayer = EventLayers.Battle;
            public GamepadId Gamepad = GamepadId.Pad1;
            public CharacterSheet CharacterSheet;
            public Inventory Inventory;
            public String PlayerSprite { get; set; }
        }

        public BattlePlayer(
            RTInstances<IBattleManager> rtInstances,
            SpriteInstanceFactory spriteInstanceFactory,
            IDestructionRequest destructionRequest,
            Description description,
            ISpriteMaterialManager spriteMaterialManager,
            IObjectResolverFactory objectResolverFactory,
            IScopedCoroutine coroutine,
            IScaleHelper scaleHelper,
            IBattleScreenLayout battleScreenLayout,
            ICharacterTimer characterTimer,
            IBattleManager battleManager,
            ITurnTimer turnTimer,
            IBattleSkills skills,
            BattleItemMenu itemMenu,
            IXpCalculator xpCalculator,
            ILevelCalculator levelCalculator,
            IAssetFactory assetFactory,
            ISkillFactory skillFactory)
        {
            this.inventory = description.Inventory ?? throw new InvalidOperationException("You must include a inventory in the description");
            this.characterSheet = description.CharacterSheet ?? throw new InvalidOperationException("You must include a character sheet in the description");
            this.playerSpriteInfo = assetFactory.CreatePlayer(description.PlayerSprite ?? throw new InvalidOperationException($"You must include the {nameof(description.PlayerSprite)} property in your description."));
            this.skills = skills;
            this.itemMenu = itemMenu;
            this.xpCalculator = xpCalculator;
            this.levelCalculator = levelCalculator;
            this.assetFactory = assetFactory;
            this.rtInstances = rtInstances;
            this.spriteInstanceFactory = spriteInstanceFactory;
            this.destructionRequest = destructionRequest;
            this.coroutine = coroutine;
            this.scaleHelper = scaleHelper;
            this.battleScreenLayout = battleScreenLayout;
            this.characterTimer = characterTimer;
            this.battleManager = battleManager;
            this.turnTimer = turnTimer;
            this.primaryHand = description.PrimaryHand;
            this.secondaryHand = description.SecondaryHand;
            this.gamepadId = description.Gamepad;
            this.objectResolver = objectResolverFactory.Create();

            this.skills.AddRange(description.CharacterSheet.Skills.Select(i => skillFactory.CreateSkill(i)));

            turnProgress.DesiredSize = scaleHelper.Scaled(new IntSize2(200, 25));
            infoRowLayout = new RowLayout(
                new FixedWidthLayout(scaleHelper.Scaled(240), name),
                new FixedWidthLayout(scaleHelper.Scaled(165), currentHp),
                new FixedWidthLayout(scaleHelper.Scaled(125), currentMp),
                new FixedWidthLayout(scaleHelper.Scaled(210), turnProgress));
            battleScreenLayout.InfoColumn.Add(infoRowLayout);

            name.Text = description.CharacterSheet.Name;
            currentHp.Text = GetCurrentHpText();
            currentMp.Text = GetCurrentMpText();

            turnTimer.AddTimer(characterTimer);
            characterTimer.TurnReady += CharacterTimer_TurnReady;
            characterTimer.TotalDex = characterSheet.Dexterity;

            sprite = new FrameEventSprite(playerSpriteInfo.Animations);
            sprite.FrameChanged += Sprite_FrameChanged;
            sprite.SetAnimation("stand-left");

            var scale = description.Scale * sprite.BaseScale;
            var halfScale = scale.y / 2f;
            var startPos = description.Translation;
            startPos.y += halfScale;

            characterSheet.OnMainHandModified += OnMainHandModified;
            characterSheet.OnOffHandModified += OnOffHandModified;

            OnMainHandModified(characterSheet);
            OnOffHandModified(characterSheet);

            this.startPosition = startPos;
            this.currentPosition = startPos;
            this.currentOrientation = description.Orientation;
            this.currentScale = scale;

            this.tlasData = new TLASBuildInstanceData()
            {
                InstanceName = RTId.CreateId("BattlePlayer"),
                Mask = RtStructures.OPAQUE_GEOM_MASK,
                Transform = new InstanceMatrix(this.currentPosition, this.currentOrientation, this.currentScale)
            };

            Sprite_FrameChanged(sprite);

            coroutine.RunTask(async () =>
            {
                using var destructionBlock = destructionRequest.BlockDestruction(); //Block destruction until coroutine is finished and this is disposed.

                this.spriteInstance = await spriteInstanceFactory.Checkout(playerSpriteInfo.SpriteMaterialDescription);

                if (this.disposed)
                {
                    this.spriteInstanceFactory.TryReturn(spriteInstance);
                    return; //Stop loading
                }

                this.tlasData.pBLAS = spriteInstance.Instance.BLAS.Obj;

                rtInstances.AddTlasBuild(tlasData);
                rtInstances.AddShaderTableBinder(Bind);
                rtInstances.AddSprite(sprite);
            });
        }

        private String GetCurrentHpText()
        {
            return $"{characterSheet.CurrentHp} / {characterSheet.Hp}";
        }

        private String GetCurrentMpText()
        {
            return $"{characterSheet.CurrentMp} / {characterSheet.Mp}";
        }

        private bool guiActive = false;
        internal void SetGuiActive(bool active)
        {
            if (guiActive != active)
            {
                guiActive = active;
                if (guiActive)
                {
                    this.currentPosition = this.startPosition + new Vector3(-1f, 0f, 0f);
                    name.Color = Color.LightBlue;
                }
                else
                {
                    this.currentPosition = this.startPosition;
                    name.Color = Color.White;
                }
                Sprite_FrameChanged(sprite);
            }
        }

        public void Dispose()
        {
            characterSheet.OnMainHandModified -= OnMainHandModified;
            characterSheet.OnOffHandModified -= OnOffHandModified;

            turnTimer.RemoveTimer(characterTimer);
            battleScreenLayout.InfoColumn.Remove(infoRowLayout);
            characterTimer.TurnReady -= CharacterTimer_TurnReady;
            sprite.FrameChanged -= Sprite_FrameChanged;
            spriteInstanceFactory.TryReturn(spriteInstance);
            rtInstances.RemoveSprite(sprite);
            rtInstances.RemoveShaderTableBinder(Bind);
            rtInstances.RemoveTlasBuild(tlasData);
            objectResolver.Dispose();
        }

        public void DrawInfoGui(Clock clock, ISharpGui sharpGui, bool currentTarget = false)
        {
            if (currentTarget)
            {
                name.Color = Color.Red;
            }
            else
            {
                if (guiActive)
                {
                    name.Color = Color.LightBlue;
                }
                else
                {
                    name.Color = Color.White;
                }
            }

            sharpGui.Text(name);
            sharpGui.Text(currentHp);
            sharpGui.Text(currentMp);
            sharpGui.Progress(turnProgress, characterTimer.TurnTimerPct);
        }

        public enum MenuMode
        {
            Root,
            Magic,
            Item
        }

        private MenuMode currentMenuMode = MenuMode.Root;

        public bool UpdateActivePlayerGui(ISharpGui sharpGui)
        {
            bool didSomething = false;

            switch (currentMenuMode)
            {
                case MenuMode.Root:
                    didSomething = UpdateRootMenu(sharpGui, didSomething);
                    break;
                case MenuMode.Magic:
                    didSomething = skills.UpdateGui(sharpGui, coroutine, ref currentMenuMode, Cast);
                    break;
                case MenuMode.Item:
                    didSomething = itemMenu.UpdateGui(sharpGui, this, this.inventory, coroutine, ref currentMenuMode, UseItem);
                    break;
            }

            if (!didSomething)
            {
                switch (sharpGui.GamepadButtonEntered)
                {
                    case GamepadButtonCode.XInput_Y:
                        SwitchPlayer();
                        break;
                    default:
                        //Handle keyboard
                        switch (sharpGui.KeyEntered)
                        {
                            case KeyboardButtonCode.KC_LSHIFT:
                                SwitchPlayer();
                                break;
                        }
                        break;
                }
            }

            return didSomething;
        }

        private void SwitchPlayer()
        {
            currentMenuMode = MenuMode.Root;
            battleManager.SwitchPlayer();
        }

        private bool UpdateRootMenu(ISharpGui sharpGui, bool didSomething)
        {
            battleScreenLayout.LayoutBattleMenu(attackButton, skillsButton, itemButton, defendButton);

            if (sharpGui.Button(attackButton, navUp: defendButton.Id, navDown: skillsButton.Id))
            {
                coroutine.RunTask(async () =>
                {
                    var target = await battleManager.GetTarget(false);
                    if (target != null)
                    {
                        Attack(target);
                    }
                });
                didSomething = true;
            }

            if (sharpGui.Button(skillsButton, navUp: attackButton.Id, navDown: itemButton.Id))
            {
                currentMenuMode = MenuMode.Magic;
                didSomething = true;
            }

            if (sharpGui.Button(itemButton, navUp: skillsButton.Id, navDown: defendButton.Id))
            {
                currentMenuMode = MenuMode.Item;
                didSomething = true;
            }

            if (sharpGui.Button(defendButton, navUp: itemButton.Id, navDown: attackButton.Id))
            {
                didSomething = true;
            }

            return didSomething;
        }

        private void Attack(IBattleTarget target)
        {
            var swingEnd = Quaternion.Identity;
            var swingStart = new Quaternion(0f, MathF.PI / 2.1f, 0f);

            long remainingTime = (long)(1.8f * Clock.SecondsToMicro);
            long standTime = (long)(0.2f * Clock.SecondsToMicro);
            long standStartTime = remainingTime / 2;
            long swingTime = standStartTime - standTime / 3;
            long standEndTime = standStartTime - standTime;
            bool needsAttack = true;
            battleManager.DeactivateCurrentPlayer();
            battleManager.QueueTurn(c =>
            {
                if (IsDead)
                {
                    return true;
                }

                var done = false;
                remainingTime -= c.DeltaTimeMicro;
                Vector3 start;
                Vector3 end;
                float interpolate;

                if (remainingTime > standStartTime)
                {
                    sprite.SetAnimation("left");
                    target = battleManager.ValidateTarget(this, target);
                    start = this.startPosition;
                    end = GetAttackLocation(target);
                    interpolate = (remainingTime - standStartTime) / (float)standStartTime;
                }
                else if (remainingTime > standEndTime)
                {
                    var slerpAmount = (remainingTime - standEndTime) / (float)standEndTime;
                    mainHandItem?.SetAdditionalRotation(swingStart.slerp(swingEnd, slerpAmount));
                    sprite.SetAnimation("stand-left");
                    interpolate = 0.0f;
                    start = end = GetAttackLocation(target);

                    if (needsAttack && remainingTime < swingTime)
                    {
                        needsAttack = false;
                        battleManager.Attack(this, target);
                    }
                }
                else
                {
                    sprite.SetAnimation("right");

                    mainHandItem?.SetAdditionalRotation(Quaternion.Identity);

                    start = GetAttackLocation(target);
                    end = this.startPosition;
                    interpolate = remainingTime / (float)standEndTime;
                }

                this.currentPosition = end.lerp(start, interpolate);

                if (remainingTime < 0)
                {
                    sprite.SetAnimation("stand-left");
                    TurnComplete();
                    done = true;
                }

                Sprite_FrameChanged(sprite);

                return done;
            });
        }

        private Vector3 GetAttackLocation(IBattleTarget target)
        {
            var totalScale = sprite.BaseScale * currentScale;
            var targetAttackLocation = target.MeleeAttackLocation;
            targetAttackLocation.x += totalScale.x / 2;
            targetAttackLocation.y = totalScale.y / 2.0f;
            return targetAttackLocation;
        }

        private void Cast(IBattleTarget target, ISkill skill)
        {
            castEffect?.RequestDestruction();
            castEffect = objectResolver.Resolve<Attachment<IBattleManager>, Attachment<IBattleManager>.Description>(o =>
            {
                ISpriteAsset asset = new Assets.PixelEffects.Nebula();
                o.RenderShadow = false;
                o.Sprite = asset.CreateSprite();
                o.SpriteMaterial = asset.CreateMaterial();
            });

            var swingEnd = Quaternion.Identity;
            var swingStart = new Quaternion(0f, MathF.PI / 2.1f, 0f);

            long remainingTime = (long)(1.8f * Clock.SecondsToMicro);
            long standTime = (long)(0.2f * Clock.SecondsToMicro);
            long standStartTime = remainingTime / 2;
            long swingTime = standStartTime - standTime / 3;
            long standEndTime = standStartTime - standTime;
            bool needsAttack = true;
            ISkillEffect skillEffect = null;
            battleManager.DeactivateCurrentPlayer();
            battleManager.QueueTurn(c =>
            {
                if (IsDead)
                {
                    return true;
                }

                //If there is an effect, just let it run
                if (skillEffect != null && !skillEffect.Finished)
                {
                    skillEffect.Update(c);
                    return false;
                }

                var done = false;
                remainingTime -= c.DeltaTimeMicro;
                Vector3 start;
                Vector3 end;
                float interpolate;

                if (remainingTime > standStartTime)
                {
                    sprite.SetAnimation("stand-left");
                    target = battleManager.ValidateTarget(this, target);
                    start = this.startPosition;
                    end = target.MeleeAttackLocation;
                    interpolate = (remainingTime - standStartTime) / (float)standStartTime;
                }
                else if (remainingTime > standEndTime)
                {
                    var slerpAmount = (remainingTime - standEndTime) / (float)standEndTime;
                    //sword?.SetAdditionalRotation(swingStart.slerp(swingEnd, slerpAmount));
                    sprite.SetAnimation("cast-left");
                    interpolate = 0.0f;
                    start = target.MeleeAttackLocation;
                    end = target.MeleeAttackLocation;

                    if (needsAttack && remainingTime < swingTime)
                    {
                        needsAttack = false;
                        DestroyCastEffect();

                        if (characterSheet.CurrentMp < skill.MpCost)
                        {
                            battleManager.AddDamageNumber(this, "Not Enough MP", Color.Red);
                        }
                        else
                        {
                            TakeMp(skill.MpCost);
                            skillEffect = skill.Apply(battleManager, objectResolver, coroutine, this, target);
                        }
                    }
                }
                else
                {
                    sprite.SetAnimation("stand-left");

                    mainHandItem?.SetAdditionalRotation(Quaternion.Identity);

                    start = target.MeleeAttackLocation;
                    end = this.startPosition;
                    interpolate = remainingTime / (float)standEndTime;
                }

                if (remainingTime < 0)
                {
                    sprite.SetAnimation("stand-left");
                    TurnComplete();
                    done = true;
                }
                
                Sprite_FrameChanged(sprite);

                return done;
            });
        }

        private void UseItem(IBattleTarget target, InventoryItem item)
        {
            castEffect?.RequestDestruction();
            castEffect = objectResolver.Resolve<Attachment<IBattleManager>, Attachment<IBattleManager>.Description>(o =>
            {
                ISpriteAsset asset = new Assets.PixelEffects.Nebula();
                o.RenderShadow = false;
                o.Sprite = asset.CreateSprite();
                o.SpriteMaterial = asset.CreateMaterial();
            });

            var action = inventory.CreateAction(item);
            var swingEnd = Quaternion.Identity;
            var swingStart = new Quaternion(0f, MathF.PI / 2.1f, 0f);

            long remainingTime = (long)(1.8f * Clock.SecondsToMicro);
            long standTime = (long)(0.2f * Clock.SecondsToMicro);
            long standStartTime = remainingTime / 2;
            long swingTime = standStartTime - standTime / 3;
            long standEndTime = standStartTime - standTime;
            bool needsAttack = true;
            battleManager.DeactivateCurrentPlayer();
            battleManager.QueueTurn(c =>
            {
                if (IsDead)
                {
                    return true;
                }

                var done = false;
                remainingTime -= c.DeltaTimeMicro;
                Vector3 start;
                Vector3 end;
                float interpolate;

                if (remainingTime > standStartTime)
                {
                    sprite.SetAnimation("stand-left");
                    if (action.AllowTargetChange)
                    {
                        target = battleManager.ValidateTarget(this, target);
                    }
                    if (!battleManager.IsStillValidTarget(target))
                    {
                        return true; //No target, didn't find a new target give up
                    }
                    start = this.startPosition;
                    end = target.MeleeAttackLocation;
                    interpolate = (remainingTime - standStartTime) / (float)standStartTime;
                }
                else if (remainingTime > standEndTime)
                {
                    var slerpAmount = (remainingTime - standEndTime) / (float)standEndTime;
                    //sword?.SetAdditionalRotation(swingStart.slerp(swingEnd, slerpAmount));
                    sprite.SetAnimation("cast-left");
                    interpolate = 0.0f;
                    start = target.MeleeAttackLocation;
                    end = target.MeleeAttackLocation;

                    if (needsAttack && remainingTime < swingTime)
                    {
                        needsAttack = false;
                        DestroyCastEffect();

                        action.Use(item, inventory, battleManager, objectResolver, coroutine, this, target);
                    }
                }
                else
                {
                    sprite.SetAnimation("stand-left");

                    mainHandItem?.SetAdditionalRotation(Quaternion.Identity);

                    start = target.MeleeAttackLocation;
                    end = this.startPosition;
                    interpolate = remainingTime / (float)standEndTime;
                }

                if (remainingTime < 0)
                {
                    sprite.SetAnimation("stand-left");
                    TurnComplete();
                    done = true;
                }

                Sprite_FrameChanged(sprite);

                return done;
            });
        }

        private void DestroyCastEffect()
        {
            castEffect?.RequestDestruction();
            castEffect = null;
        }

        private void CharacterTimer_TurnReady(ICharacterTimer timer)
        {
            battleManager.AddToActivePlayers(this);
        }

        private void TurnComplete()
        {
            characterTimer.Reset();
            characterTimer.TurnTimerActive = true;
        }

        public void RequestDestruction()
        {
            destructionRequest.RequestDestruction();
        }

        private void Sprite_FrameChanged(FrameEventSprite obj)
        {
            var frame = obj.GetCurrentFrame();

            var scale = sprite.BaseScale * this.currentScale;
            this.tlasData.Transform = new InstanceMatrix(this.currentPosition, this.currentOrientation, scale);

            if(mainHandItem != null)
            {
                var primaryAttach = frame.Attachments[this.primaryHand];
                var offset = scale * primaryAttach.translate;
                offset = Quaternion.quatRotate(this.currentOrientation, offset) + this.currentPosition;
                mainHandItem.SetPosition(offset, this.currentOrientation, scale);
            }

            if(offHandItem != null)
            {
                var secondaryAttach = frame.Attachments[this.secondaryHand];
                var offset = scale * secondaryAttach.translate;
                offset = Quaternion.quatRotate(this.currentOrientation, offset) + this.currentPosition;
                offHandItem.SetPosition(offset, this.currentOrientation, scale);
            }

            if(castEffect != null)
            {
                var secondaryAttach = frame.Attachments[this.secondaryHand];
                var offset = scale * secondaryAttach.translate;
                offset = Quaternion.quatRotate(this.currentOrientation, offset) + this.currentPosition;
                castEffect.SetPosition(offset, this.currentOrientation, scale);
            }
        }

        public void ApplyDamage(IDamageCalculator calculator, long damage)
        {
            if (IsDead) { return; } //Do nothing if dead

            characterSheet.CurrentHp = calculator.ApplyDamage(damage, characterSheet.CurrentHp, characterSheet.Hp);
            currentHp.UpdateText(GetCurrentHpText());

            //Player died from applied damage
            if (IsDead)
            {
                battleManager.PlayerDead(this);
                characterTimer.TurnTimerActive = false;
                characterTimer.Reset();
            }
        }

        public void Resurrect(IDamageCalculator calculator, long damage)
        {
            if(damage < 0 && !IsDead) { return; } //Don't do anything if healing and not dead

            characterSheet.CurrentHp = calculator.ApplyDamage(damage, characterSheet.CurrentHp, characterSheet.Hp);
            currentHp.UpdateText(GetCurrentHpText());

            //Player died from applied damage
            if (IsDead)
            {
                battleManager.PlayerDead(this);
                characterTimer.TurnTimerActive = false;
            }
            else
            {
                characterTimer.TurnTimerActive = true;
            }

            characterTimer.Reset();
        }

        public void TakeMp(long mp)
        {
            if(IsDead) { return; }

            characterSheet.CurrentMp -= mp;
            if(characterSheet.CurrentMp < 0)
            {
                characterSheet.CurrentMp = 0;
            }
            else if (characterSheet.CurrentMp > characterSheet.Mp)
            {
                characterSheet.CurrentMp = characterSheet.Mp;
            }
            currentMp.UpdateText(GetCurrentMpText());
        }

        private void Bind(IShaderBindingTable sbt, ITopLevelAS tlas)
        {
            spriteInstance.Bind(this.tlasData.InstanceName, sbt, tlas, sprite.GetCurrentFrame());
        }

        private void OnMainHandModified(CharacterSheet obj)
        {
            mainHandItem?.RequestDestruction();
            if (characterSheet.MainHand?.Sprite != null)
            {
                mainHandItem = objectResolver.Resolve<Attachment<IBattleManager>, Attachment<IBattleManager>.Description>(o =>
                {
                    var asset = assetFactory.CreateEquipment(characterSheet.MainHand.Sprite);
                    o.Orientation = asset.GetOrientation();
                    o.Sprite = asset.CreateSprite();
                    o.SpriteMaterial = asset.CreateMaterial();
                });
            }
        }

        private void OnOffHandModified(CharacterSheet obj)
        {
            offHandItem?.RequestDestruction();
            if (characterSheet.OffHand?.Sprite != null)
            {
                offHandItem = objectResolver.Resolve<Attachment<IBattleManager>, Attachment<IBattleManager>.Description>(o =>
                {
                    var asset = assetFactory.CreateEquipment(characterSheet.OffHand.Sprite);
                    o.Orientation = asset.GetOrientation();
                    o.Sprite = asset.CreateSprite();
                    o.SpriteMaterial = asset.CreateMaterial();
                });
            }
        }
    }
}
