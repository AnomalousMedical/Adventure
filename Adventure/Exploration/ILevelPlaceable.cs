using Engine;

namespace Adventure
{
    internal interface ILevelPlaceable
    {
        void RequestDestruction();
        void SetLevelPosition(in Vector3 levelPosition);
    }
}