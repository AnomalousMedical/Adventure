﻿using DiligentEngine;
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
        private readonly Sprite sprite;
        private readonly TLASInstanceData tlasData;
        private SpriteInstance spriteInstance;
        private bool disposed;

        private SharpButton nextTargetButton = new SharpButton() { Text = "Next" };
        private SharpButton previousTargetButton = new SharpButton() { Text = "Previous" };
        private SharpButton selectTargetButton = new SharpButton() { Text = "Select" };
        private SharpText statText = new SharpText() { Color = Color.White };

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

        public TargetCursor
        (
            IDestructionRequest destructionRequest,
            RTInstances<BattleScene> rtInstances,
            SpriteInstanceFactory spriteInstanceFactory,
            IScopedCoroutine coroutine,
            ISharpGui sharpGui,
            IBattleScreenLayout battleScreenLayout,
            ICameraProjector cameraProjector,
            IScaleHelper scaleHelper
        )
        {
            this.destructionRequest = destructionRequest;
            this.rtInstances = rtInstances;
            this.spriteInstanceFactory = spriteInstanceFactory;
            this.sharpGui = sharpGui;
            this.battleScreenLayout = battleScreenLayout;
            this.cameraProjector = cameraProjector;
            this.scaleHelper = scaleHelper;
            this.sprite = new Sprite(animations)
            { BaseScale = new Vector3(0.5f, 0.5f, 1f) };

            this.tlasData = new TLASInstanceData()
            {
                InstanceName = RTId.CreateId("TargetCursor"),
                Mask = RtStructures.OPAQUE_GEOM_MASK,
                Transform = new InstanceMatrix(Vector3.Zero, Quaternion.Identity, sprite.BaseScale)
            };

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

        IBattleTarget resistanceTextTarget;

        public void UpdateCursor(IBattleManager battleManager, IBattleTarget target, Vector3 enemyPos, BattlePlayer activePlayer)
        {
            SetPosition(enemyPos);

            battleScreenLayout.LayoutBattleMenu(selectTargetButton, nextTargetButton, previousTargetButton);

            if (activePlayer.Stats.CanSeeEnemyInfo)
            {
                if (resistanceTextTarget != target)
                {
                    resistanceTextTarget = target;
                    var resistances = new StringBuilder();
                    foreach (var resistance in target.Stats.Resistances.OrderBy(i => i.Value))
                    {
                        resistances.AppendLine($"{resistance.Key} - {resistance.Value}");
                    }
                    statText.Text = resistances.ToString();
                }

                var cursorOffset = currentPosition;
                cursorOffset.y -= sprite.BaseScale.y / 2.0f;
                var resistanceLoc = cameraProjector.Project(cursorOffset);

                statText.SetRect(new IntRect((int)resistanceLoc.x, (int)resistanceLoc.y, scaleHelper.Scaled(400), scaleHelper.Scaled(1000)));

                sharpGui.Text(statText);
            }

            if (sharpGui.Button(selectTargetButton, activePlayer.GamepadId, style: activePlayer.UiStyle))
            {
                SetTarget(target);
            }
            else if (sharpGui.Button(nextTargetButton, activePlayer.GamepadId, style: activePlayer.UiStyle))
            {
                NextTarget();
            }
            else if (sharpGui.Button(previousTargetButton, activePlayer.GamepadId, style: activePlayer.UiStyle))
            {
                PreviousTarget();
            }
            else
            {
                switch (sharpGui.GamepadButtonEntered[(int)activePlayer.GamepadId])
                {
                    case GamepadButtonCode.XInput_A:
                        SetTarget(target);
                        break;
                    case GamepadButtonCode.XInput_B:
                        SetTarget(null);
                        break;
                    case GamepadButtonCode.XInput_DPadUp:
                        NextTarget();
                        break;
                    case GamepadButtonCode.XInput_DPadDown:
                        PreviousTarget();
                        break;
                    case GamepadButtonCode.XInput_DPadLeft:
                    case GamepadButtonCode.XInput_DPadRight:
                        ChangeRow();
                        break;
                    case GamepadButtonCode.XInput_Y:
                        battleManager.SwitchPlayer();
                        SetTarget(null);
                        break;
                    default:
                        //Handle keyboard
                        switch (sharpGui.KeyEntered)
                        {
                            case KeyboardButtonCode.KC_RETURN:
                                SetTarget(target);
                                break;
                            case KeyboardButtonCode.KC_ESCAPE:
                                SetTarget(null);
                                break;
                            case KeyboardButtonCode.KC_UP:
                                NextTarget();
                                break;
                            case KeyboardButtonCode.KC_DOWN:
                                PreviousTarget();
                                break;
                            case KeyboardButtonCode.KC_LEFT:
                            case KeyboardButtonCode.KC_RIGHT:
                                ChangeRow();
                                break;
                            case KeyboardButtonCode.KC_LSHIFT:
                                battleManager.SwitchPlayer();
                                SetTarget(null);
                                break;
                        }
                        break;
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
    }
}
