using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGui;

public class ImageTexture
{
    private readonly uint textureIndex;

    internal uint TextureIndex => textureIndex;

    public ImageTexture(uint textureIndex, int width, int height)
    {
        this.textureIndex = textureIndex;
        this.AspectRatio = (float)width / height;
        this.Width = width;
        this.Height = height;
    }

    public float AspectRatio { get; init; }

    public int Width { get; init; }

    public int Height { get; init; }
}
