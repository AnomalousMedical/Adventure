using Adventure.Items.Creators;
using Adventure.Menu;
using Adventure.Services;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPlugin;
using DiligentEngine;
using DiligentEngine.RT;
using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Threading.Tasks;

namespace Adventure.WorldMap
{
    class ElementalStone : IDisposable, IWorldMapPlaceable
    {
        public class Description : SceneObjectDesc
        {
            public Vector3[] Transforms { get; set; }

            public ISprite Sprite { get; set; }

            public SpriteMaterialDescription SpriteMaterial { get; set; }
        }

        public record Text
        (
            String Check,
            String ProveMastery,
            String ProvenWorthy
        );

        private readonly RTInstances<WorldMapScene> rtInstances;
        private readonly IDestructionRequest destructionRequest;
        private readonly SpriteInstanceFactory spriteInstanceFactory;
        private readonly IContextMenu contextMenu;
        private readonly IWorldMapManager worldMapManager;
        private readonly Persistence persistence;
        private readonly IWorldDatabase worldDatabase;
        private readonly ICoroutineRunner coroutineRunner;
        private readonly TextDialog textDialog;
        private readonly IExplorationMenu explorationMenu;
        private readonly TreasureMenu treasureMenu;
        private readonly AccessoryCreator accessoryCreator;
        private readonly ILanguageService languageService;
        private readonly CameraMover cameraMover;
        private readonly IAchievementService achievementService;
        private SpriteInstance spriteInstance;
        private readonly ISprite sprite;
        private readonly TLASInstanceData[] tlasData;
        private readonly IBepuScene<WorldMapScene> bepuScene;
        private readonly ICollidableTypeIdentifier<WorldMapScene> collidableIdentifier;
        private StaticHandle staticHandle;
        private TypedIndex shapeIndex;
        private bool physicsCreated = false;
        private bool graphicsCreated = false;
        private readonly Vector3[] transforms;

        private Vector3 currentPosition;
        private Quaternion currentOrientation;
        private Vector3 currentScale;

        private Vector3 cameraOffset = new Vector3(0, 1, -2);
        private Quaternion cameraAngle = new Quaternion(Vector3.Left, -MathF.PI / 8f);

        public ElementalStone
        (
            RTInstances<WorldMapScene> rtInstances,
            IDestructionRequest destructionRequest,
            IScopedCoroutine coroutine,
            IBepuScene<WorldMapScene> bepuScene,
            Description description,
            ICollidableTypeIdentifier<WorldMapScene> collidableIdentifier,
            SpriteInstanceFactory spriteInstanceFactory,
            IContextMenu contextMenu,
            IWorldMapManager worldMapManager,
            Persistence persistence,
            IWorldDatabase worldDatabase,
            ICoroutineRunner coroutineRunner,
            TextDialog textDialog,
            IExplorationMenu explorationMenu,
            TreasureMenu treasureMenu,
            AccessoryCreator accessoryCreator,
            ILanguageService languageService,
            CameraMover cameraMover,
            IAchievementService achievementService
        )
        {
            this.sprite = description.Sprite;
            this.rtInstances = rtInstances;
            this.destructionRequest = destructionRequest;
            this.bepuScene = bepuScene;
            this.collidableIdentifier = collidableIdentifier;
            this.spriteInstanceFactory = spriteInstanceFactory;
            this.contextMenu = contextMenu;
            this.worldMapManager = worldMapManager;
            this.persistence = persistence;
            this.worldDatabase = worldDatabase;
            this.coroutineRunner = coroutineRunner;
            this.textDialog = textDialog;
            this.explorationMenu = explorationMenu;
            this.treasureMenu = treasureMenu;
            this.accessoryCreator = accessoryCreator;
            this.languageService = languageService;
            this.cameraMover = cameraMover;
            this.achievementService = achievementService;
            this.transforms = description.Transforms;

            this.currentPosition = description.Translation;
            this.currentOrientation = description.Orientation;
            this.currentScale = sprite.BaseScale * description.Scale;

            var finalPosition = currentPosition;
            finalPosition.y += currentScale.y / 2.0f;

            this.tlasData = new TLASInstanceData[description.Transforms.Length];
            for (var i = 0; i < tlasData.Length; i++)
            {
                this.tlasData[i] = new TLASInstanceData()
                {
                    InstanceName = RTId.CreateId("ElementalStone"),
                    Mask = RtStructures.OPAQUE_GEOM_MASK,
                    Transform = new InstanceMatrix(finalPosition + description.Transforms[i], currentOrientation, currentScale)
                };
            }

            MoveToPosition();

            if (persistence.Current.PlotItems.Contains(PlotItems.ElementalStone))
            {
                coroutine.RunTask(async () =>
                {
                    await Task.Delay(1);
                    RequestDestruction();
                });
            }
            else
            {
                coroutine.RunTask(async () =>
                {
                    using var destructionBlock = destructionRequest.BlockDestruction(); //Block destruction until coroutine is finished and this is disposed.

                    this.spriteInstance = await spriteInstanceFactory.Checkout(description.SpriteMaterial, sprite);

                    foreach (var data in tlasData)
                    {
                        spriteInstance.UpdateBlas(data);
                        rtInstances.AddTlasBuild(data);
                    }

                    rtInstances.AddShaderTableBinder(Bind);
                    rtInstances.AddSprite(sprite);

                    graphicsCreated = true;
                });
            }
        }

        public void CreatePhysics()
        {
            if (!physicsCreated)
            {
                physicsCreated = true;
                var shape = new Box(0.25f, 1000, 0.25f); //TODO: Each one creates its own, try to load from resources
                shapeIndex = bepuScene.Simulation.Shapes.Add(shape);

                staticHandle = bepuScene.Simulation.Statics.Add(
                    new StaticDescription(
                        currentPosition.ToSystemNumerics(),
                        Quaternion.Identity.ToSystemNumerics(),
                        shapeIndex));

                bepuScene.RegisterCollisionListener(new CollidableReference(staticHandle), collisionEvent: HandleCollision, endEvent: HandleCollisionEnd);
            }
        }

        public void DestroyPhysics()
        {
            if (physicsCreated)
            {
                physicsCreated = false;
                bepuScene.UnregisterCollisionListener(new CollidableReference(staticHandle));
                bepuScene.Simulation.Shapes.Remove(shapeIndex);
                bepuScene.Simulation.Statics.Remove(staticHandle);
            }
        }

        public void Dispose()
        {
            if (graphicsCreated)
            {
                spriteInstanceFactory.TryReturn(spriteInstance);
                rtInstances.RemoveSprite(sprite);
                rtInstances.RemoveShaderTableBinder(Bind);
                foreach (var data in tlasData)
                {
                    rtInstances.RemoveTlasBuild(data);
                }
            }
            DestroyPhysics();
        }

        public void RequestDestruction()
        {
            this.destructionRequest.RequestDestruction();
        }

        protected virtual void HandleCollision(CollisionEvent evt)
        {
            if (collidableIdentifier.TryGetIdentifier<WorldMapPlayer>(evt.Pair.A, out var player)
               || collidableIdentifier.TryGetIdentifier<WorldMapPlayer>(evt.Pair.B, out player))
            {
                contextMenu.HandleContext(languageService.Current.ElementalStone.Check, Check, player.GamepadId);
            }
        }

        private void HandleCollisionEnd(CollisionEvent evt)
        {
            contextMenu.ClearContext(Check);
        }

        private void Check(ContextMenuArgs args)
        {
            if (!persistence.Current.PlotItems.Contains(PlotItems.ElementalStone))
            {
                if (persistence.Current.PlotItems.Contains(PlotItems.RuneOfFire)
                && persistence.Current.PlotItems.Contains(PlotItems.RuneOfIce)
                && persistence.Current.PlotItems.Contains(PlotItems.RuneOfElectricity))
                {
                    coroutineRunner.RunTask(async () =>
                    {
                        cameraMover.SetInterpolatedGoalPosition(this.currentPosition + cameraOffset, cameraAngle);
                        await textDialog.ShowTextAndWait(languageService.Current.ElementalStone.ProvenWorthy, args.GamepadId);
                        var treasure = new Treasure(accessoryCreator.CreateDoublecast(), TreasureType.Accessory);
                        treasureMenu.GatherTreasures(new[] { treasure }, treasureGivenCallback: t => persistence.Current.PlotItems.Add(PlotItems.ElementalStone));
                        explorationMenu.RequestSubMenu(treasureMenu, args.GamepadId);
                        contextMenu.ClearContext(Check);
                        achievementService.UnlockAchievement(Achievements.ElementalMastery);
                        RequestDestruction();
                    });
                }
                else
                {
                    coroutineRunner.RunTask(async () =>
                    {
                        cameraMover.SetInterpolatedGoalPosition(this.currentPosition + cameraOffset, cameraAngle);
                        await textDialog.ShowTextAndWait(languageService.Current.ElementalStone.ProveMastery, args.GamepadId);
                    });
                }
            }
        }

        private void MoveToPosition()
        {
            IntVector2 targetCell;

            targetCell = worldDatabase.ElementalStonePosition;

            currentPosition = worldMapManager.GetCellCenterpoint(targetCell);
            var finalPosition = currentPosition;
            finalPosition.y += currentScale.y / 2.0f;

            for (var i = 0; i < tlasData.Length; i++)
            {
                tlasData[i].Transform = new InstanceMatrix(finalPosition + transforms[i], currentOrientation, currentScale);
            }

            if (physicsCreated)
            {
                DestroyPhysics();
                CreatePhysics();
            }
        }

        private void Bind(IShaderBindingTable sbt, ITopLevelAS tlas)
        {
            foreach (var data in tlasData)
            {
                spriteInstance.Bind(data.InstanceName, sbt, tlas, sprite);
            }
        }
    }
}
