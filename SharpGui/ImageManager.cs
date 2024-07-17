using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGui;

public interface IImageManager
{
    ImageTexture Load(string filename, bool isSrgb);
}

class ImageManager(SharpGuiRenderer sharpGuiRenderer) : IImageManager
{
    public ImageTexture Load(string filename, bool isSrgb)
    {
        return sharpGuiRenderer.LoadImageTexture(filename, isSrgb);
    }
}
