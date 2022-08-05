using BepuPhysics.Collidables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure
{
    interface ICollidableTypeIdentifier<IdType>
    {
        void AddIdentifier<T>(CollidableReference collidable, T reference);
        void RemoveIdentifier(CollidableReference collidable);
        bool TryGetIdentifier(CollidableReference collidable, out object value);
        bool TryGetIdentifier<T>(CollidableReference collidable, out T value);
    }

    class CollidableTypeIdentifier<IdType> : ICollidableTypeIdentifier<IdType>
    {
        private Dictionary<CollidableReference, Object> identifiers = new Dictionary<CollidableReference, object>();

        public void AddIdentifier<T>(CollidableReference collidable, T reference)
        {
            identifiers.Add(collidable, reference);
        }

        public void RemoveIdentifier(CollidableReference collidable)
        {
            identifiers.Remove(collidable);
        }

        public bool TryGetIdentifier(CollidableReference collidable, out Object value)
        {
            return TryGetIdentifier<Object>(collidable, out value);
        }

        public bool TryGetIdentifier<T>(CollidableReference collidable, out T value)
        {
            identifiers.TryGetValue(collidable, out var obj);
            if (obj is T)
            {
                value = (T)obj;
                return true;
            }
            value = default(T);
            return false;
        }
    }
}
