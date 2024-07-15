using Engine;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Services;

class IconLoader(IImageManager imageManager)
{
    private ImageTexture icons;
    public ImageTexture Icons
    {
        get
        {
            if(icons == null)
            {
                icons = imageManager.Load("Graphics/Icons/Icons.png");
            }
            return icons;
        }
    }

    private const int IconWidth = 64;
    private const int IconHeight = 64;
    private const float ImageWidth = 256f;
    private const float ImageHeight = 256f;
    private const float IconSizeU = IconWidth / ImageWidth;
    private const float IconSizeV = IconHeight / ImageHeight;

    static Rect CreateRect(int x, int y)
    {
        return new Rect(x * IconWidth / ImageWidth, y * IconHeight / ImageHeight, IconSizeU, IconSizeV);
    }

    public Rect Fire { get; set; } = CreateRect(3, 0);

    public Rect Ice { get; set; } = CreateRect(1, 0);

    public Rect Electricity { get; set; } = CreateRect(2, 0);

    public Rect Slashing { get; set; } = CreateRect(1, 1);

    public Rect Bludgeoning { get; set; } = CreateRect(0, 2);

    public Rect Piercing { get; set; } = CreateRect(0, 3);

    public Rect PhysBuff { get; set; } = CreateRect(0, 1);

    public Rect MentalBuff { get; set; } = CreateRect(2, 1);

    public Rect Haste { get; set; } = CreateRect(0, 0);
}
