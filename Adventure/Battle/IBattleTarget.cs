using Engine;
using RpgMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Battle
{
    public interface IBattleTarget
    {
        IBattleStats Stats { get; }

        Vector3 DamageDisplayLocation { get; }

        Vector3 CursorDisplayLocation { get; }

        Vector3 MagicHitLocation { get; }

        Vector3 EffectScale { get; }

        public BattleTargetType BattleTargetType { get; }

        public void RequestDestruction();

        public void ApplyDamage(IBattleTarget attacker, IDamageCalculator calculator, long damage);

        public bool IsDead { get; }

        Vector3 MeleeAttackLocation { get; }

        void Resurrect(IDamageCalculator damageCalculator, long damage);

        void TakeMp(long mp);

        void MoveToGuard(in Vector3 position);
        
        void MoveToStart();

        void AttemptMeleeCounter(IBattleTarget attacker);

        bool TryContextTrigger();
    }
}
