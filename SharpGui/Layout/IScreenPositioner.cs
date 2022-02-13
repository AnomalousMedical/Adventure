using Engine;

namespace SharpGui
{
    public interface IScreenPositioner
    {
        IntSize2 ScreenSize { get; }

        IntRect GetBottomLeftRect(in IntSize2 size);
        IntRect GetBottomRightRect(in IntSize2 size);
        IntRect GetTopLeftRect(in IntSize2 size);
        IntRect GetTopRightRect(in IntSize2 size);
        IntRect GetCenterRect(in IntSize2 size);
        IntRect GetCenterTopRect(in IntSize2 size);
    }
}