using Adventure.Services;
using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.World;

class Torch : ISpriteAsset
{
    public ISpriteAsset CreateAnotherInstance() => new Torch();

    private const string colorMap = "Graphics/Sprites/Anomalous/World/Torch.png";
    private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
    {
        new SpriteMaterialTextureItem(0xff834d36, "Graphics/Textures/AmbientCG/Wood049_1K", "jpg"),
    };

    private static readonly SpriteMaterialDescription defaultMaterial = new SpriteMaterialDescription
    (
        colorMap: colorMap,
        materials: materials
    );

    public SpriteMaterialDescription CreateMaterial()
    {
        return defaultMaterial;
    }

    private static readonly Dictionary<string, SpriteAnimation> animations = FuncOp.Create(() =>
    {
        var animSpeed = (int)(0.7f * Clock.SecondsToMicro);

        var anims = new Dictionary<string, SpriteAnimation>()
            {
                { "default", new SpriteAnimation(animSpeed,
                    new SpriteFrame(0, 0, 1f / 3f, 1)) 
                },

                { "lit", new SpriteAnimation(animSpeed,
                    new SpriteFrame(1f / 3f, 0, 2f / 3f, 1),
                    new SpriteFrame(2f / 3f, 0f, 1, 1)) 
                },
            };

        return anims;
    });

    public ISprite CreateSprite()
    {
        return new Sprite(animations) { BaseScale = new Vector3(9f / 24f * 0.65f, 0.65f, 0.1f) };
    }
}
