using DiligentEngine.RT.Sprites;
using Engine;
using System.Collections.Generic;

namespace Adventure.Assets.World;

class Signpost : ISpriteAsset
{
    private const string colorMap = "Graphics/Sprites/Anomalous/World/Signpost.png";
    private static readonly HashSet<SpriteMaterialTextureItem> materials = new HashSet<SpriteMaterialTextureItem>
    {
        new SpriteMaterialTextureItem(0xff87765e, "Graphics/Textures/AmbientCG/Wood049_1K", "jpg"),
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

    public ISprite CreateSprite()
    {
        return new Sprite() { BaseScale = new Vector3(1f, 1f, 1f) };
    }
}
