using Adventure.Services;
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
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Battle
{
    class TargetCursor : IDisposable
    {
        private readonly IDestructionRequest destructionRequest;
        private readonly RTInstances<BattleScene> rtInstances;
        private readonly SpriteInstanceFactory spriteInstanceFactory;
        private readonly ISharpGui sharpGui;
        private readonly IBattleScreenLayout battleScreenLayout;
        private readonly ICameraProjector cameraProjector;
        private readonly IScaleHelper scaleHelper;
        private readonly KeybindService keybindService;
        private readonly Sprite sprite;
        private readonly TLASInstanceData tlasData;
        private SpriteInstance spriteInstance;
        private bool disposed;
        private SharpPanel panel = new SharpPanel();
        private SharpStyle panelStyle = new SharpStyle() { Background = Color.FromARGB(0xbb020202) };

        private SharpButton nextTargetButton = new SharpButton() { Text = "Next" };
        private SharpButton previousTargetButton = new SharpButton() { Text = "Previous" };
        private SharpButton selectTargetButton = new SharpButton() { Text = "Select" };
        private SharpButton rowButton = new SharpButton() { Text = "Row" };
        private SharpButton cancelButton = new SharpButton() { Text = "Cancel" };

        public uint EnemyTargetIndex { get; set; }
        public uint PlayerTargetIndex { get; set; }

        private bool __targetPlayers;
        public bool TargetPlayers
        {
            get
            {
                return __targetPlayers;
            }
            private set
            {
                __targetPlayers = value;
                if (__targetPlayers)
                {
                    sprite.SetAnimation("reverse");
                }
                else
                {
                    sprite.SetAnimation("default");
                }
            }
        }

        public bool Targeting => getTargetTask != null;

        TaskCompletionSource<IBattleTarget> getTargetTask;

        private static readonly Dictionary<string, SpriteAnimation> animations = new Dictionary<string, SpriteAnimation>()
        {
            { "default", new SpriteAnimation(1,
                new SpriteFrame(0, 0, 1, 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(0, 9, -0.01f, 32, 32),
                    }
                } )
            },
            { "reverse", new SpriteAnimation(1,
                new SpriteFrame(1, 0, 0, 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        SpriteFrameAttachment.FromFramePosition(31, 9, -0.01f, 32, 32),
                    }
                } )
            }
        };

        private SharpImage fire = new SharpImage();
        private SharpImage ice = new SharpImage();
        private SharpImage electricity = new SharpImage();
        private SharpImage slashing = new SharpImage();
        private SharpImage bludgeoning = new SharpImage();
        private SharpImage piercing = new SharpImage();

        IBattleTarget resistanceTextTarget;
        private ILayoutItem targetRootLayout;
        private ColumnLayout targetResistenceLayout = new ColumnLayout();
        private List<SharpImage> targetResistenceItems = new List<SharpImage>(8);

        private KeyboardButtonCode AcceptKey;
        private KeyboardButtonCode CancelKey;
        private KeyboardButtonCode UpKey;
        private KeyboardButtonCode DownKey;
        private KeyboardButtonCode LeftKey;
        private KeyboardButtonCode RightKey;
        private KeyboardButtonCode SwitchKey;

        private GamepadButtonCode[] AcceptButton;
        private GamepadButtonCode[] CancelButton;
        private GamepadButtonCode[] UpButton;
        private GamepadButtonCode[] DownButton;
        private GamepadButtonCode[] LeftButton;
        private GamepadButtonCode[] RightButton;
        private GamepadButtonCode[] SwitchButton;

        public TargetCursor
        (
            IDestructionRequest destructionRequest,
            RTInstances<BattleScene> rtInstances,
            SpriteInstanceFactory spriteInstanceFactory,
            IScopedCoroutine coroutine,
            ISharpGui sharpGui,
            IBattleScreenLayout battleScreenLayout,
            ICameraProjector cameraProjector,
            IScaleHelper scaleHelper,
            IconLoader iconLoader,
            KeybindService keybindService
        )
        {
            this.destructionRequest = destructionRequest;
            this.rtInstances = rtInstances;
            this.spriteInstanceFactory = spriteInstanceFactory;
            this.sharpGui = sharpGui;
            this.battleScreenLayout = battleScreenLayout;
            this.cameraProjector = cameraProjector;
            this.scaleHelper = scaleHelper;
            this.keybindService = keybindService;
            this.sprite = new Sprite(animations)
            { BaseScale = new Vector3(0.5f, 0.5f, 1f) };

            fire.Image = iconLoader.Icons;
            fire.UvRect = iconLoader.Fire;
            fire.DesiredWidth = scaleHelper.Scaled(48);
            ice.Image = iconLoader.Icons;
            ice.UvRect = iconLoader.Ice;
            ice.DesiredWidth = scaleHelper.Scaled(48);
            electricity.Image = iconLoader.Icons;
            electricity.UvRect = iconLoader.Electricity;
            electricity.DesiredWidth = scaleHelper.Scaled(48);
            slashing.Image = iconLoader.Icons;
            slashing.UvRect = iconLoader.Slashing;
            slashing.DesiredWidth = scaleHelper.Scaled(48);
            bludgeoning.Image = iconLoader.Icons;
            bludgeoning.UvRect = iconLoader.Bludgeoning;
            bludgeoning.DesiredWidth = scaleHelper.Scaled(48);
            piercing.Image = iconLoader.Icons;
            piercing.UvRect = iconLoader.Piercing;
            piercing.DesiredWidth = scaleHelper.Scaled(48);

            targetRootLayout = new PanelLayoutNoPad(panel, new MarginLayout(new IntPad(scaleHelper.Scaled(5)), targetResistenceLayout));
            targetResistenceLayout.Margin = new IntPad(scaleHelper.Scaled(5));

            this.tlasData = new TLASInstanceData()
            {
                InstanceName = RTId.CreateId("TargetCursor"),
                Mask = RtStructures.OPAQUE_GEOM_MASK,
                Transform = new InstanceMatrix(Vector3.Zero, Quaternion.Identity, sprite.BaseScale)
            };

            AcceptKey = keybindService.GetKeyboardMouseBinding(KeyBindings.Confirm).KeyboardButton.Value;
            CancelKey = keybindService.GetKeyboardMouseBinding(KeyBindings.Cancel).KeyboardButton.Value;
            UpKey = keybindService.GetKeyboardMouseBinding(KeyBindings.Up).KeyboardButton.Value;
            DownKey = keybindService.GetKeyboardMouseBinding(KeyBindings.Down).KeyboardButton.Value;
            LeftKey = keybindService.GetKeyboardMouseBinding(KeyBindings.Left).KeyboardButton.Value;
            RightKey = keybindService.GetKeyboardMouseBinding(KeyBindings.Right).KeyboardButton.Value;
            SwitchKey = keybindService.GetKeyboardMouseBinding(KeyBindings.SwitchCharacter).KeyboardButton.Value;

            AcceptButton = keybindService.GetAllGamepadBindings(KeyBindings.Confirm);
            CancelButton = keybindService.GetAllGamepadBindings(KeyBindings.Cancel);
            UpButton = keybindService.GetAllGamepadBindings(KeyBindings.Up);
            DownButton = keybindService.GetAllGamepadBindings(KeyBindings.Down);
            LeftButton = keybindService.GetAllGamepadBindings(KeyBindings.Left);
            RightButton = keybindService.GetAllGamepadBindings(KeyBindings.Right);
            SwitchButton = keybindService.GetAllGamepadBindings(KeyBindings.SwitchCharacter);

            keybindService.KeybindChanged += KeybindService_KeybindChanged;

            coroutine.RunTask(async () =>
            {
                using var destructionBlock = destructionRequest.BlockDestruction(); //Block destruction until coroutine is finished and this is disposed.

                var matDesc = new SpriteMaterialDescription(
                    colorMap: "Graphics/Sprites/Crawl/UI/pointingfinger.png",
                    materials: new HashSet<SpriteMaterialTextureItem>());

                this.spriteInstance = await spriteInstanceFactory.Checkout(matDesc, sprite);

                if (disposed)
                {
                    spriteInstanceFactory.TryReturn(spriteInstance);
                    return; //Stop loading
                }

                if (visible)
                {
                    AddToScene();
                }
            });
        }

        public void Dispose()
        {
            keybindService.KeybindChanged -= KeybindService_KeybindChanged;
            disposed = true;
            this.spriteInstanceFactory.TryReturn(spriteInstance);
            RemoveFromScene();
        }

        private bool visible = true;
        public bool Visible
        {
            get
            {
                return visible;
            }
            set
            {
                if (visible != value)
                {
                    visible = value;
                    if (visible)
                    {
                        AddToScene();
                    }
                    else
                    {
                        RemoveFromScene();
                    }
                }
            }
        }

        private void RemoveFromScene()
        {
            rtInstances.RemoveSprite(sprite);
            rtInstances.RemoveShaderTableBinder(Bind);
            rtInstances.RemoveTlasBuild(tlasData);
        }

        private void AddToScene()
        {
            rtInstances.AddTlasBuild(tlasData);
            rtInstances.AddShaderTableBinder(Bind);
            rtInstances.AddSprite(sprite, tlasData, spriteInstance);
        }

        internal void BattleStarted()
        {
            EnemyTargetIndex = 0;
            PlayerTargetIndex = 0;
        }

        private Vector3 currentPosition;

        public void SetPosition(Vector3 targetPosition)
        {
            currentPosition = targetPosition - sprite.GetCurrentFrame().Attachments[0].translate * sprite.BaseScale;
            this.tlasData.Transform = new InstanceMatrix(currentPosition, sprite.BaseScale);
        }

        public Task<IBattleTarget> GetTarget(bool targetPlayers)
        {
            TargetPlayers = targetPlayers;
            getTargetTask = new TaskCompletionSource<IBattleTarget>();
            return getTargetTask.Task;
        }

        public void Cancel()
        {
            if (getTargetTask != null)
            {
                SetTarget(null);
            }
        }

        private SharpImage GetElementImage(Element element)
        {
            switch (element)
            {
                case Element.Fire:
                    return fire;
                case Element.Ice:
                    return ice;
                case Element.Electricity:
                    return electricity;
                case Element.Slashing:
                    return slashing;
                case Element.Bludgeoning:
                    return bludgeoning;
                case Element.Piercing:
                    return piercing;
                default:
                    return null;
            }
        }

        public void UpdateCursor(IBattleManager battleManager, IBattleTarget target, Vector3 enemyPos, BattlePlayer activePlayer)
        {
            SetPosition(enemyPos);

            battleScreenLayout.LayoutBattleMenu(cancelButton, selectTargetButton, nextTargetButton, previousTargetButton, rowButton);

            if (activePlayer.Stats.CanSeeEnemyInfo)
            {
                if (resistanceTextTarget != target)
                {
                    var strongResist = new RowLayout() { Margin = new IntPad(scaleHelper.Scaled(5)) };
                    var weakResist = new RowLayout() { Margin = new IntPad(scaleHelper.Scaled(5)) };
                    var absorbResist = new RowLayout() { Margin = new IntPad(scaleHelper.Scaled(5)) };
                    targetResistenceItems.Clear();
                    targetResistenceLayout.Clear();

                    resistanceTextTarget = target;
                    var resistances = new StringBuilder();
                    foreach (var resistance in target.Stats.Resistances.OrderBy(i => i.Value))
                    {
                        var image = GetElementImage(resistance.Key);
                        if (image != null)
                        {
                            targetResistenceItems.Add(image);
                            switch (resistance.Value)
                            {
                                case Resistance.Resist:
                                    strongResist.Add(image);
                                    image.Color = Color.UIRed;
                                    break;
                                case Resistance.Weak:
                                    weakResist.Add(image);
                                    image.Color = Color.UIGreen;
                                    break;
                                case Resistance.Absorb:
                                    absorbResist.Add(image);
                                    image.Color = Color.UIPurple;
                                    break;
                            }
                        }
                    }

                    if (weakResist.HasItems)
                    {
                        targetResistenceLayout.Add(weakResist);
                    }
                    if (strongResist.HasItems)
                    {
                        targetResistenceLayout.Add(strongResist);
                    }
                    if (absorbResist.HasItems)
                    {
                        targetResistenceLayout.Add(absorbResist);
                    }
                }

                var cursorOffset = currentPosition;
                cursorOffset.y -= sprite.BaseScale.y / 2.0f;
                var resistanceLoc = cameraProjector.Project(cursorOffset);

                var layoutSize = targetRootLayout.GetDesiredSize(sharpGui);
                targetRootLayout.SetRect(new IntRect((int)resistanceLoc.x, (int)resistanceLoc.y, layoutSize.Width, layoutSize.Height));

                sharpGui.Panel(panel, panelStyle);
                foreach (var image in targetResistenceItems)
                {
                    sharpGui.Image(image);
                }
            }

            var handled = false;
            if (sharpGui.ShowHover && sharpGui.Button(selectTargetButton, activePlayer.GamepadId, style: activePlayer.UiStyle))
            {
                SetTarget(target);
                handled = true;
            }
            if (sharpGui.ShowHover && sharpGui.Button(nextTargetButton, activePlayer.GamepadId, style: activePlayer.UiStyle))
            {
                NextTarget();
                handled = true;
            }
            if (sharpGui.ShowHover && sharpGui.Button(previousTargetButton, activePlayer.GamepadId, style: activePlayer.UiStyle))
            {
                PreviousTarget();
                handled = true;
            }
            if (sharpGui.ShowHover && sharpGui.Button(rowButton, activePlayer.GamepadId, style: activePlayer.UiStyle))
            {
                ChangeRow();
                handled = true;
            }
            if (sharpGui.ShowHover && sharpGui.Button(cancelButton, activePlayer.GamepadId, style: activePlayer.UiStyle))
            {
                SetTarget(null);
                handled = true;
            }

            if (!handled)
            {
                var currentGamepad = (int)activePlayer.GamepadId;
                var gamepadButton = sharpGui.GamepadButtonEntered[currentGamepad];
                if (gamepadButton == AcceptButton[currentGamepad])
                {
                    SetTarget(target);
                }
                else if (gamepadButton == CancelButton[currentGamepad])
                {
                    SetTarget(null);
                }
                else if (gamepadButton == UpButton[currentGamepad])
                {
                    NextTarget();
                }
                else if (gamepadButton == DownButton[currentGamepad])
                {
                    PreviousTarget();
                }
                else if (gamepadButton == LeftButton[currentGamepad] || gamepadButton == RightButton[currentGamepad])
                {
                    ChangeRow();
                }
                else if (gamepadButton == SwitchButton[currentGamepad])
                {
                    battleManager.SwitchPlayer();
                    SetTarget(null);
                }
                else
                {
                    //Handle keyboard
                    var keyboardKey = sharpGui.KeyEntered;
                    if (keyboardKey == AcceptKey)
                    {
                        SetTarget(target);
                    }
                    else if (keyboardKey == CancelKey)
                    {
                        SetTarget(null);
                    }
                    else if (keyboardKey == UpKey)
                    {
                        NextTarget();
                    }
                    else if (keyboardKey == DownKey)
                    {
                        PreviousTarget();
                    }
                    else if (keyboardKey == LeftKey || keyboardKey == RightKey)
                    {
                        ChangeRow();
                    }
                    else if(keyboardKey == SwitchKey)
                    {
                        battleManager.SwitchPlayer();
                        SetTarget(null);
                    }
                }
            }
        }

        private void ChangeRow()
        {
            TargetPlayers = !TargetPlayers;
        }

        private void PreviousTarget()
        {
            if (TargetPlayers)
            {
                ++PlayerTargetIndex;
            }
            else
            {
                --EnemyTargetIndex;
            }
        }

        private void NextTarget()
        {
            if (TargetPlayers)
            {
                --PlayerTargetIndex;
            }
            else
            {
                ++EnemyTargetIndex;
            }
        }

        private void SetTarget(IBattleTarget enemy)
        {
            resistanceTextTarget = null;
            if (getTargetTask != null)
            {
                var refGetTargetTask = getTargetTask;
                getTargetTask = null;
                refGetTargetTask.SetResult(enemy);
            }
        }

        private void Bind(IShaderBindingTable sbt, ITopLevelAS tlas)
        {
            spriteInstance.Bind(this.tlasData.InstanceName, sbt, tlas, sprite);
        }

        private void KeybindService_KeybindChanged(KeybindService service, KeyBindings keyBinding)
        {
            switch (keyBinding)
            {
                case KeyBindings.Confirm:
                    AcceptKey = keybindService.GetKeyboardMouseBinding(KeyBindings.Confirm).KeyboardButton.Value;
                    AcceptButton = keybindService.GetAllGamepadBindings(KeyBindings.Confirm);
                    break;
                case KeyBindings.Cancel:
                    CancelKey = keybindService.GetKeyboardMouseBinding(KeyBindings.Cancel).KeyboardButton.Value;
                    CancelButton = keybindService.GetAllGamepadBindings(KeyBindings.Cancel);
                    break;
                case KeyBindings.Up:
                    UpKey = keybindService.GetKeyboardMouseBinding(KeyBindings.Up).KeyboardButton.Value;
                    UpButton = keybindService.GetAllGamepadBindings(KeyBindings.Up);
                    break;
                case KeyBindings.Down:
                    DownKey = keybindService.GetKeyboardMouseBinding(KeyBindings.Down).KeyboardButton.Value;
                    DownButton = keybindService.GetAllGamepadBindings(KeyBindings.Down);
                    break;
                case KeyBindings.Left:
                    LeftKey = keybindService.GetKeyboardMouseBinding(KeyBindings.Left).KeyboardButton.Value;
                    LeftButton = keybindService.GetAllGamepadBindings(KeyBindings.Left);
                    break;
                case KeyBindings.Right:
                    RightKey = keybindService.GetKeyboardMouseBinding(KeyBindings.Right).KeyboardButton.Value;
                    RightButton = keybindService.GetAllGamepadBindings(KeyBindings.Right);
                    break;
                case KeyBindings.SwitchCharacter:
                    SwitchKey = keybindService.GetKeyboardMouseBinding(KeyBindings.SwitchCharacter).KeyboardButton.Value;
                    SwitchButton = keybindService.GetAllGamepadBindings(KeyBindings.SwitchCharacter);
                    break;
            }
        }
    }
}
