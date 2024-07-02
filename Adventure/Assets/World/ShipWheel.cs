using Adventure.Services;
using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.World;

class ShipWheel : ISpriteAsset
{
    public ISpriteAsset CreateAnotherInstance() => new ShipWheel();

    private const string colorMap = "Graphics/Sprites/Anomalous/World/ShipWheel.png";
    private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
    {
        new SpriteMaterialTextureItem(0xff903c18, "Graphics/Textures/AmbientCG/Wood049_1K", "jpg"),
        new SpriteMaterialTextureItem(0xff787878, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
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
                    new SpriteFrame(0, 0, 1, 1))
                },
            };

        return anims;
    });

    public ISprite CreateSprite()
    {
        return new Sprite(animations) { BaseScale = new Vector3(33f / 34f * 0.65f, 0.65f, 0.1f) };
    }
}
