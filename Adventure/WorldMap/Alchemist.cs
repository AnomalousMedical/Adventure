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
    class Alchemist : IDisposable, IWorldMapPlaceable
    {
        public class Description : SceneObjectDesc
        {
            public Vector3[] Transforms { get; set; }

            public ISprite Sprite { get; set; }

            public SpriteMaterialDescription SpriteMaterial { get; set; }
        }

        public record Text
        (
            String Greeting,
            String Intro1,
            String Intro2,
            String SalesPitch,
            String Ancient1,
            String Ancient2,
            String Goodbye,
            String LevelPotion1,
            String LevelPotion2
        );

        private readonly RTInstances<WorldMapScene> rtInstances;
        private readonly IDestructionRequest destructionRequest;
        private readonly SpriteInstanceFactory spriteInstanceFactory;
        private readonly IContextMenu contextMenu;
        private readonly IWorldMapManager worldMapManager;
        private readonly IWorldDatabase worldDatabase;
        private readonly TextDialog textDialog;
        private readonly ICoroutineRunner coroutineRunner;
        private readonly BuyMenu buyMenu;
        private readonly IExplorationMenu explorationMenu;
        private readonly Persistence persistence;
        private readonly ILanguageService languageService;
        private readonly CameraMover cameraMover;
        private readonly EarthquakeMenu earthquakeMenu;
        private readonly TreasureMenu treasureMenu;
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

        public Alchemist
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
            IWorldDatabase worldDatabase,
            TextDialog textDialog,
            ICoroutineRunner coroutineRunner,
            BuyMenu buyMenu,
            IExplorationMenu explorationMenu,
            Persistence persistence,
            ILanguageService languageService,
            CameraMover cameraMover,
            EarthquakeMenu earthquakeMenu,
            TreasureMenu treasureMenu
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
            this.worldDatabase = worldDatabase;
            this.textDialog = textDialog;
            this.coroutineRunner = coroutineRunner;
            this.buyMenu = buyMenu;
            this.explorationMenu = explorationMenu;
            this.persistence = persistence;
            this.languageService = languageService;
            this.cameraMover = cameraMover;
            this.earthquakeMenu = earthquakeMenu;
            this.treasureMenu = treasureMenu;
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
                    InstanceName = RTId.CreateId("Alchemist"),
                    Mask = RtStructures.OPAQUE_GEOM_MASK,
                    Transform = new InstanceMatrix(finalPosition + description.Transforms[i], currentOrientation, currentScale)
                };
            }

            MoveToPosition();

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
                contextMenu.HandleContext(languageService.Current.Alchemist.Greeting, Talk, player.GamepadId);
            }
        }

        private void HandleCollisionEnd(CollisionEvent evt)
        {
            contextMenu.ClearContext(Talk);
        }

        private void Talk(ContextMenuArgs args)
        {
            coroutineRunner.RunTask(async () =>
            {
                cameraMover.SetInterpolatedGoalPosition(this.currentPosition + cameraOffset, cameraAngle);
                if (!persistence.Current.PlotFlags.Contains(PlotFlags.AlchemistIntro))
                {
                    persistence.Current.PlotFlags.Add(PlotFlags.AlchemistIntro);
                    await textDialog.ShowTextAndWait(languageService.Current.Alchemist.Intro1, args.GamepadId);
                    await textDialog.ShowTextAndWait(languageService.Current.Alchemist.Intro2, args.GamepadId);
                }

                if (persistence.Current.PlotItems.Contains(PlotItems.AlchemistUpgrade))
                {
                    if(!persistence.Current.PlotFlags.Contains(PlotFlags.AlchemistUpgradeDelivered))
                    {
                        persistence.Current.PlotFlags.Add(PlotFlags.AlchemistUpgradeDelivered);
                        await textDialog.ShowTextAndWait(languageService.Current.Alchemist.Ancient1, args.GamepadId);
                        await textDialog.ShowTextAndWait(languageService.Current.Alchemist.Ancient2, args.GamepadId);
                    }
                }

                bool showStore = true;
                if (!persistence.Current.PlotFlags.Contains(PlotFlags.AlchemistLevelPotionDelivered)
                    && persistence.Current.PlotItems.Contains(PlotItems.GuideToPowerAndMayhem)
                    && persistence.Current.PlotItems.Contains(PlotItems.GuideToPowerAndMayhemChapter4)
                    && persistence.Current.PlotItems.Contains(PlotItems.GuideToPowerAndMayhemChapter5)
                    && persistence.Current.PlotItems.Contains(PlotItems.GuideToPowerAndMayhemChapter6))
                {
                    await textDialog.ShowTextAndWait(languageService.Current.Alchemist.LevelPotion1, args.GamepadId);
                    await earthquakeMenu.ShowAndWaitAndClose(args.GamepadId);
                    await textDialog.ShowTextAndWait(languageService.Current.Alchemist.LevelPotion2, args.GamepadId);
                    persistence.Current.PlotFlags.Add(PlotFlags.AlchemistLevelPotionDelivered);
                    treasureMenu.GatherTreasures(new[] { new Treasure(worldDatabase.PotionCreator.CreateLevelBoost(), TreasureType.Potion) });
                    explorationMenu.RequestSubMenu(treasureMenu, args.GamepadId);
                    showStore = false;
                }

                if (showStore)
                {
                    await textDialog.ShowTextAndWait(languageService.Current.Alchemist.SalesPitch, args.GamepadId);
                    buyMenu.PreviousMenu = null;
                    buyMenu.CurrentShopType = ShopType.Alchemist;
                    explorationMenu.RequestSubMenu(buyMenu, args.GamepadId);
                    await buyMenu.WaitForClose();
                    cameraMover.SetInterpolatedGoalPosition(this.currentPosition + cameraOffset, cameraAngle);
                    await textDialog.ShowTextAndWait(languageService.Current.Alchemist.Goodbye, args.GamepadId);
                }
            });
        }

        private void MoveToPosition()
        {
            IntVector2 targetCell = worldDatabase.AlchemistPosition;

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
