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

class PlayerCageEntry
{
    public Vector3 Position;
}

class PlayerCage<T> : IDisposable
{
    private List<PlayerCageEntry> entries = new List<PlayerCageEntry>();
    private TypedIndex boxShapeIndex = new TypedIndex();
    
    private BodyHandle left;
    private Vector3 leftOffset;

    private BodyHandle right;
    private Vector3 rightOffset;

    private BodyHandle far;
    private Vector3 farOffset;

    private BodyHandle near;
    private Vector3 nearOffset;

    private readonly IBepuScene<T> bepuScene;

    private const float BoxSize = 300f;
    private const float HalfBoxSize = BoxSize / 2f;
    private const float PlayerAreaSize = 6.0f;

    public PlayerCage(IBepuScene<T> bepuScene)
    {
        this.bepuScene = bepuScene;
        var shape = new Box(BoxSize, 1000f, BoxSize); //Each character creates a shape, try to load from resources somehow
        boxShapeIndex = bepuScene.Simulation.Shapes.Add(shape);

        left = bepuScene.Simulation.Bodies.Add(BodyDescription.CreateKinematic(new System.Numerics.Vector3(0f, 0f, 0f), boxShapeIndex, -1000f));
        leftOffset = new Vector3(-HalfBoxSize - PlayerAreaSize, 0f, 0f);

        right = bepuScene.Simulation.Bodies.Add(BodyDescription.CreateKinematic(new System.Numerics.Vector3(0f, 0f, 0f), boxShapeIndex, -1000f));
        rightOffset = new Vector3(HalfBoxSize + PlayerAreaSize, 0f, 0f);

        far = bepuScene.Simulation.Bodies.Add(BodyDescription.CreateKinematic(new System.Numerics.Vector3(0f, 0f, 0f), boxShapeIndex, -1000f));
        farOffset = new Vector3(0f, 0f, HalfBoxSize + PlayerAreaSize);

        near = bepuScene.Simulation.Bodies.Add(BodyDescription.CreateKinematic(new System.Numerics.Vector3(0f, 0f, 0f), boxShapeIndex, -1000f));
        nearOffset = new Vector3(0f, 0f, -HalfBoxSize - PlayerAreaSize);
    }

    public void Dispose()
    {
        bepuScene.Simulation.Bodies.Remove(left);
        bepuScene.Simulation.Shapes.Remove(boxShapeIndex);
    }

    public void Add(PlayerCageEntry entry)
    {
        entries.Add(entry);
    }

    public void Remove(PlayerCageEntry entry)
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
        foreach (var entry in entries)
        {
            currentPosition += entry.Position;
        }
        currentPosition /= entries.Count;

        {
            var characterBody = new BodyReference(left, bepuScene.Simulation.Bodies);
            characterBody.Pose.Position = (currentPosition + leftOffset).ToSystemNumerics();
        }

        {
            var characterBody = new BodyReference(right, bepuScene.Simulation.Bodies);
            characterBody.Pose.Position = (currentPosition + rightOffset).ToSystemNumerics();
        }

        {
            var characterBody = new BodyReference(far, bepuScene.Simulation.Bodies);
            characterBody.Pose.Position = (currentPosition + farOffset).ToSystemNumerics();
        }

        {
            var characterBody = new BodyReference(near, bepuScene.Simulation.Bodies);
            characterBody.Pose.Position = (currentPosition + nearOffset).ToSystemNumerics();
        }
    }
}
