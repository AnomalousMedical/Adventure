using Engine;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Services;

class FontLoader(IFontManager fontManager, IScaleHelper scaleHelper)
{
    private Font titleFont;
    public Font TitleFont
    {
        get
        {
            if(titleFont == null)
            {
                var fontDesc = MyGUITrueTypeFontDesc.CreateDefault(scaleHelper);
                fontDesc.size = 60;
                titleFont = fontManager.LoadFont("Fonts/metal-macabre.regular.ttf", fontDesc);
            }
            return titleFont;
        }
    }
}
