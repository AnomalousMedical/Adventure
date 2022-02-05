using Engine;

namespace Adventure
{
    internal interface IZonePlaceable
    {
        void RequestDestruction();
        void SetLevelPosition(in Vector3 levelPosition);
    }
}