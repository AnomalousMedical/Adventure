using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGui;

interface IFontManager
{
    Font Font { get; }
}

class FontManager : IFontManager
{
    private readonly SharpGuiRenderer sharpGuiRenderer;

    public FontManager(SharpGuiRenderer sharpGuiRenderer, IScaleHelper scaleHelper)
    {
        this.sharpGuiRenderer = sharpGuiRenderer;
        this.Font = sharpGuiRenderer.LoadFontTexture("Fonts/Roboto-Regular.ttf", MyGUITrueTypeFontDesc.CreateDefault(scaleHelper));
    }

    public Font Font { get; private set; }
}
