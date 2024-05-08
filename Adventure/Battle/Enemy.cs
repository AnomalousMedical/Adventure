using Adventure.Assets;
using Adventure.Assets.SoundEffects;
using Adventure.Battle.Skills;
using Adventure.Services;
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
            public ISprite Sprite { get; set; }

            public SpriteMaterialDescription SpriteMaterial { get; set; }

            public BattleStats BattleStats { get; set; }

            public long GoldReward { get; set; }

            public ISoundEffect SoundEffect { get; set; }
        }

        private readonly RTInstances<BattleScene> rtInstances;
        private readonly IDestructionRequest destructionRequest;
        private readonly IScopedCoroutine coroutine;
        private readonly SpriteInstanceFactory spriteInstanceFactory;
        private SpriteInstance spriteInstance;
        private readonly ISprite sprite;
        private readonly TLASInstanceData tlasData;
        private bool disposed;
        private readonly ICharacterTimer characterTimer;
        private readonly IBattleManager battleManager;
        private readonly ITurnTimer turnTimer;
        private readonly ISkillFactory skillFactory;
        private readonly IObjectResolver objectResolver;
        private BattleStats battleStats;
        private Func<Clock, IBattleTarget, bool> counterAttack;
        private Attachment<BattleScene> castEffect;

        private Vector3 startPosition;
        private Vector3 currentPosition;
        private Quaternion currentOrientation;
        private Vector3 currentScale;

        public Vector3 MeleeAttackLocation => this.currentPosition + new Vector3(currentScale.x * 0.5f, 0, 0);

        public Vector3 MagicHitLocation => this.currentPosition + new Vector3(0f, 0f, -0.1f);

        public Vector3 EffectScale => this.currentScale;

        public ICharacterTimer CharacterTimer => characterTimer;

        public ISoundEffect DefaultAttackSoundEffect { get; private set; }

        public bool OffHandRaised { get; set; }

        public Enemy(
            RTInstances<BattleScene> rtInstances,
            IDestructionRequest destructionRequest,
            IScopedCoroutine coroutine,
            SpriteInstanceFactory spriteInstanceFactory,
            Desc description,
            ICharacterTimer characterTimer,
            IBattleManager battleManager,
            ITurnTimer turnTimer,
            IObjectResolverFactory objectResolverFactory,
            ISkillFactory skillFactory)
        {
            this.rtInstances = rtInstances;
            this.destructionRequest = destructionRequest;
            this.coroutine = coroutine;
            this.spriteInstanceFactory = spriteInstanceFactory;
            this.characterTimer = characterTimer;
            this.battleManager = battleManager;
            this.turnTimer = turnTimer;
            this.skillFactory = skillFactory;
            this.objectResolver = objectResolverFactory.Create();
            this.sprite = description.Sprite;
            this.sprite.RandomizeFrameTime();
            this.battleStats = description.BattleStats ?? throw new InvalidOperationException("You must include battle stats in an enemy description.");
            this.battleStats.CurrentHp = Stats.Hp;
            this.battleStats.CurrentMp = Stats.Mp;
            this.GoldReward = description.GoldReward;
            this.DefaultAttackSoundEffect = description.SoundEffect;

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
            ISkill skillToUse = null;
            foreach(var skill in battleStats.SkillConfig)
            {
                if(battleStats.Level > skill.MinLevel && battleStats.Level < skill.MaxLevel)
                {
                    const int maxRoll = 1000;
                    int needRoll = (int)(maxRoll * skill.Chance);
                    if(Random.Shared.Next(maxRoll) < needRoll)
                    {
                        skillToUse = skillFactory.CreateSkill(skill.Name);
                        break;
                    }
                }
            }

            if(skillToUse != null)
            {
                Cast(skillToUse);
            }
            else
            {
                MeleeAttack();
            }
        }

        private void MeleeAttack()
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
            IBattleTarget originalTarget = null; //The target that was targeted before the guard took over
            bool findGuard = true;
            bool startGuard = true;
            var blockManager = new ContextTriggerManager();

            battleManager.QueueTurn(c =>
            {
                //If there is a counter attack just do that
                if (counterAttack != null)
                {
                    var complete = counterAttack.Invoke(c, this);
                    if (complete)
                    {
                        counterAttack = null;
                    }
                    return false;
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
                    if (guardInput == null)
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
                    guardInput.OffHandRaised = blockManager.Activated && !blockManager.Spammed && !guardInput.IsDefending && !guardInput.IsDead;
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
                        if (guardInput != null && guardInput != target && !guardInput.IsDefending && !guardInput.IsDead)
                        {
                            guard = guardInput;
                            originalTarget = target;
                            target = guard;
                        }
                    }

                    if (needsAttack && remainingTime < swingTime)
                    {
                        needsAttack = false;
                        battleManager.Attack(this, target, false, blockManager.Activated, blockManager.Spammed, false, false, false);
                        guardInput.OffHandRaised = false;
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

                if (remainingTime < 0 || IsDead)
                {
                    sprite.SetAnimation("stand-left");
                    TurnComplete();
                    guard?.MoveToStart();
                    done = true;
                }
                else
                {
                    guard?.MoveToGuard(originalTarget.MeleeAttackLocation);
                }

                this.tlasData.Transform = new InstanceMatrix(this.currentPosition, this.currentOrientation, this.currentScale);
                //Sprite_FrameChanged(sprite);

                return done;
            });
        }

        private void Cast(ISkill skill)
        {
            castEffect?.RequestDestruction();
            castEffect = objectResolver.Resolve<Attachment<BattleScene>, Attachment<BattleScene>.Description>(o =>
            {
                var asset = new Assets.PixelEffects.Nebula();
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

            var castScaleFactor = MathF.Max(1.0f, MathF.Min(this.currentScale.x, this.currentScale.y));
            var castScale = new Vector3(castScaleFactor, castScaleFactor, this.currentScale.z);
            castEffect.SetPosition(this.currentPosition + new Vector3(0, 0, -0.01f), this.currentOrientation, castScale);

            var swingEnd = Quaternion.Identity;
            var swingStart = new Quaternion(0f, MathF.PI / 2.1f, 0f);

            long remainingTime = (long)(1.8f * Clock.SecondsToMicro);
            long standTime = (long)(0.2f * Clock.SecondsToMicro);
            long standStartTime = remainingTime / 2;
            long swingTime = standStartTime - standTime / 3;
            long standEndTime = standStartTime - standTime;
            bool needsAttack = true;
            var target = battleManager.GetRandomPlayer();
            IBattleTarget guardInput = null; //This is the character input to check for guard
            bool findGuard = true;
            var blockManager = new ContextTriggerManager();
            bool createSkillCastEffect = true;
            ISkillEffect skillEffect = null;

            battleManager.QueueTurn(c =>
            {
                //If there is a counter attack just do that
                if (counterAttack != null)
                {
                    var complete = counterAttack.Invoke(c, this);
                    if (complete)
                    {
                        counterAttack = null;
                    }
                    return false;
                }

                if (IsDead)
                {
                    DestroyCastEffect();
                    return true;
                }

                if (createSkillCastEffect)
                {
                    createSkillCastEffect = false;
                    castEffect?.RequestDestruction();
                    castEffect = objectResolver.Resolve<Attachment<BattleScene>, Attachment<BattleScene>.Description>(o =>
                    {
                        ISpriteAsset asset = skill.SpriteAsset;
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

                if (findGuard)
                {
                    findGuard = false;
                    guardInput = battleManager.GetGuard(this, target);
                    if (guardInput == null)
                    {
                        //If a guard can't be found, use the target
                        guardInput = target;
                    }
                }

                var done = false;
                remainingTime -= c.DeltaTimeMicro;
                Vector3 start;
                Vector3 end;
                float interpolate;

                if (remainingTime > standStartTime)
                {
                    //sprite.SetAnimation("cast-left");
                    if (!battleManager.IsStillValidTarget(target))
                    {
                        target = battleManager.ValidateTarget(this, target);
                    }
                    start = this.startPosition;
                    end = target.MeleeAttackLocation;
                    interpolate = (remainingTime - standStartTime) / (float)standStartTime;
                    blockManager.CheckTrigger(guardInput, false); //Can't ever block magic, but want to punish the player for spamming the button.
                }
                else if (remainingTime > standEndTime)
                {
                    //sprite.SetAnimation("cast-left");
                    interpolate = 0.0f;
                    start = target.MeleeAttackLocation;
                    end = target.MeleeAttackLocation;

                    if (needsAttack && remainingTime < swingTime)
                    {
                        needsAttack = false;
                        DestroyCastEffect();

                        var mpCost = skill.GetMpCost(false, false); //Enemy mp costs are always the base cost
                        if (battleStats.CurrentMp < mpCost)
                        {
                            battleManager.AddDamageNumber(this, "Not Enough MP", Color.Red);
                        }
                        else
                        {
                            TakeMp(mpCost);
                            //Apply skill bonus effect if the player spammed, never apply spammed effect for enemies since it is a penalty on the spell
                            //We want the player taking more damage if they are hitting the triggers for enemy spells
                            skillEffect = skill.Apply(battleManager, objectResolver, coroutine, this, target, blockManager.Spammed, false);
                        }
                    }
                }
                else
                {
                    //sprite.SetAnimation("stand-left");

                    start = target.MeleeAttackLocation;
                    end = this.startPosition;
                    interpolate = remainingTime / (float)standEndTime;
                }

                var position = end.lerp(start, interpolate);

                if (remainingTime < 0)
                {
                    position = end;
                    sprite.SetAnimation("stand-left");
                    TurnComplete();
                    done = true;
                }

                //Sprite_FrameChanged(sprite);

                if (castEffect != null)
                {
                    var scale = castScale;
                    if (blockManager.Spammed)
                    {
                        scale *= 1.63f;
                    }
                    else if (blockManager.Activated)
                    {
                        scale *= 0.72f;
                    }
                    castEffect.SetWorldPosition(position, this.currentOrientation, castEffect.BaseScale * scale);
                }

                return done;
            });
        }

        private void DestroyCastEffect()
        {
            castEffect?.RequestDestruction();
            castEffect = null;
        }

        public void SetCounterAttack(Func<Clock, IBattleTarget, bool> counter)
        {
            this.counterAttack = counter;
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
            DestroyCastEffect();
            turnTimer.RemoveTimer(characterTimer);
            disposed = true;
            this.spriteInstanceFactory.TryReturn(spriteInstance);
            rtInstances.RemoveSprite(sprite);
            rtInstances.RemoveShaderTableBinder(Bind);
            rtInstances.RemoveTlasBuild(tlasData);
            objectResolver.Dispose();
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

        public bool IsDefending => false;

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
