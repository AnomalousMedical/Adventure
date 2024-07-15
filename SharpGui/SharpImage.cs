using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGui;

public class SharpImage : ILayoutItem
{
    public SharpImage() { }

    public SharpImage(ImageTexture image)
    {
        Image = image;
    }

    public ImageTexture Image { get; set; }

    public Rect UvRect { get; set; } = new Rect(0f, 0f, 1f, 1f);

    public float Layer { get; set; }

    public Color Color { get; set; } = Color.White;

    public IntRect Rect;

    public int? DesiredWidth { get; set; }

    public int? DesiredHeight { get; set; }

    public IntSize2 GetDesiredSize(ISharpGui sharpGui)
    {
        if(Image == null)
        {
            return new IntSize2(0, 0);
        }
        if(DesiredWidth == null && DesiredHeight == null) 
        {
            return new IntSize2(Image.Width, Image.Height);
        }
        if(DesiredWidth != null && DesiredHeight == null)
        {
            return new IntSize2(DesiredWidth.Value, (int)(Image.AspectRatio * DesiredWidth.Value));
        }
        if (DesiredWidth == null && DesiredHeight != null)
        {
            return new IntSize2((int)((1f / Image.AspectRatio) * DesiredHeight.Value), DesiredHeight.Value);
        }

        return new IntSize2(DesiredWidth.Value, DesiredHeight.Value);
    }

    public void SetRect(in IntRect rect)
    {
        this.Rect = rect;
    }
}
