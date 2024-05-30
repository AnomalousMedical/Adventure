using Engine;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Services;

class CharacterStyleService(IScaleHelper scaleHelper)
{
    List<SharpStyle> characterStyles = new ()
    {
        SharpStyle.CreateComplete(scaleHelper, OpenColor.Blue),
        SharpStyle.CreateComplete(scaleHelper, OpenColor.Grape),
        SharpStyle.CreateComplete(scaleHelper, OpenColor.Green),
        SharpStyle.CreateComplete(scaleHelper, OpenColor.Orange),
    };

    List<Color> nameHighlightColors = new()
    {
        OpenColor.Blue.Colors[7],
        OpenColor.Grape.Colors[7],
        OpenColor.Green.Colors[7],
        OpenColor.Orange.Colors[7],
    };

    public SharpStyle GetCharacterStyle(int index)
    {
        if(index < characterStyles.Count)
        {
            return characterStyles[index];
        }
        return characterStyles[0];
    }

    public Color GetNameHighlightColor(int index)
    {
        if (index < nameHighlightColors.Count)
        {
            return nameHighlightColors[index];
        }
        return nameHighlightColors[0];
    }
}
