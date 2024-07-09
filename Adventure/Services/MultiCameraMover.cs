using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Trees;
using BepuPlugin;
using BepuPlugin.Characters;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Services;

class MultiCameraMoverEntry
{
    public Vector3 Position;

    public Vector3 SpeedOffset;
}

class MultiCameraMover<T>
(
    MultiCameraMover<T>.Description description,
    CameraMover cameraMover,
    IBepuScene<T> bepuScene,
    ICollidableTypeIdentifier<T> collidableIdentifier
)
{
    public class Description
    {
        public Vector3 cameraOffset = new Vector3(0, 4, -12);
        public Quaternion cameraAngle = new Quaternion(Vector3.Left, -MathF.PI / 10f);
    }

    private List<MultiCameraMoverEntry> entries = new List<MultiCameraMoverEntry>();

    private Vector3 cameraOffset = description.cameraOffset;
    private Quaternion cameraAngle = description.cameraAngle;

    public void Add(MultiCameraMoverEntry entry)
    {
        entries.Add(entry);
    }

    public void Remove(MultiCameraMoverEntry entry)
    {
        entries.Remove(entry);
    }

    public void Update()
    {
        if (entries.Count == 0)
        {
            return;
        }

        Vector3 currentPosition = new Vector3();
        Vector3 speedOffset = new Vector3();
        foreach (var entry in entries)
        {
            currentPosition += entry.Position;
            speedOffset += entry.SpeedOffset;
        }
        currentPosition /= entries.Count;
        speedOffset /= entries.Count;

        speedOffset.y = 0;
        var camPos = currentPosition + cameraOffset + speedOffset;
        var rayAdjust = FindCameraTerrainOffset(camPos, (currentPosition - camPos).normalized());
        cameraMover.SetInterpolatedGoalPosition(camPos + rayAdjust, cameraAngle);
    }

    internal void CenterCamera()
    {
        if (entries.Count == 0)
        {
            return;
        }

        Vector3 currentPosition = new Vector3();
        foreach (var entry in entries)
        {
            currentPosition += entry.Position;
        }
        currentPosition /= entries.Count;

        var camPos = currentPosition + cameraOffset;
        var rayAdjust = FindCameraTerrainOffset(camPos, (currentPosition - camPos).normalized());
        cameraMover.SetPosition(camPos + rayAdjust, cameraAngle);
    }

    class RayHit
    {
        public RayHit(float t, in CollidableReference collidable)
        {
            this.T = t;
            this.Collidable = collidable;
        }

        public float T;
        public CollidableReference Collidable;
    }

    struct RayHitHandler : IRayHitHandler
    {
        class HitComparer : IComparer<RayHit>
        {
            public static readonly HitComparer Instance = new HitComparer();

            public int Compare(RayHit x, RayHit y)
            {
                if (x.T > y.T) { return -1; }
                if (x.T < y.T) { return 1; }
                return 0;
            }
        }

        private SortedSet<RayHit> hits = new SortedSet<RayHit>(HitComparer.Instance);
        public RayHitHandler() { }

        public IEnumerable<RayHit> Hits => hits;


        public bool AllowTest(CollidableReference collidable)
        {
            return true;
        }

        public bool AllowTest(CollidableReference collidable, int childIndex)
        {
            return true;
        }

        public void OnRayHit(in RayData ray, ref float maximumT, float t, in System.Numerics.Vector3 normal, CollidableReference collidable, int childIndex)
        {
            hits.Add(new RayHit(t, collidable));
        }
    }

    public Vector3 FindCameraTerrainOffset(in Vector3 origin, in Vector3 direction, float maxT = float.MaxValue)
    {
        var hitOffset = Vector3.Zero;
        var hitHandler = new RayHitHandler();
        bepuScene.Simulation.RayCast(origin.ToSystemNumerics(), direction.ToSystemNumerics(), maxT, ref hitHandler);

        var findPlayer = true;
        foreach (var hit in hitHandler.Hits)
        {
            if (findPlayer)
            {
                if (collidableIdentifier.TryGetIdentifier<Player>(hit.Collidable, out var _))
                {
                    findPlayer = false;
                }
            }
            else
            {
                if (collidableIdentifier.TryGetIdentifier<Zone>(hit.Collidable, out var _))
                {
                    hitOffset = direction * hit.T + new Vector3(0f, 0f, 0.05f);
                    break;
                }
            }
        }

        return hitOffset;
    }
}
