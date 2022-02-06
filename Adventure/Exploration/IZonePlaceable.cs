using Engine;

namespace Adventure
{
    internal interface IZonePlaceable
    {
        void RequestDestruction();
        void SetZonePosition(in Vector3 zonePosition);
        void CreatePhysics();
        void DestroyPhysics();
    }
}