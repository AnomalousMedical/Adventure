using Adventure.Assets;
using Adventure.Assets.SoundEffects;
using Adventure.Items;
using Adventure.Services;
using Adventure.Skills;
using Adventure.Skills.Spells;
using DiligentEngine;
using DiligentEngine.RT;
using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using RpgMath;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Adventure.Battle
{
    class BattlePlayer : IDisposable, IBattleTarget
    {
        private readonly IPlayerSprite playerSpriteInfo;
        private readonly RTInstances<BattleScene> rtInstances;
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

        public ISoundEffect DefaultAttackSoundEffect => PunchSoundEffect.Instance;

        private readonly TLASInstanceData tlasData;
        private SpriteInstance spriteInstance;
        private bool disposed = false;
        private int primaryHand;
        private int secondaryHand;
        private GamepadId gamepadId;
        private EventSprite sprite;

        private Attachment<BattleScene> mainHandItem;
        private Attachment<BattleScene> offHandItem;
        private Attachment<BattleScene> castEffect;
        private Attachment<BattleScene> mainHandHand;
        private Attachment<BattleScene> offHandHand;

        private SharpButton attackButton = new SharpButton() { Text = "Attack" };
        private SharpButton skillsButton = new SharpButton() { Text = "Skills" };
        private SharpButton itemButton = new SharpButton() { Text = "Item" };
        private SharpButton defendButton = new SharpButton() { Text = "Defend" };

        private SharpProgressHorizontal turnProgress = new SharpProgressHorizontal();
        private SharpProgressHorizontal powerProgress = new SharpProgressHorizontal();
        private RowLayout currentBuffsLayout = new RowLayout();
        private List<SharpImage> currentBuffs = new List<SharpImage>(4);
        private SharpText name = new SharpText() { Color = Color.UIWhite };
        private SharpText currentHp = new SharpText() { Color = Color.UIWhite };
        private SharpText currentMp = new SharpText() { Color = Color.UIWhite };
        private ILayoutItem infoRowLayout;

        private IBattleSkills skills;
        private readonly BattleItemMenu itemMenu;
        private readonly IAssetFactory assetFactory;
        private readonly ISkillFactory skillFactory;
        private readonly EventManager eventManager;
        private readonly IInventoryFunctions inventoryFunctions;
        private readonly KeybindService keybindService;
        private bool victorious = false;

        private SharpPanel infoPanel = new SharpPanel();
        private SharpStyle panelStyle = new SharpStyle() { Background = Color.FromARGB(0xbb020202) };

        public IBattleStats Stats => this.characterSheet;

        private Vector3 currentPosition;
        private Quaternion currentOrientation;
        private Vector3 currentScale;

        public Vector3 DamageDisplayLocation => this.currentPosition;

        private Vector3 ActivePosition => this.startPosition + new Vector3(-1.35f, 0f, 0f);

        private Vector3 DefendPosition => this.startPosition + new Vector3(+0.78f, 0f, 0f);

        public Vector3 CursorDisplayLocation => this.currentPosition + new Vector3(-0.5f * currentScale.x, 0.5f * currentScale.y, 0f);

        public Vector3 MeleeAttackLocation => this.currentPosition - new Vector3(sprite.BaseScale.x * 0.5f, 0, -0.01f);

        public Vector3 MagicHitLocation => this.currentPosition + new Vector3(0f, 0f, -0.1f);

        public Vector3 EffectScale => Vector3.ScaleIdentity;

        public BattleTargetType BattleTargetType => BattleTargetType.Player;

        public ICharacterTimer CharacterTimer => characterTimer;

        public bool IsDead => characterSheet.CurrentHp == 0;

        public GamepadId GamepadId => gamepadId;

        public long BaseDexterity => characterSheet.BaseDexterity;

        private bool offHandRaised = false;
        public bool OffHandRaised
        {
            get => offHandRaised;
            set
            {
                if (offHandRaised != value)
                {
                    offHandRaised = value;
                    Sprite_FrameChanged(sprite);
                }
            }
        }
        public bool IsDefending { get; private set; }

        private Vector3 startPosition;

        private static readonly Skills.Attack attack = new Skills.Attack();
        private static readonly Skills.CounterAttack counterAttack = new Skills.CounterAttack();
        private static readonly Skills.PowerAttack powerAttack = new Skills.PowerAttack();

        ButtonEvent contextTriggerKeyboard;
        ButtonEvent contextTriggerJoystick;
        SharpStyle uiStyle;
        Color nameHighlightColor;

        private SharpImage fire = new SharpImage();
        private SharpImage ice = new SharpImage();
        private SharpImage electricity = new SharpImage();
        private SharpImage physBuff = new SharpImage();
        private SharpImage mentalBuff = new SharpImage();
        private SharpImage haste = new SharpImage();

        private GamepadButtonCode switchCharacterButton;
        private KeyboardButtonCode switchCharacterKey;

        public class Description : SceneObjectDesc
        {
            public int PrimaryHand = Player.RightHand;
            public int SecondaryHand = Player.LeftHand;
            public EventLayers EventLayer = EventLayers.Battle;
            public GamepadId Gamepad = GamepadId.Pad1;
            public CharacterSheet CharacterSheet;
            public Inventory Inventory;
            public String PlayerSprite;
            public int CharacterStyleIndex;
        }

        public SharpStyle UiStyle => uiStyle;

        public BattlePlayer
        (
            RTInstances<BattleScene> rtInstances,
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
            IAssetFactory assetFactory,
            ISkillFactory skillFactory,
            EventManager eventManager,
            IInventoryFunctions inventoryFunctions,
            CharacterStyleService characterStyleService,
            IconLoader iconLoader,
            KeybindService keybindService
        )
        {
            this.contextTriggerKeyboard = new ButtonEvent(description.EventLayer, keys: keybindService.GetKeyboardBinding(KeyBindings.ActiveAbility));
            this.contextTriggerJoystick = new ButtonEvent(description.EventLayer, gamepadButtons: keybindService.GetGamepadBindingArray(KeyBindings.ActiveAbility, description.Gamepad)) { Pad = description.Gamepad };
            eventManager.addEvent(contextTriggerKeyboard);
            eventManager.addEvent(contextTriggerJoystick);

            this.inventory = description.Inventory ?? throw new InvalidOperationException("You must include a inventory in the description");
            this.characterSheet = description.CharacterSheet ?? throw new InvalidOperationException("You must include a character sheet in the description");
            this.playerSpriteInfo = assetFactory.CreatePlayer(description.PlayerSprite ?? throw new InvalidOperationException($"You must include the {nameof(description.PlayerSprite)} property in your description."));
            this.uiStyle = characterStyleService.GetCharacterStyle(description.CharacterStyleIndex);
            this.nameHighlightColor = characterStyleService.GetNameHighlightColor(description.CharacterStyleIndex);
            this.skills = skills;
            this.itemMenu = itemMenu;
            this.assetFactory = assetFactory;
            this.skillFactory = skillFactory;
            this.eventManager = eventManager;
            this.inventoryFunctions = inventoryFunctions;
            this.keybindService = keybindService;
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

            this.currentBuffsLayout.Margin = new IntPad(scaleHelper.Scaled(5), 0, scaleHelper.Scaled(5), 0);

            UpdateSkills();

            turnProgress.DesiredSize = scaleHelper.Scaled(new IntSize2(200, 25));
            powerProgress.DesiredSize = scaleHelper.Scaled(new IntSize2(200, 25));
            infoRowLayout =
                new PanelLayoutNoPad(infoPanel,
                new KeepWidthRightLayout(new RowLayout(
                    new KeepHeightLayout(currentBuffsLayout),
                    name,
                    new FixedWidthLayout(scaleHelper.Scaled(165), currentHp),
                    new FixedWidthLayout(scaleHelper.Scaled(125), currentMp),
                    new FixedWidthLayout(scaleHelper.Scaled(210), powerProgress),
                    new FixedWidthLayout(scaleHelper.Scaled(210), turnProgress)
                ) { Margin = new IntPad(scaleHelper.Scaled(9)) }));
            battleScreenLayout.InfoColumn.Add(infoRowLayout);

            UpdateCurrentBuffs();
            name.Text = description.CharacterSheet.Name;
            currentHp.Text = GetCurrentHpText();
            currentHp.Color = GetCurrentHpTextColor();
            currentMp.Text = GetCurrentMpText();
            currentMp.Color = GetCurrentMpTextColor();

            turnTimer.AddTimer(characterTimer);
            characterTimer.TurnReady += CharacterTimer_TurnReady;
            characterTimer.TotalDex = () => characterSheet.TotalDexterity;

            sprite = new EventSprite(new Sprite(playerSpriteInfo.Animations));
            sprite.FrameChanged += Sprite_FrameChanged;
            sprite.AnimationChanged += Sprite_AnimationChanged;
            sprite.SetAnimation(IsDead ? "dead" : "stand-left");

            var scale = description.Scale * sprite.BaseScale;
            var halfScale = scale.y / 2f;
            var startPos = description.Translation;
            startPos.y += halfScale;

            this.startPosition = startPos;
            this.currentPosition = startPos;
            this.currentOrientation = description.Orientation;
            this.currentScale = scale;

            this.tlasData = new TLASInstanceData()
            {
                InstanceName = RTId.CreateId("BattlePlayer"),
                Mask = RtStructures.OPAQUE_GEOM_MASK,
                Transform = new InstanceMatrix(this.currentPosition, this.currentOrientation, this.currentScale)
            };

            characterSheet.OnMainHandModified += OnMainHandModified;
            characterSheet.OnOffHandModified += OnOffHandModified;
            characterSheet.OnBodyModified += CharacterSheet_OnBodyModified;
            characterSheet.OnBuffsModified += CharacterSheet_OnBuffsModified;

            OnMainHandModified(characterSheet);
            OnOffHandModified(characterSheet);

            Sprite_FrameChanged(sprite);

            keybindService.KeybindChanged += KeybindService_KeybindChanged;
            switchCharacterButton = keybindService.GetGamepadBinding(KeyBindings.SwitchCharacter, gamepadId);
            switchCharacterKey = keybindService.GetKeyboardMouseBinding(KeyBindings.SwitchCharacter).KeyboardButton.Value;

            coroutine.RunTask(async () =>
            {
                using var destructionBlock = destructionRequest.BlockDestruction(); //Block destruction until coroutine is finished and this is disposed.

                this.spriteInstance = await spriteInstanceFactory.Checkout(playerSpriteInfo.GetTier(characterSheet.EquipmentTier), sprite);

                if (this.disposed)
                {
                    this.spriteInstanceFactory.TryReturn(spriteInstance);
                    return; //Stop loading
                }

                rtInstances.AddTlasBuild(tlasData);
                rtInstances.AddShaderTableBinder(Bind);
                rtInstances.AddSprite(sprite, tlasData, spriteInstance);
            });

            var iconSize = scaleHelper.Scaled(20);
            fire.Image = iconLoader.Icons;
            fire.UvRect = iconLoader.Fire;
            fire.DesiredWidth = iconSize;
            ice.Image = iconLoader.Icons;
            ice.UvRect = iconLoader.Ice;
            ice.DesiredWidth = iconSize;
            electricity.Image = iconLoader.Icons;
            electricity.UvRect = iconLoader.Electricity;
            electricity.DesiredWidth = iconSize;
            physBuff.Image = iconLoader.Icons;
            physBuff.UvRect = iconLoader.PhysBuff;
            physBuff.DesiredWidth = iconSize;
            mentalBuff.Image = iconLoader.Icons;
            mentalBuff.UvRect = iconLoader.MentalBuff;
            mentalBuff.DesiredWidth = iconSize;
            haste.Image = iconLoader.Icons;
            haste.UvRect = iconLoader.Haste;
            haste.DesiredWidth = iconSize;
        }

        private void KeybindService_KeybindChanged(KeybindService service, KeyBindings keyBinding)
        {
            switch (keyBinding)
            {
                case KeyBindings.SwitchCharacter:
                    switchCharacterButton = keybindService.GetGamepadBinding(keyBinding, gamepadId);
                    switchCharacterKey = keybindService.GetKeyboardMouseBinding(keyBinding).KeyboardButton.Value;
                    break;
                case KeyBindings.ActiveAbility:
                    contextTriggerJoystick.clearButtons();
                    contextTriggerJoystick.addButtons(keybindService.GetGamepadBindingArray(keyBinding, gamepadId));
                    contextTriggerKeyboard.clearButtons();
                    contextTriggerKeyboard.addButtons(keybindService.GetKeyboardBinding(keyBinding));
                    break;
            }
        }

        private String GetCurrentHpText()
        {
            return $"{characterSheet.CurrentHp} / {characterSheet.Hp}";
        }

        private Color GetCurrentHpTextColor()
        {
            return characterSheet.CurrentHp > 0 && (float)characterSheet.CurrentHp / characterSheet.Hp < 0.35f ? Color.Yellow : Color.UIWhite;
        }

        private String GetCurrentMpText()
        {
            return $"{characterSheet.CurrentMp} / {characterSheet.Mp}";
        }

        private void UpdateCurrentBuffs()
        {
            currentBuffsLayout.Clear();
            currentBuffs.Clear();
            foreach(var buff in characterSheet.Buffs)
            {
                switch (buff.BuffTypeId)
                {
                    case PhysicalBuff.Id:
                        currentBuffsLayout.Add(physBuff);
                        currentBuffs.Add(physBuff);
                        break;
                    case MagicBuff.Id:
                        currentBuffsLayout.Add(mentalBuff);
                        currentBuffs.Add(mentalBuff);
                        break;
                    case Haste.Id:
                        currentBuffsLayout.Add(haste);
                        currentBuffs.Add(haste);
                        break;
                    case ElementalBase.BuffTypeId:
                        foreach(var element in buff.AttackElements)
                        {
                            switch (element)
                            {
                                case Element.Fire:
                                    currentBuffsLayout.Add(fire);
                                    currentBuffs.Add(fire);
                                    break;
                                case Element.Ice:
                                    currentBuffsLayout.Add(ice);
                                    currentBuffs.Add(ice);
                                    break;
                                case Element.Electricity:
                                    currentBuffsLayout.Add(electricity);
                                    currentBuffs.Add(electricity);
                                    break;
                            }
                        }
                        break;
                }
            }
        }

        private Color GetCurrentMpTextColor()
        {
            return characterSheet.CurrentHp > 0 && characterSheet.CurrentMp > 0 && (float)characterSheet.CurrentMp / characterSheet.Mp < 0.35f ? Color.Yellow : Color.UIWhite;
        }

        private bool guiActive = false;
        internal void SetGuiActive(bool active)
        {
            if (guiActive != active)
            {
                guiActive = active && !victorious;
                if (guiActive)
                {
                    this.currentPosition = ActivePosition;
                    name.Color = nameHighlightColor;
                    IsDefending = false;
                    sprite.SetAnimation("stand-down");
                }
                else
                {
                    sprite.SetAnimation(IsDead ? "dead" : "stand-left");
                    if (IsDefending)
                    {
                        this.currentPosition = DefendPosition;
                    }
                    else
                    {
                        this.currentPosition = this.startPosition;
                    }
                    name.Color = Color.UIWhite;
                }
                Sprite_FrameChanged(sprite);
            }
        }

        public void Dispose()
        {
            characterSheet.OnBodyModified -= CharacterSheet_OnBodyModified;
            characterSheet.OnMainHandModified -= OnMainHandModified;
            characterSheet.OnOffHandModified -= OnOffHandModified;
            characterSheet.OnBuffsModified -= CharacterSheet_OnBuffsModified;

            keybindService.KeybindChanged -= KeybindService_KeybindChanged;

            eventManager.removeEvent(contextTriggerKeyboard);
            eventManager.removeEvent(contextTriggerJoystick);

            turnTimer.RemoveTimer(characterTimer);
            battleScreenLayout.InfoColumn.Remove(infoRowLayout);
            characterTimer.TurnReady -= CharacterTimer_TurnReady;
            sprite.FrameChanged -= Sprite_FrameChanged;
            sprite.AnimationChanged -= Sprite_AnimationChanged;
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
                name.Color = Color.UIRed;
            }
            else
            {
                if (guiActive)
                {
                    name.Color = nameHighlightColor;
                }
                else
                {
                    name.Color = Color.UIWhite;
                }
            }

            sharpGui.Panel(infoPanel, panelStyle);
            foreach(var buff in currentBuffs)
            {
                sharpGui.Image(buff);
            }
            sharpGui.Text(name);
            sharpGui.Text(currentHp);
            sharpGui.Text(currentMp);
            sharpGui.Progress(turnProgress, characterTimer.TurnTimerPct);
            sharpGui.Progress(powerProgress, characterSheet.PowerGaugePct);
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
                    didSomething = skills.UpdateGui(sharpGui, coroutine, ref currentMenuMode, UseSkill, gamepadId, uiStyle);
                    break;
                case MenuMode.Item:
                    didSomething = itemMenu.UpdateGui(sharpGui, this, this.inventory, coroutine, ref currentMenuMode, UseItem, gamepadId, uiStyle);
                    break;
            }

            if (!didSomething)
            {
                if (sharpGui.GamepadButtonEntered[(int)gamepadId] == switchCharacterButton
                    || sharpGui.KeyEntered == switchCharacterKey)
                {
                        SwitchPlayer();
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
            attackButton.Text = characterSheet.AtPowerMax ? "PowAtk" : "Attack";

            if (sharpGui.Button(attackButton, gamepadId, navUp: defendButton.Id, navDown: skillsButton.Id, style: uiStyle))
            {
                coroutine.RunTask(async () =>
                {
                    var target = await battleManager.GetTarget(false);
                    if (target != null)
                    {
                        if (characterSheet.AtPowerMax)
                        {
                            characterSheet.PowerGauge = 0;
                            Melee(target, powerAttack, true, true);
                        }
                        else
                        {
                            Melee(target, attack, false, true);
                        }
                    }
                });
                didSomething = true;
            }

            if (sharpGui.Button(skillsButton, gamepadId, navUp: attackButton.Id, navDown: itemButton.Id, style: uiStyle))
            {
                currentMenuMode = MenuMode.Magic;
                didSomething = true;
            }

            if (sharpGui.Button(itemButton, gamepadId, navUp: skillsButton.Id, navDown: defendButton.Id, style: uiStyle))
            {
                currentMenuMode = MenuMode.Item;
                didSomething = true;
            }

            if (sharpGui.Button(defendButton, gamepadId, navUp: itemButton.Id, navDown: attackButton.Id, style: uiStyle))
            {
                IsDefending = true;
                battleManager.QueueTurn(c =>
                {
                    TurnComplete();
                    return true;
                });
                battleManager.DeactivateCurrentPlayer();
                this.currentPosition = DefendPosition;
                Sprite_FrameChanged(sprite);
                didSomething = true;
            }

            return didSomething;
        }

        private Vector3 GetAttackLocation(IBattleTarget target)
        {
            var totalScale = sprite.BaseScale * currentScale;
            var targetAttackLocation = target.MeleeAttackLocation;
            targetAttackLocation.x += totalScale.x / 2;
            targetAttackLocation.y = totalScale.y / 2.0f;
            return targetAttackLocation;
        }

        private void UseSkill(IBattleTarget target, ISkill skill)
        {
            if (target == null)
            {
                target = this;
            }
            switch (skill.AttackStyle)
            {
                case SkillAttackStyle.Cast:
                    Cast(target, skill);
                    break;
                case SkillAttackStyle.Melee:
                    Melee(target, skill, false, true);
                    break;
            }
        }

        private void Cast(IBattleTarget target, ISkill skill)
        {
            castEffect?.RequestDestruction();
            castEffect = objectResolver.Resolve<Attachment<BattleScene>, IAttachment.Description>(o =>
            {
                ISpriteAsset asset = new Assets.PixelEffects.Nebula();
                o.RenderShadow = false;
                o.Sprite = asset.CreateSprite();
                o.SpriteMaterial = asset.CreateMaterial();
                o.Light = new Light
                {
                    Color = skill.CastColor,
                    Length = 2.3f,
                };
                o.LightOffset = new Vector3(0, 0, -0.1f);
            });

            var swingEnd = Quaternion.Identity;
            var swingStart = new Quaternion(0f, MathF.PI / 2.1f, 0f);

            long remainingTime = (long)(1.8f * Clock.SecondsToMicro);
            long standTime = (long)(0.2f * Clock.SecondsToMicro);
            long standStartTime = remainingTime / 2;
            long swingTime = standStartTime - standTime / 3;
            long standEndTime = standStartTime - standTime;
            bool needsAttack = true;
            bool createSkillCastEffect = true;
            ISkillEffect skillEffect = null;
            battleManager.DeactivateCurrentPlayer();
            var triggerManager = new ContextTriggerManager();
            battleManager.QueueTurn(c =>
            {
                if (IsDead)
                {
                    DestroyCastEffect();
                    return true;
                }

                if (createSkillCastEffect)
                {
                    createSkillCastEffect = false;
                    castEffect?.RequestDestruction();
                    castEffect = objectResolver.Resolve<Attachment<BattleScene>, IAttachment.Description>(o =>
                    {
                        ISpriteAsset asset = skill.CastSpriteAsset;
                        o.RenderShadow = false;
                        o.Sprite = asset.CreateSprite();
                        o.SpriteMaterial = asset.CreateMaterial();
                        o.Light = new Light
                        {
                            Color = skill.CastColor,
                            Length = 2.3f,
                        };
                        o.LightOffset = new Vector3(0, 0, -0.1f);
                    });
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
                    sprite.SetAnimation("cast-left");
                    if (!battleManager.IsStillValidTarget(target))
                    {
                        target = battleManager.ValidateTarget(this, target);
                    }
                    start = this.startPosition;
                    end = target.MeleeAttackLocation;
                    interpolate = (remainingTime - standStartTime) / (float)standStartTime;
                    triggerManager.CheckTrigger(this, true);
                }
                else if (remainingTime > standEndTime)
                {
                    sprite.SetAnimation("cast-left");
                    interpolate = 0.0f;
                    start = target.MeleeAttackLocation;
                    end = target.MeleeAttackLocation;

                    if (needsAttack && remainingTime < swingTime)
                    {
                        needsAttack = false;
                        DestroyCastEffect();

                        if (characterSheet.CurrentMp == 0)
                        {
                            battleManager.AddDamageNumber(this, "No MP", Color.Red);
                        }
                        else
                        {
                            var mpCost = skill.GetMpCost(triggerManager.Activated, triggerManager.Spammed);
                            skillEffect = skill.Apply(battleManager, objectResolver, coroutine, this, target, triggerManager.Activated, triggerManager.Spammed);
                            TakeMp(mpCost);
                        }
                    }
                }
                else
                {
                    sprite.SetAnimation(victorious ? "victory" : "stand-left");

                    mainHandItem?.SetAdditionalRotation(Quaternion.Identity);

                    start = target.MeleeAttackLocation;
                    end = this.startPosition;
                    interpolate = remainingTime / (float)standEndTime;
                }

                var position = end.lerp(start, interpolate);

                if (remainingTime < 0)
                {
                    position = end;
                    sprite.SetAnimation(victorious ? "victory" : "stand-left");
                    TurnComplete();
                    done = true;
                }

                Sprite_FrameChanged(sprite);

                if (castEffect != null)
                {
                    var scale = sprite.BaseScale * this.currentScale;
                    if (triggerManager.Spammed)
                    {
                        scale *= 0.72f;
                    }
                    else if (triggerManager.Activated)
                    {
                        scale *= 1.63f;
                    }
                    castEffect.SetWorldPosition(position, this.currentOrientation, castEffect.BaseScale * scale);
                }

                return done;
            }, skill.QueueFront || characterSheet.QueueTurnsFront);
        }

        private readonly Quaternion swingEnd = Quaternion.Identity;
        private readonly Quaternion swingStart = new Quaternion(0f, MathF.PI / 2.1f, 0f);

        private void Melee(IBattleTarget target, ISkill skill, bool queueFront, bool deactivatePlayer)
        {
            long remainingTime = (long)(1.8f * Clock.SecondsToMicro);
            long standTime = (long)(0.2f * Clock.SecondsToMicro);
            long standStartTime = remainingTime / 2;
            long swingTime = standStartTime - standTime / 3;
            long standEndTime = standStartTime - standTime;
            bool needsAttack = true;
            queueFront = queueFront || characterSheet.QueueTurnsFront;
            ISkillEffect skillEffect = null;
            if (deactivatePlayer)
            {
                battleManager.DeactivateCurrentPlayer();
            }
            var triggerManager = new ContextTriggerManager();
            battleManager.QueueTurn(c =>
            {
                if (IsDead)
                {
                    return true;
                }

                Vector3 attackStartPosition;
                if (guiActive)
                {
                    attackStartPosition = ActivePosition;
                }
                else
                {
                    attackStartPosition = this.startPosition;
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
                    sprite.SetAnimation("left");
                    target = battleManager.ValidateTarget(this, target);
                    start = attackStartPosition;
                    end = GetAttackLocation(target);
                    interpolate = (remainingTime - standStartTime) / (float)standStartTime;
                    triggerManager.CheckTrigger(this, true);
                }
                else if (remainingTime > standEndTime)
                {
                    sprite.SetAnimation("stand-left");
                    interpolate = 0.0f;
                    start = end = GetAttackLocation(target);

                    var slerpAmount = (remainingTime - standEndTime) / (float)standEndTime;
                    mainHandItem?.SetAdditionalRotation(swingStart.slerp(swingEnd, slerpAmount));

                    if (this.Stats.CanTriggerAttack && triggerManager.Activated && remainingTime > swingTime)
                    {
                        offHandItem?.SetAdditionalRotation(swingStart.slerp(swingEnd, slerpAmount));
                    }

                    if (triggerManager.Spammed)
                    {
                        offHandItem?.SetAdditionalRotation(Quaternion.Identity);
                    }

                    if (needsAttack && remainingTime < swingTime)
                    {
                        needsAttack = false;

                        if (characterSheet.CurrentMp < skill.GetMpCost(triggerManager.Activated, triggerManager.Spammed))
                        {
                            battleManager.AddDamageNumber(this, "Not Enough MP", Color.Red);
                        }
                        else
                        {
                            TakeMp(skill.GetMpCost(triggerManager.Activated, triggerManager.Spammed));
                            skillEffect = skill.Apply(battleManager, objectResolver, coroutine, this, target, triggerManager.Activated, triggerManager.Spammed);
                        }
                    }
                }
                else
                {
                    sprite.SetAnimation("right");

                    mainHandItem?.SetAdditionalRotation(Quaternion.Identity);
                    offHandItem?.SetAdditionalRotation(Quaternion.Identity);

                    start = GetAttackLocation(target);
                    end = attackStartPosition;
                    interpolate = remainingTime / (float)standEndTime;
                }

                this.currentPosition = end.lerp(start, interpolate);

                if (remainingTime < 0)
                {
                    this.currentPosition = end;
                    sprite.SetAnimation(victorious ? "victory" : "stand-left");
                    if (deactivatePlayer)
                    {
                        TurnComplete();
                    }
                    done = true;
                }

                Sprite_FrameChanged(sprite);

                return done;
            }, queueFront);
        }

        private void UseItem(IBattleTarget target, InventoryItem item)
        {
            castEffect?.RequestDestruction();
            castEffect = objectResolver.Resolve<Attachment<BattleScene>, IAttachment.Description>(o =>
            {
                ISpriteAsset asset = new Assets.PixelEffects.Nebula();
                o.RenderShadow = false;
                o.Sprite = asset.CreateSprite();
                o.SpriteMaterial = asset.CreateMaterial();
            });

            var action = inventoryFunctions.CreateAction(item, inventory);
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
                    DestroyCastEffect();
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
            }, characterSheet.QueueTurnsFront);
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

        private void Sprite_AnimationChanged(ISprite obj)
        {
            if (mainHandHand != null)
            {
                switch (primaryHand)
                {
                    case Player.RightHand:
                        mainHandHand.SetAnimation(obj.CurrentAnimationName + "-r-hand");
                        break;
                    case Player.LeftHand:
                        mainHandHand.SetAnimation(obj.CurrentAnimationName + "-l-hand");
                        break;
                }
            }

            if (offHandHand != null)
            {
                switch (secondaryHand)
                {
                    case Player.RightHand:
                        offHandHand.SetAnimation(obj.CurrentAnimationName + "-r-hand");
                        break;
                    case Player.LeftHand:
                        offHandHand.SetAnimation(obj.CurrentAnimationName + "-l-hand");
                        break;
                }
            }

            mainHandItem?.SetAnimation(obj.CurrentAnimationName);
            offHandItem?.SetAnimation(obj.CurrentAnimationName);
        }

        private void Sprite_FrameChanged(ISprite obj)
        {
            var frame = obj.GetCurrentFrame();

            Vector3 offset;
            var scale = sprite.BaseScale * this.currentScale;
            this.tlasData.Transform = new InstanceMatrix(this.currentPosition, this.currentOrientation, scale);

            var primaryAttach = frame.Attachments[this.primaryHand];
            offset = scale * primaryAttach.translate;
            offset = Quaternion.quatRotate(this.currentOrientation, offset) + this.currentPosition;
            mainHandItem?.SetPosition(offset, this.currentOrientation, scale);
            mainHandHand?.SetPosition(offset, this.currentOrientation, scale);

            var secondaryAttach = frame.Attachments[this.secondaryHand];
            offset = secondaryAttach.translate;
            if (offHandRaised)
            {
                offset += new Vector3(0f, 0.11f, 0f);
            }
            offset = offset * scale;
            offset = Quaternion.quatRotate(this.currentOrientation, offset) + this.currentPosition;
            offHandItem?.SetPosition(offset, this.currentOrientation, scale);
            offHandHand?.SetPosition(offset, this.currentOrientation, scale);

            if (castEffect != null)
            {
                var castAttach = frame.Attachments[this.secondaryHand];
                offset = scale * castAttach.translate;
                offset = Quaternion.quatRotate(this.currentOrientation, offset) + this.currentPosition;
                offset += new Vector3(0f, 0f, -0.03f);
                castEffect.SetPosition(offset, this.currentOrientation, scale);
            }
        }

        public void ApplyDamage(IBattleTarget attacker, IDamageCalculator calculator, long damage)
        {
            if (IsDead) { return; } //Do nothing if dead

            characterSheet.CurrentHp = calculator.ApplyDamage(damage, characterSheet.CurrentHp, characterSheet.Hp);
            currentHp.UpdateText(GetCurrentHpText());
            currentHp.Color = GetCurrentHpTextColor();
            if (attacker.BattleTargetType == BattleTargetType.Enemy)
            {
                characterSheet.PowerGauge += calculator.PowerGaugeGain(characterSheet, damage);
            }

            //Player died from applied damage
            if (IsDead)
            {
                characterSheet.PowerGauge = 0;
                battleManager.PlayerDead(this);
                sprite.SetAnimation("dead");
                Sprite_FrameChanged(sprite);
                characterTimer.TurnTimerActive = false;
                characterTimer.Reset();
            }
        }

        public void AttemptMeleeCounter(IBattleTarget attacker)
        {
            long standTime = (long)(0.2f * Clock.SecondsToMicro);
            long remainingTime = standTime + (long)(0.187f * Clock.SecondsToMicro);
            long standStartTime = standTime;
            long swingTime = standStartTime - standTime / 3;
            long standEndTime = standStartTime - standTime;
            bool needsAttack = true;
            ISkill skill = counterAttack;
            ISkillEffect skillEffect = null;

            attacker.SetCounterAttack((c, t) =>
            {
                var finished = false;

                remainingTime -= c.DeltaTimeMicro;

                if (remainingTime < standStartTime)
                {
                    if (remainingTime > standEndTime)
                    {
                        //If there is an effect, just let it run
                        if (skillEffect != null && !skillEffect.Finished)
                        {
                            skillEffect.Update(c);
                            return false;
                        }

                        sprite.SetAnimation("stand-left");

                        var slerpAmount = remainingTime / (float)standTime;
                        mainHandItem?.SetAdditionalRotation(swingStart.slerp(swingEnd, slerpAmount));

                        if (needsAttack && remainingTime < swingTime)
                        {
                            needsAttack = false;
                            skillEffect = skill.Apply(battleManager, objectResolver, coroutine, this, t, false, false);
                        }
                    }
                    else
                    {
                        mainHandItem?.SetAdditionalRotation(swingEnd);
                        finished = true;
                    }

                    Sprite_FrameChanged(sprite);
                }

                return finished;
            });
        }

        public void Resurrect(IDamageCalculator calculator, long damage)
        {
            if (damage < 0 && !IsDead) { return; } //Don't do anything if healing and not dead

            characterSheet.CurrentHp = calculator.ApplyDamage(damage, characterSheet.CurrentHp, characterSheet.Hp);
            currentHp.UpdateText(GetCurrentHpText());
            currentHp.Color = GetCurrentHpTextColor();

            //Player died from applied damage
            if (IsDead)
            {
                battleManager.PlayerDead(this);
                sprite.SetAnimation("dead");
                Sprite_FrameChanged(sprite);
                characterTimer.TurnTimerActive = false;
            }
            else
            {
                characterTimer.TurnTimerActive = true;
                sprite.SetAnimation("stand-left");
                Sprite_FrameChanged(sprite);
            }

            characterTimer.Reset();
        }

        public void TakeMp(long mp)
        {
            if (IsDead) { return; }

            characterSheet.CurrentMp -= mp;
            if (characterSheet.CurrentMp < 0)
            {
                characterSheet.CurrentMp = 0;
            }
            else if (characterSheet.CurrentMp > characterSheet.Mp)
            {
                characterSheet.CurrentMp = characterSheet.Mp;
            }
            currentMp.UpdateText(GetCurrentMpText());
            currentMp.Color = GetCurrentMpTextColor();
        }

        public void MoveToGuard(in Vector3 position)
        {
            this.currentPosition = position;
            this.currentPosition.z += -0.03f;
            sprite.SetAnimation("stand-left");
            Sprite_FrameChanged(sprite);
        }

        public void MoveToStart()
        {
            if (IsDead)
            {
                this.currentPosition = this.startPosition;
                sprite.SetAnimation("dead");
            }
            else
            {
                if (battleManager.GetActivePlayer() == this)
                {
                    this.currentPosition = this.ActivePosition;
                    sprite.SetAnimation("stand-down");
                }
                else
                {
                    this.currentPosition = this.startPosition;
                    sprite.SetAnimation("stand-left");
                }
            }
            Sprite_FrameChanged(sprite);
        }

        private void Bind(IShaderBindingTable sbt, ITopLevelAS tlas)
        {
            spriteInstance.Bind(this.tlasData.InstanceName, sbt, tlas, sprite);
        }

        private void OnMainHandModified(CharacterSheet obj)
        {
            mainHandItem?.RequestDestruction();
            mainHandItem = null;
            if (characterSheet.MainHand?.Sprite != null)
            {
                mainHandItem = objectResolver.Resolve<Attachment<BattleScene>, IAttachment.Description>(o =>
                {
                    var asset = assetFactory.CreateEquipment(characterSheet.MainHand.Sprite);
                    o.Orientation = asset.GetOrientation();
                    o.Sprite = asset.CreateSprite();
                    o.SpriteMaterial = asset.CreateMaterial();
                    o.Light = asset.CreateLight();
                    o.LightAttachmentChannel = asset.LightAttachmentChannel;
                });
            }

            if (characterSheet.MainHand?.ShowHand != false)
            {
                if (mainHandHand == null)
                {
                    mainHandHand = objectResolver.Resolve<Attachment<BattleScene>, IAttachment.Description>(o =>
                    {
                        o.Sprite = new Sprite(playerSpriteInfo.Animations)
                        {
                            BaseScale = new Vector3(0.1875f, 0.1875f, 1.0f)
                        };
                        o.SpriteMaterial = playerSpriteInfo.Tier1;
                    });
                }
            }
            else if (mainHandHand != null)
            {
                mainHandHand.RequestDestruction();
                mainHandHand = null;
            }
            Sprite_AnimationChanged(sprite);
            Sprite_FrameChanged(sprite);
            UpdateSkills();
        }

        private void OnOffHandModified(CharacterSheet obj)
        {
            offHandItem?.RequestDestruction();
            offHandItem = null;
            if (characterSheet.OffHand?.Sprite != null)
            {
                offHandItem = objectResolver.Resolve<Attachment<BattleScene>, IAttachment.Description>(o =>
                {
                    var asset = assetFactory.CreateEquipment(characterSheet.OffHand.Sprite);
                    o.Orientation = asset.GetOrientation();
                    o.Sprite = asset.CreateSprite();
                    o.SpriteMaterial = asset.CreateMaterial();
                    o.Light = asset.CreateLight();
                    o.LightAttachmentChannel = asset.LightAttachmentChannel;
                });
            }

            if (characterSheet.OffHand?.ShowHand != false)
            {
                if (offHandHand == null)
                {
                    offHandHand = objectResolver.Resolve<Attachment<BattleScene>, IAttachment.Description>(o =>
                    {
                        o.Sprite = new Sprite(playerSpriteInfo.Animations)
                        {
                            BaseScale = new Vector3(0.1875f, 0.1875f, 1.0f)
                        };
                        o.SpriteMaterial = playerSpriteInfo.Tier1;
                    });
                }
            }
            else if (offHandHand != null)
            {
                offHandHand.RequestDestruction();
                offHandHand = null;
            }
            Sprite_AnimationChanged(sprite);
            Sprite_FrameChanged(sprite);
            UpdateSkills();
        }

        private void UpdateSkills()
        {
            this.skills.Clear();
            this.skills.AddRange(characterSheet.Skills.Select(i => skillFactory.CreateSkill(i)));
        }

        public bool TryContextTrigger()
        {
            return (contextTriggerJoystick.FirstFrameDown || contextTriggerKeyboard.FirstFrameDown);
        }

        public void SetVictorious()
        {
            victorious = true;
            if(!IsDead)
            {
                sprite.SetAnimation("victory");
                this.currentPosition = this.startPosition;
            }
            Sprite_FrameChanged(sprite);
        }

        private void CharacterSheet_OnBodyModified(CharacterSheet obj)
        {
            coroutine.RunTask(SwapSprites());
        }

        private void CharacterSheet_OnBuffsModified(CharacterSheet obj)
        {
            UpdateCurrentBuffs();
        }

        public void SetCounterAttack(Func<Clock, IBattleTarget, bool> counter)
        {
            //Does nothing
        }

        public void SetShakePosition(bool shook)
        {
            MoveToStart();
            if (shook && !IsDead)
            {
                this.currentPosition += new Vector3(0.065f, 0.0f, 0.0f);
                this.tlasData.Transform = new InstanceMatrix(this.currentPosition, this.currentOrientation, this.currentScale);
            }
            //Sprite_FrameChanged(sprite); //Intentionally not calling this, since the visual of the base sprite shaking separately from the weapons looks cool
        }

        private async Task SwapSprites()
        {
            using var destructionBlock = destructionRequest.BlockDestruction();

            var loadingTier = characterSheet.EquipmentTier;
            var newSprite = await spriteInstanceFactory.Checkout(playerSpriteInfo.GetTier(loadingTier), sprite);

            if (this.disposed || loadingTier != characterSheet.EquipmentTier)
            {
                this.spriteInstanceFactory.TryReturn(newSprite);
                return; //Stop loading
            }

            rtInstances.RemoveSprite(sprite);
            this.spriteInstanceFactory.TryReturn(spriteInstance);
            this.spriteInstance = newSprite;
            rtInstances.AddSprite(sprite, tlasData, spriteInstance);
        }
    }
}
