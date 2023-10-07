using DiligentEngine;
using DiligentEngine.RT;
using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using RpgMath;
using System;

namespace Adventure.Battle
{
    class Enemy : IDisposable, IBattleTarget
    {
        public class Desc : SceneObjectDesc
        {
            public Sprite Sprite { get; set; }

            public SpriteMaterialDescription SpriteMaterial { get; set; }

            public BattleStats BattleStats { get; set; }

            public long GoldReward { get; set; }
        }

        private readonly RTInstances<BattleScene> rtInstances;
        private readonly IDestructionRequest destructionRequest;
        private readonly SpriteInstanceFactory spriteInstanceFactory;
        private SpriteInstance spriteInstance;
        private readonly Sprite sprite;
        private readonly TLASInstanceData tlasData;
        private bool disposed;
        private readonly ICharacterTimer characterTimer;
        private readonly IBattleManager battleManager;
        private readonly ITurnTimer turnTimer;
        private BattleStats battleStats;

        private Vector3 startPosition;
        private Vector3 currentPosition;
        private Quaternion currentOrientation;
        private Vector3 currentScale;

        public Vector3 MeleeAttackLocation => this.currentPosition + new Vector3(currentScale.x * 0.5f, 0, 0);

        public Vector3 MagicHitLocation => this.currentPosition + new Vector3(0f, 0f, -0.1f);

        public Vector3 EffectScale => this.currentScale;

        public ICharacterTimer CharacterTimer => characterTimer;

        public Enemy(
            RTInstances<BattleScene> rtInstances,
            IDestructionRequest destructionRequest,
            IScopedCoroutine coroutine,
            SpriteInstanceFactory spriteInstanceFactory,
            Desc description,
            ICharacterTimer characterTimer,
            IBattleManager battleManager,
            ITurnTimer turnTimer)
        {
            this.rtInstances = rtInstances;
            this.destructionRequest = destructionRequest;
            this.spriteInstanceFactory = spriteInstanceFactory;
            this.characterTimer = characterTimer;
            this.battleManager = battleManager;
            this.turnTimer = turnTimer;
            this.sprite = description.Sprite;
            this.sprite.RandomizeFrameTime();
            this.battleStats = description.BattleStats ?? throw new InvalidOperationException("You must include battle stats in an enemy description.");
            this.battleStats.CurrentHp = Stats.Hp;
            this.battleStats.CurrentMp = Stats.Mp;
            this.GoldReward = description.GoldReward;

            turnTimer.AddTimer(characterTimer);
            characterTimer.TurnReady += CharacterTimer_TurnReady;
            characterTimer.TotalDex = () => Stats.TotalDexterity;

            this.currentPosition = description.Translation;
            this.currentOrientation = description.Orientation;
            this.currentScale = sprite.BaseScale * description.Scale;
            this.startPosition = currentPosition;

            this.tlasData = new TLASInstanceData()
            {
                InstanceName = RTId.CreateId("Enemy"),
                Mask = RtStructures.OPAQUE_GEOM_MASK,
                Transform = new InstanceMatrix(currentPosition, currentOrientation, currentScale)
            };

            coroutine.RunTask(async () =>
            {
                using var destructionBlock = destructionRequest.BlockDestruction(); //Block destruction until coroutine is finished and this is disposed.

                this.spriteInstance = await spriteInstanceFactory.Checkout(description.SpriteMaterial, sprite);

                if (this.disposed)
                {
                    this.spriteInstanceFactory.TryReturn(spriteInstance);
                    return; //Stop loading
                }

                rtInstances.AddTlasBuild(tlasData);
                rtInstances.AddShaderTableBinder(Bind);
                rtInstances.AddSprite(sprite, tlasData, spriteInstance);
            });
        }

        private void CharacterTimer_TurnReady(ICharacterTimer obj)
        {
            var swingEnd = Quaternion.Identity;
            var swingStart = new Quaternion(0f, MathF.PI / 2.1f, 0f);

            long remainingTime = (long)(1.8f * Clock.SecondsToMicro);
            long standTime = (long)(0.2f * Clock.SecondsToMicro);
            long standStartTime = remainingTime / 2;
            long swingTime = standStartTime - standTime / 3;
            long standEndTime = standStartTime - standTime;
            bool needsAttack = true;
            var target = battleManager.GetRandomPlayer();
            IBattleTarget guard = null; //This is the guard that moved, if there was one
            IBattleTarget guardInput = null; //This is the character input to check for guard
            bool findGuard = true;
            bool startGuard = true;
            var blockManager = new ContextTriggerManager();

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

                if (findGuard)
                {
                    findGuard = false;
                    guardInput = battleManager.GetGuard(this, target);
                    if(guardInput == null)
                    {
                        //If a guard can't be found, use the target
                        guardInput = target;
                    }
                }

                if (remainingTime > standStartTime)
                {
                    //sprite.SetAnimation("left");
                    target = battleManager.ValidateTarget(this, target);
                    start = this.startPosition;
                    end = GetAttackLocation(target);
                    interpolate = (remainingTime - standStartTime) / (float)standStartTime;
                    blockManager.CheckTrigger(guardInput, guardInput.Stats.CanBlock);
                }
                else if (remainingTime > standEndTime)
                {
                    var slerpAmount = (remainingTime - standEndTime) / (float)standEndTime;
                    //sword.SetAdditionalRotation(swingStart.slerp(swingEnd, slerpAmount));
                    //sprite.SetAnimation("stand-left");
                    interpolate = 0.0f;
                    start = end = GetAttackLocation(target);

                    if (startGuard && blockManager.Activated && !blockManager.Spammed)
                    {
                        startGuard = false;
                        if (guardInput != null && guardInput != target)
                        {
                            guard = guardInput;
                            guard.MoveToGuard(target.MeleeAttackLocation);
                            target = guard;
                        }
                    }

                    if (needsAttack && remainingTime < swingTime)
                    {
                        needsAttack = false;
                        battleManager.Attack(this, target, false, blockManager.Activated, blockManager.Spammed, false, false, false);
                    }
                }
                else
                {
                    //sprite.SetAnimation("right");

                    //sword.SetAdditionalRotation(Quaternion.Identity);

                    start = GetAttackLocation(target);
                    end = this.startPosition;
                    interpolate = remainingTime / (float)standEndTime;
                }

                this.currentPosition = end.lerp(start, interpolate);

                if (remainingTime < 0)
                {
                    sprite.SetAnimation("stand-left");
                    TurnComplete();
                    guard?.MoveToStart();
                    done = true;
                }

                this.tlasData.Transform = new InstanceMatrix(this.currentPosition, this.currentOrientation, this.currentScale);
                //Sprite_FrameChanged(sprite);

                return done;
            });
        }

        private Vector3 GetAttackLocation(IBattleTarget target)
        {
            var totalScale = currentScale;
            var targetAttackLocation = target.MeleeAttackLocation;
            targetAttackLocation.x -= totalScale.x / 2;
            targetAttackLocation.y = totalScale.y / 2.0f;
            return targetAttackLocation;
        }

        private void TurnComplete()
        {
            characterTimer.Reset();
            characterTimer.TurnTimerActive = true;
        }

        public void RequestDestruction()
        {
            this.destructionRequest.RequestDestruction();
        }

        public void Dispose()
        {
            turnTimer.RemoveTimer(characterTimer);
            disposed = true;
            this.spriteInstanceFactory.TryReturn(spriteInstance);
            rtInstances.RemoveSprite(sprite);
            rtInstances.RemoveShaderTableBinder(Bind);
            rtInstances.RemoveTlasBuild(tlasData);
        }

        public void ApplyDamage(IBattleTarget attacker, IDamageCalculator calculator, long damage)
        {
            battleStats.CurrentHp = calculator.ApplyDamage(damage, battleStats.CurrentHp, Stats.Hp);
        }

        public void AttemptMeleeCounter(IBattleTarget attacker)
        {
            //No enemy counters for now
        }

        public void Resurrect(IDamageCalculator damageCalculator, long damage)
        {
            //Doesn't do anything
        }

        public void TakeMp(long mp)
        {
            battleStats.CurrentMp -= mp;
            if (battleStats.CurrentMp < 0)
            {
                battleStats.CurrentMp = 0;
            }
            else if (battleStats.CurrentMp > Stats.Mp)
            {
                battleStats.CurrentMp = Stats.Mp;
            }
        }

        public void MoveToGuard(in Vector3 position)
        {
            this.currentPosition = position;
            this.tlasData.Transform = new InstanceMatrix(this.currentPosition, this.currentOrientation, this.currentScale);
        }

        public void MoveToStart()
        {
            this.currentPosition = this.startPosition;
            this.tlasData.Transform = new InstanceMatrix(this.currentPosition, this.currentOrientation, this.currentScale);
        }

        public Vector3 DamageDisplayLocation => currentPosition + new Vector3(0.5f * currentScale.x, 0.5f * currentScale.y, 0f);

        public IBattleStats Stats => battleStats;

        public Vector3 CursorDisplayLocation => DamageDisplayLocation;

        public bool IsDead => this.battleStats.CurrentHp == 0;

        public BattleTargetType BattleTargetType => BattleTargetType.Enemy;

        public long GoldReward { get; private set; }

        private void Bind(IShaderBindingTable sbt, ITopLevelAS tlas)
        {
            spriteInstance.Bind(this.tlasData.InstanceName, sbt, tlas, sprite);
        }

        public bool TryContextTrigger()
        {
            return false;
        }
    }
}
