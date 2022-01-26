using Engine;

namespace Adventure
{
    interface ICameraProjector
    {
        Vector2 Project(in Vector3 position);
    }
}