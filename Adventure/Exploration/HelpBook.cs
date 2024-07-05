using Adventure.Menu;
using Adventure.Services;
using BepuPhysics.Collidables;
using BepuPhysics;
using BepuPlugin;
using DiligentEngine.RT.Sprites;
using DiligentEngine.RT;
using DiligentEngine;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adventure.Assets.Equipment;

namespace Adventure.Exploration;

class HelpBook : IDisposable, IZonePlaceable
{
    public class Description : SceneObjectDesc
    {
        public Vector3 MapOffset { get; set; }

        public PlotItems PlotItem { get; set; }
    }

    private readonly RTInstances<ZoneScene> rtInstances;
    private readonly IDestructionRequest destructionRequest;
    private readonly IScopedCoroutine coroutine;
    private readonly SpriteInstanceFactory spriteInstanceFactory;
    private readonly IContextMenu contextMenu;
    private readonly Persistence persistence;
    private readonly ILanguageService languageService;
    private readonly TextDialog textDialog;
    private readonly ConfirmMenu confirmMenu;
    private readonly IExplorationMenu explorationMenu;
    private readonly HelpMenu helpMenu;
    private SpriteInstance spriteInstance;
    private bool graphicsLoaded = false;
    private readonly ISprite sprite;
    private readonly TLASInstanceData tlasData;
    private readonly IBepuScene<ZoneScene> bepuScene;
    private readonly ICollidableTypeIdentifier<IExplorationGameState> collidableIdentifier;
    private readonly Vector3 mapOffset;
    private StaticHandle staticHandle;
    private TypedIndex shapeIndex;
    private bool physicsCreated = false;
    private bool graphicsCreated = false;
    private PlotItems plotItem;

    private Vector3 currentPosition;
    private Quaternion currentOrientation;
    private Vector3 currentScale;

    public record Text
    (
        String Greeting,
        String BookIntro,
        String ReadBookPrompt,
        String TakeBookPrompt,
        String PageIntro,
        String ReadPagePrompt,
        String TakePagePrompt,
        String Chapter1Title,
        String Chapter1,
        String Chapter2Title,
        String Chapter2,
        String Chapter3Title,
        String Chapter3,
        String Chapter4Title,
        String Chapter4,
        String Chapter4Part2Missing,
        String Chapter4Part2,
        String Chapter5Title,
        String Chapter5,
        String Chapter5Part2Missing,
        String Chapter5Part2,
        String Chapter6Title,
        String Chapter6,
        String Chapter6Part2Missing,
        String Chapter6Part2,
        String Chapter7Title,
        String Chapter7,
        String Chapter7Part2,
        String Chapter8Title,
        String Chapter8,
        String BookTaken,
        String Page1Found,
        String Page2Found,
        String AllPagesFound,
        String CannotTakePage
    );

    public HelpBook
    (
        RTInstances<ZoneScene> rtInstances,
        IDestructionRequest destructionRequest,
        IScopedCoroutine coroutine,
        IBepuScene<ZoneScene> bepuScene,
        Description description,
        ICollidableTypeIdentifier<IExplorationGameState> collidableIdentifier,
        SpriteInstanceFactory spriteInstanceFactory,
        IContextMenu contextMenu,
        Persistence persistence,
        ILanguageService languageService,
        TextDialog textDialog,
        ConfirmMenu confirmMenu,
        IExplorationMenu explorationMenu,
        HelpMenu helpMenu
    )
    {
        var asset = new Book3();

        this.plotItem = description.PlotItem;
        this.sprite = asset.CreateSprite();
        this.rtInstances = rtInstances;
        this.destructionRequest = destructionRequest;
        this.coroutine = coroutine;
        this.bepuScene = bepuScene;
        this.collidableIdentifier = collidableIdentifier;
        this.spriteInstanceFactory = spriteInstanceFactory;
        this.contextMenu = contextMenu;
        this.persistence = persistence;
        this.languageService = languageService;
        this.textDialog = textDialog;
        this.confirmMenu = confirmMenu;
        this.explorationMenu = explorationMenu;
        this.helpMenu = helpMenu;
        this.mapOffset = description.MapOffset;

        this.currentPosition = description.Translation;
        this.currentOrientation = description.Orientation;
        this.currentScale = sprite.BaseScale * description.Scale;

        var finalPosition = currentPosition;
        finalPosition.y += currentScale.y / 2.0f;

        this.tlasData = new TLASInstanceData()
        {
            InstanceName = RTId.CreateId("Key"),
            Mask = RtStructures.OPAQUE_GEOM_MASK,
            Transform = new InstanceMatrix(finalPosition, currentOrientation, currentScale)
        };

        coroutine.RunTask(async () =>
        {
            using var destructionBlock = destructionRequest.BlockDestruction(); //Block destruction until coroutine is finished and this is disposed.

            this.spriteInstance = await spriteInstanceFactory.Checkout(asset.CreateMaterial(), sprite);
            this.graphicsLoaded = true;

            if (!Taken)
            {
                AddGraphics();
            }
        });
    }

    public void Reset()
    {
        if (!Taken)
        {
            AddGraphics();
        }
        else
        {
            DestroyGraphics();
        }
    }

    private bool Taken => persistence.Current.PlotItems.Contains(plotItem);

    public void CreatePhysics()
    {
        if (!Taken && !physicsCreated)
        {
            physicsCreated = true;
            var shape = new Box(currentScale.x, 1000, currentScale.z); //TODO: Each one creates its own, try to load from resources
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
        DestroyGraphics();
        DestroyPhysics();
        spriteInstanceFactory.TryReturn(spriteInstance);
    }

    private void AddGraphics()
    {
        if (!graphicsLoaded || Taken) { return; }

        if (!graphicsCreated)
        {
            rtInstances.AddTlasBuild(tlasData);
            rtInstances.AddShaderTableBinder(Bind);
            rtInstances.AddSprite(sprite, tlasData, spriteInstance);

            graphicsCreated = true;
        }
    }

    private void DestroyGraphics()
    {
        if (graphicsCreated)
        {
            rtInstances.RemoveSprite(sprite);
            rtInstances.RemoveShaderTableBinder(Bind);
            rtInstances.RemoveTlasBuild(tlasData);
            graphicsCreated = false;
        }
    }

    public void RequestDestruction()
    {
        this.destructionRequest.RequestDestruction();
    }

    public void SetZonePosition(in Vector3 zonePosition)
    {
        currentPosition = zonePosition + mapOffset;
        currentPosition.y += currentScale.y / 2;
        this.tlasData.Transform = new InstanceMatrix(currentPosition, currentOrientation, currentScale);
    }

    private void HandleCollision(CollisionEvent evt)
    {
        if (collidableIdentifier.TryGetIdentifier<Player>(evt.Pair.A, out var player)
         || collidableIdentifier.TryGetIdentifier<Player>(evt.Pair.B, out player))
        {
            if (!Taken)
            {
                contextMenu.HandleContext(languageService.Current.HelpBook.Greeting, Take, player.GamepadId);
            }
        }
    }

    private void HandleCollisionEnd(CollisionEvent evt)
    {
        contextMenu.ClearContext(Take);
    }

    private void Take(ContextMenuArgs args)
    {
        coroutine.RunTask((Func<Task>)(async () =>
        {
            string intro, read, takePrompt, take, text;
            switch (plotItem)
            {
                case PlotItems.GuideToPowerAndMayhem:
                    intro = languageService.Current.HelpBook.BookIntro;
                    read = languageService.Current.HelpBook.ReadBookPrompt;
                    takePrompt = languageService.Current.HelpBook.TakeBookPrompt;
                    take = languageService.Current.HelpBook.BookTaken;
                    text = null;
                    break;
                default:
                case PlotItems.GuideToPowerAndMayhemChapter4:
                case PlotItems.GuideToPowerAndMayhemChapter5:
                case PlotItems.GuideToPowerAndMayhemChapter6:
                    intro = languageService.Current.HelpBook.PageIntro;
                    read = languageService.Current.HelpBook.ReadPagePrompt;
                    if(persistence.Current.PlotItems.Contains(PlotItems.GuideToPowerAndMayhem))
                    {
                        takePrompt = languageService.Current.HelpBook.TakePagePrompt;
                        var pageCount = 0;

                        if (persistence.Current.PlotItems.Contains(PlotItems.GuideToPowerAndMayhemChapter4))
                        {
                            ++pageCount;
                        }
                        if (persistence.Current.PlotItems.Contains(PlotItems.GuideToPowerAndMayhemChapter5))
                        {
                            ++pageCount;
                        }
                        if (persistence.Current.PlotItems.Contains(PlotItems.GuideToPowerAndMayhemChapter6))
                        {
                            ++pageCount;
                        }

                        switch (pageCount)
                        {
                            case 0:
                                take = languageService.Current.HelpBook.Page1Found;
                                break;
                            case 1:
                                take = languageService.Current.HelpBook.Page2Found;
                                break;
                            default:
                                take = languageService.Current.HelpBook.AllPagesFound;
                                break;
                        }
                    }
                    else
                    {
                        take = null; 
                        takePrompt = null;
                    }

                    switch (plotItem)
                    {
                        case PlotItems.GuideToPowerAndMayhemChapter4:
                            text = languageService.Current.HelpBook.Chapter4Part2;
                            break;
                        case PlotItems.GuideToPowerAndMayhemChapter5:
                            text = languageService.Current.HelpBook.Chapter5Part2;
                            break;
                        case PlotItems.GuideToPowerAndMayhemChapter6:
                            text = languageService.Current.HelpBook.Chapter6Part2;
                            break;
                        default:
                            text = null;
                            break;
                    }

                    break;
            }

            await textDialog.ShowTextAndWait(intro, args.GamepadId);
            if(await confirmMenu.ShowAndWait(read, null, args.GamepadId))
            {
                if(text != null)
                {
                    await textDialog.ShowTextAndWait(text, args.GamepadId);
                }
                else
                {
                    helpMenu.PreviousMenu = null;
                    explorationMenu.RequestSubMenu(helpMenu, args.GamepadId);
                    await helpMenu.WaitForCurrentInput();
                }
            }

            if (takePrompt != null)
            {
                if (await confirmMenu.ShowAndWait(takePrompt, null, args.GamepadId))
                {
                    await textDialog.ShowTextAndWait(take, args.GamepadId);
                    contextMenu.ClearContext(Take);
                    persistence.Current.PlotItems.Add(plotItem);
                    DestroyGraphics();
                    DestroyPhysics();
                }
            }
            else
            {
                await textDialog.ShowTextAndWait(languageService.Current.HelpBook.CannotTakePage, args.GamepadId);
            }
        }));
    }

    private void Bind(IShaderBindingTable sbt, ITopLevelAS tlas)
    {
        spriteInstance.Bind(this.tlasData.InstanceName, sbt, tlas, sprite);
    }
}
