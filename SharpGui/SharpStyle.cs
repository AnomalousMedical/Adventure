using Engine;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpGui
{
    public class SharpStyle
    {
        public SharpLook Normal { get; set; } = new SharpLook();

        public SharpLook Hover { get; set; } = new SharpLook();

        public SharpLook Active { get; set; } = new SharpLook();

        public SharpLook HoverAndActive { get; set; } = new SharpLook();

        public SharpLook Focus { get; set; } = new SharpLook();

        public SharpLook HoverAndFocus { get; set; } = new SharpLook();

        public SharpLook HoverAndActiveAndFocus { get; set; } = new SharpLook();

        public Color Background
        {
            set
            {
                Normal.Background = value;
                Hover.Background = value;
                Active.Background = value;
                HoverAndActive.Background = value;
                Focus.Background = value;
                HoverAndFocus.Background = value;
                HoverAndActiveAndFocus.Background = value;
            }
        }
        public Color Color
        {
            set
            {
                Normal.Color = value;
                Hover.Color = value;
                Active.Color = value;
                HoverAndActive.Color = value;
                Focus.Color = value;
                HoverAndFocus.Color = value;
                HoverAndActiveAndFocus.Color = value;
            }
        }
        public Color ShadowColor
        {
            set
            {
                Normal.ShadowColor = value;
                Hover.ShadowColor = value;
                Active.ShadowColor = value;
                HoverAndActive.ShadowColor = value;
                Focus.ShadowColor = value;
                HoverAndFocus.ShadowColor = value;
                HoverAndActiveAndFocus.ShadowColor = value;
            }
        }
        public IntVector2 ShadowOffset
        {
            set
            {
                Normal.ShadowOffset = value;
                Hover.ShadowOffset = value;
                Active.ShadowOffset = value;
                HoverAndActive.ShadowOffset = value;
                Focus.ShadowOffset = value;
                HoverAndFocus.ShadowOffset = value;
                HoverAndActiveAndFocus.ShadowOffset = value;
            }
        }
        public Color BorderColor
        {
            set
            {
                Normal.BorderColor = value;
                Hover.BorderColor = value;
                Active.BorderColor = value;
                HoverAndActive.BorderColor = value;
                Focus.BorderColor = value;
                HoverAndFocus.BorderColor = value;
                HoverAndActiveAndFocus.BorderColor = value;
            }
        }
        public IntPad Border
        {
            set
            {
                Normal.Border = value;
                Hover.Border = value;
                Active.Border = value;
                HoverAndActive.Border = value;
                Focus.Border = value;
                HoverAndFocus.Border = value;
                HoverAndActiveAndFocus.Border = value;
            }
        }
        public IntPad Padding
        {
            set
            {
                Normal.Padding = value;
                Hover.Padding = value;
                Active.Padding = value;
                HoverAndActive.Padding = value;
                Focus.Padding = value;
                HoverAndFocus.Padding = value;
                HoverAndActiveAndFocus.Padding = value;
            }
        }

        public static SharpStyle CreateComplete(IScaleHelper scaleHelper, OpenColorSpec spec)
        {
            var style = new SharpStyle()
            {
                Background = Color.FromARGB(0xff202429).ToSrgb(),
                Color = Color.UIWhite.ToSrgb(),
                BorderColor = Color.FromARGB(0xff181b1e).ToSrgb(),
                ShadowColor = Color.FromARGB(0x80000000).ToSrgb(),
                ShadowOffset = scaleHelper.Scaled(new IntVector2(6, 6)),
                Padding = scaleHelper.Scaled(new IntPad(19)),
                Border = scaleHelper.Scaled(new IntPad(2)),
                Hover =
                {
                    Background = spec.Colors[9],
                },
                Active =
                {
                    Background = spec.Colors[8],
                },
                HoverAndActive =
                {
                    Background = spec.Colors[7],
                },
                Focus =
                {
                    Background = spec.Colors[8],
                    BorderColor = spec.Colors[9],
                },
                HoverAndFocus =
                {
                    Background = spec.Colors[7],
                    BorderColor = spec.Colors[9],
                },
                HoverAndActiveAndFocus =
                {
                    Background = spec.Colors[6],
                    BorderColor = spec.Colors[9],
                }
            };
            return style;
        }
    }
}
