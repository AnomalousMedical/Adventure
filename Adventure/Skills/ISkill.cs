using Adventure.Assets;
using Adventure.Battle;
using Adventure.Services;
using Engine;
using Engine.Platform;
using RpgMath;
using System;

namespace Adventure.Skills
{
    public enum SkillAttackStyle
    {
        Melee,
        Cast
    }

    interface ISkill
    {
        ISkillEffect Apply(IDamageCalculator damageCalculator, CharacterSheet source, CharacterSheet target, CharacterMenuPositionService characterMenuPositionService, IObjectResolver objectResolver, IScopedCoroutine coroutine, CameraMover cameraMover, ISoundEffectPlayer soundEffectPlayer) { return null; }

        ISkillEffect Apply(IBattleManager battleManager, IObjectResolver objectResolver, IScopedCoroutine coroutine, IBattleTarget attacker, IBattleTarget target, bool triggered, bool triggerSpammed);

        bool DefaultTargetPlayers => false;

        /// <summary>
        /// If this is false a skill will auto target the current player.
        /// </summary>
        bool NeedsTarget => true;

        bool QueueFront => false;

        bool UseInField => false;

        string Name { get; }

        long GetMpCost(bool triggered, bool triggerSpammed);

        SkillAttackStyle AttackStyle { get; }

        ISpriteAsset CastSpriteAsset => new Assets.PixelEffects.Nebula();

        Color CastColor => Color.FromARGB(0xff639cff);
    }

    interface ISkillEffect
    {
        bool Finished { get; }

        void Update(Clock clock) { }
    }

    class SkillEffect : ISkillEffect
    {
        public SkillEffect(bool finished = false)
        {
            this.Finished = finished;
        }

        public bool Finished { get; set; }

        public void Update(Clock clock)
        {

        }
    }

    class CallbackSkillEffect : ISkillEffect
    {
        private readonly Action<Clock> update;

        public CallbackSkillEffect(Action<Clock> update, bool finished = false)
        {
            this.update = update;
            this.Finished = finished;
        }

        public bool Finished { get; set; }

        public void Update(Clock clock)
        {
            update(clock);
        }
    }
}
