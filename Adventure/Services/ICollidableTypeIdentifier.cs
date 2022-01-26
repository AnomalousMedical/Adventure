using BepuPhysics.Collidables;

namespace Adventure
{
    interface ICollidableTypeIdentifier
    {
        void AddIdentifier<T>(CollidableReference collidable, T reference);
        void RemoveIdentifier(CollidableReference collidable);
        bool TryGetIdentifier(CollidableReference collidable, out object value);
        bool TryGetIdentifier<T>(CollidableReference collidable, out T value);
    }
}