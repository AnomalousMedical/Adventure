using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGui;

public interface IFontManager
{
    Font FontOrDefault(Font font);

    Font LoadFont(String fileName);

    Font LoadFont(String fileName, in MyGUITrueTypeFontDesc fontDesc);
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

    public Font LoadFont(String fileName)
    {
        return LoadFont(fileName, MyGUITrueTypeFontDesc.CreateDefault(scaleHelper));
    }

    public Font LoadFont(String fileName, in MyGUITrueTypeFontDesc fontDesc)
    {
        return sharpGuiRenderer.LoadFontTexture(fileName, fontDesc);
    }

    private Font EnsureDefaultFont()
    {
        if(this.defaultFont == null)
        {
            this.defaultFont = LoadFont(sharpGuiOptions.DefaultFont);
        }

        return this.defaultFont;
    }
}
