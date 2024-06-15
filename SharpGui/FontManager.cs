using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGui;

interface IFontManager
{
    Font FontOrDefault(Font font);
}

class FontManager : IFontManager
{
    private readonly SharpGuiRenderer sharpGuiRenderer;
    private readonly IScaleHelper scaleHelper;
    private readonly SharpGuiOptions sharpGuiOptions;
    private Font defaultFont;

    public FontManager(SharpGuiRenderer sharpGuiRenderer, IScaleHelper scaleHelper, SharpGuiOptions sharpGuiOptions)
    {
        this.sharpGuiRenderer = sharpGuiRenderer;
        this.scaleHelper = scaleHelper;
        this.sharpGuiOptions = sharpGuiOptions;
    }

    public Font FontOrDefault(Font font)
    {
        if(font != null)
        {
            return font;
        }
        
        return EnsureDefaultFont();
    }

    private Font EnsureDefaultFont()
    {
        if(this.defaultFont == null)
        {
            this.defaultFont = sharpGuiRenderer.LoadFontTexture(sharpGuiOptions.DefaultFont, MyGUITrueTypeFontDesc.CreateDefault(scaleHelper));
        }

        return this.defaultFont;
    }
}
