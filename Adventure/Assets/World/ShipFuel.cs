using Adventure.Services;
using DiligentEngine.RT.Sprites;
using Engine;
using Engine.Platform;
using System.Collections.Generic;

namespace Adventure.Assets.World;

class ShipFuel : ISpriteAsset
{
    public ISpriteAsset CreateAnotherInstance() => new ShipFuel();

    private const string colorMap = "Graphics/Sprites/Anomalous/World/ShipFuel.png";
    private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
    {
        new SpriteMaterialTextureItem(0xffd95842, "Graphics/Textures/AmbientCG/Metal032_1K", "jpg", reflective: true),
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
                { "default", new SpriteAnimation(animSpeed, new SpriteFrame(0, 0, 1, 1)
                {
                    Attachments = new List<SpriteFrameAttachment>()
                    {
                        new SpriteFrameAttachment(new Vector3(0f, 0.5f, -0.1f)),
                    }
                })
                },
            };

        return anims;
    });

    public ISprite CreateSprite()
    {
        return new Sprite(animations) { BaseScale = new Vector3(22f / 32f * 0.65f, 0.65f, 0.1f) };
    }

    public Light CreateLight()
    {
        return new Light()
        {
            Color = Color.FromARGB(0xffce7f18),
            Length = 2.3f,
        };
    }

    public int? LightAttachmentChannel => 0;
}
