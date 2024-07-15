using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGui;

public interface IImageManager
{
    ImageTexture Load(string filename);
}

class ImageManager(SharpGuiRenderer sharpGuiRenderer) : IImageManager
{
    public ImageTexture Load(string filename)
    {
        return sharpGuiRenderer.LoadImageTexture(filename);
    }
}
