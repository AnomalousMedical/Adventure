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
using System.Linq;
using System.Threading.Tasks;

namespace Adventure.WorldMap
{
    class FortuneTeller : IDisposable, IWorldMapPlaceable
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
            String StartShufflePitch,
            String CardShuffleNarrator,
            String ShowResultsNarrator,
            String NoResultsNarrator,
            String AssassinFortune,
            String HourglassFortune,
            String LuckFortune,
            String CountrysideFortune,
            String SnowyFortune,
            String ForestFortune,
            String BeachFortune,
            String SwampFortune,
            String MountainFortune,
            String FirstCardIntro,
            String SecondCardIntro
        )
        {
            public String GetFortune(BiomeType biome)
            {
                switch (biome)
                {
                    case BiomeType.Countryside:
                        return CountrysideFortune;
                    case BiomeType.Snowy:
                        return SnowyFortune;
                    case BiomeType.Forest:
                        return ForestFortune;
                    case BiomeType.Beach:
                        return BeachFortune;
                    case BiomeType.Swamp:
                        return SwampFortune;
                    case BiomeType.Mountain:
                        return MountainFortune;
                }

                return "Missing_" + biome;
            }
            public String GetFortune(TreasureFortuneType treasure)
            {
                switch (treasure)
                {
                    case TreasureFortuneType.Assassin:
                        return AssassinFortune;
                    case TreasureFortuneType.Hourglass:
                        return HourglassFortune;
                    case TreasureFortuneType.Luck:
                        return LuckFortune;
                }

                return "Missing_" + treasure;
            }
        }

        private readonly RTInstances<WorldMapScene> rtInstances;
        private readonly IDestructionRequest destructionRequest;
        private readonly SpriteInstanceFactory spriteInstanceFactory;
        private readonly IContextMenu contextMenu;
        private readonly IWorldMapManager worldMapManager;
        private readonly IWorldDatabase worldDatabase;
        private readonly TextDialog textDialog;
        private readonly ICoroutineRunner coroutineRunner;
        private readonly Persistence persistence;
        private readonly ILanguageService languageService;
        private readonly CameraMover cameraMover;
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

        public FortuneTeller(
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
            Persistence persistence,
            ILanguageService languageService,
            CameraMover cameraMover)
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
            this.persistence = persistence;
            this.languageService = languageService;
            this.cameraMover = cameraMover;
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
                    InstanceName = RTId.CreateId("FortuneTeller"),
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
                contextMenu.HandleContext(languageService.Current.FortuneTeller.Greeting, Talk, player.GamepadId);
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

                if (!persistence.Current.PlotFlags.Contains(PlotFlags.FortuneTellerIntro))
                {
                    persistence.Current.PlotFlags.Add(PlotFlags.FortuneTellerIntro);
                    await textDialog.ShowTextAndWait(languageService.Current.FortuneTeller.Intro1, args.GamepadId);
                }

                await textDialog.ShowTextAndWait(languageService.Current.FortuneTeller.StartShufflePitch, args.GamepadId);
                await textDialog.ShowTextAndWait(languageService.Current.FortuneTeller.CardShuffleNarrator, args.GamepadId);

                var completedLevels = persistence.Current.World.CompletedAreaLevels;
                var zonesWithTreasure = worldDatabase.AreaBuilders.Where(i => i.UniqueStealTreasure?.Any() == true);
                var stolenUniqueTreasures = persistence.Current.UniqueStolenTreasure.Entries;
                var showedCards = false;

                //For all zones that are completed and have treasure
                foreach (var zone in zonesWithTreasure.Where(i => completedLevels.ContainsKey(i.Index)))
                {
                    foreach(var treasure in zone.UniqueStealTreasure.Where(i =>
                    {
                        if (stolenUniqueTreasures.TryGetValue(zone.Index, out var treasureData))
                        {
                            //Treasure not in zone's list of stolen unique treasure
                            return !treasureData.Values.Any(ut => ut.Stolen && ut.UniqueTreasureIds?.Contains(i.Id) == true);
                        }
                        //No treasure stolen yet, passes the filter
                        return true;
                    }))
                    {
                        if(!showedCards)
                        {
                            await textDialog.ShowTextAndWait(languageService.Current.FortuneTeller.ShowResultsNarrator, args.GamepadId);
                            showedCards = true;
                        }
                        
                        if (Random.Shared.Next(0, 100) < 50)
                        {
                            await textDialog.ShowTextAndWait(languageService.Current.FortuneTeller.FirstCardIntro + languageService.Current.FortuneTeller.GetFortune(zone.Biome), args.GamepadId);
                            await textDialog.ShowTextAndWait(languageService.Current.FortuneTeller.SecondCardIntro + languageService.Current.FortuneTeller.GetFortune(treasure.FortuneText), args.GamepadId);
                        }
                        else
                        {
                            await textDialog.ShowTextAndWait(languageService.Current.FortuneTeller.FirstCardIntro + languageService.Current.FortuneTeller.GetFortune(treasure.FortuneText), args.GamepadId);
                            await textDialog.ShowTextAndWait(languageService.Current.FortuneTeller.SecondCardIntro + languageService.Current.FortuneTeller.GetFortune(zone.Biome), args.GamepadId);
                        }
                    }
                }

                if(!showedCards)
                {
                    await textDialog.ShowTextAndWait(languageService.Current.FortuneTeller.NoResultsNarrator, args.GamepadId);
                }
            });
        }

        private void MoveToPosition()
        {
            IntVector2 targetCell = worldDatabase.FortuneTellerPosition;

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
