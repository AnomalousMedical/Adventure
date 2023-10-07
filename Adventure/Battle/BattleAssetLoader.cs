﻿using Adventure.Assets;
using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Adventure.Battle;

internal class BattleAssetLoader : IDisposable
{
    public ISpriteAsset NormalHit { get; } = new Assets.PixelEffects.NormalHitEffect();
    public ISpriteAsset CriticalHit { get; } = new Assets.PixelEffects.CriticalHitEffect();
    public ISpriteAsset BlockedHit { get; } = new Assets.PixelEffects.BlockedHitEffect();
    public ISpriteAsset IonicShread { get; } = new Assets.PixelEffects.IonicShreadEffect();
    public ISpriteAsset Fire { get; } = new Assets.PixelEffects.FireEffect();
    public ISpriteAsset Ice { get; } = new Assets.PixelEffects.IceEffect();
    public ISpriteAsset Buff { get; } = new Assets.PixelEffects.BuffEffect();
    public ISpriteAsset Electric { get; } = new Assets.PixelEffects.ElectricEffect();

    private readonly SpriteInstanceFactory spriteInstanceFactory;

    private List<SpriteInstance> spriteInstances = new List<SpriteInstance>();

    public BattleAssetLoader(SpriteInstanceFactory spriteInstanceFactory, ICoroutineRunner coroutineRunner, IDestructionRequest destructionRequest)
    {
        this.spriteInstanceFactory = spriteInstanceFactory;

        coroutineRunner.RunTask(async () =>
        {
            using var destructionBlock = destructionRequest.BlockDestruction(); //Block destruction until task is finished and this is disposed.

            var spriteLoadTasks = new List<Task<SpriteInstance>>()
            {
                this.spriteInstanceFactory.Checkout(NormalHit.CreateMaterial(), NormalHit.CreateSprite()),
                this.spriteInstanceFactory.Checkout(CriticalHit.CreateMaterial(), CriticalHit.CreateSprite()),
                this.spriteInstanceFactory.Checkout(BlockedHit.CreateMaterial(), BlockedHit.CreateSprite()),
                this.spriteInstanceFactory.Checkout(IonicShread.CreateMaterial(), IonicShread.CreateSprite()),
                this.spriteInstanceFactory.Checkout(Fire.CreateMaterial(), Fire.CreateSprite()),
                this.spriteInstanceFactory.Checkout(Ice.CreateMaterial(), Ice.CreateSprite()),
                this.spriteInstanceFactory.Checkout(Electric.CreateMaterial(), Electric.CreateSprite()),
                this.spriteInstanceFactory.Checkout(Buff.CreateMaterial(), Buff.CreateSprite()),
            };

            foreach(var task in spriteLoadTasks)
            {
                spriteInstances.Add(await task);
            }
        });
    }

    public void Dispose()
    {
        foreach(var sprite in spriteInstances)
        {
            spriteInstanceFactory.TryReturn(sprite);
        }
    }
}
