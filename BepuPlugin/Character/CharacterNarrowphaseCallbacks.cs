using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using System.Runtime.CompilerServices;
using BepuPhysics.Constraints;

namespace BepuPlugin.Characters
{
    /// <summary>
    /// Implements simple callbacks to inform the CharacterControllers system of created contacts.
    /// </summary>
    struct CharacterNarrowphaseCallbacks<TEventHandler> : INarrowPhaseCallbacks where TEventHandler : IContactEventHandler
    {
        public CollidableProperty<SubgroupCollisionFilter> CollisionFilters;
        public CharacterControllers Characters;
        ContactEvents<TEventHandler> events;

        public CharacterNarrowphaseCallbacks(CollidableProperty<SubgroupCollisionFilter> filters, CharacterControllers characters, ContactEvents<TEventHandler> events)
        {
            this.CollisionFilters = filters;
            this.Characters = characters;
            this.events = events;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b, ref float speculativeMargin)
        {
            //It's impossible for two statics to collide, and pairs are sorted such that bodies always come before statics.
            //This info comes straight from the demo
            if (b.Mobility != CollidableMobility.Static)
            {
                return SubgroupCollisionFilter.AllowCollision(CollisionFilters[a.BodyHandle], CollisionFilters[b.BodyHandle]);
            }
            return a.Mobility == CollidableMobility.Dynamic || b.Mobility == CollidableMobility.Dynamic;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB)
        {
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, ref TManifold manifold, out PairMaterialProperties pairMaterial) where TManifold : unmanaged, IContactManifold<TManifold>
        {
            pairMaterial = new PairMaterialProperties { FrictionCoefficient = 1, MaximumRecoveryVelocity = 2, SpringSettings = new SpringSettings(30, 1) };
            Characters.TryReportContacts(pair, ref manifold, workerIndex, ref pairMaterial);
            events.HandleManifold(workerIndex, pair, ref manifold);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ref ConvexContactManifold manifold)
        {
            return true;
        }

        public void Dispose()
        {
            Characters.Dispose();
            CollisionFilters.Dispose();
        }

        public void Initialize(Simulation simulation)
        {
            Characters.Initialize(simulation);
            events.Initialize(simulation.Bodies);
            CollisionFilters.Initialize(simulation);
        }
    }
}


