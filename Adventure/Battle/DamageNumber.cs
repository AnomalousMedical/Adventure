using Engine;
using Engine.Platform;
using SharpGui;

namespace Adventure.Battle
{
    class DamageNumber
    {
        public DamageNumber(string number, in Vector2 position, IScaleHelper scaleHelper, Color color, long timeRemaining = (long)(0.9f * Clock.SecondsToMicro))
        {
            UpdatePosition(position, scaleHelper);
            TimeRemaining = timeRemaining;
            HalfDuration = timeRemaining / 2;
            this.Text = new SharpText(number.ToString()) { Rect = new IntRect(0, 0, 10000, 10000), Color = color };
            UpdatePosition();
        }

        public void UpdatePosition()
        {
            Vector2 position;
            if (TimeRemaining > HalfDuration)
            {
                position = StartPosition.lerp(EndPosition, 1f - (float)TimeRemaining / HalfDuration);
            }
            else
            {
                position = EndPosition.lerp(StartPosition, 1f - (float)(TimeRemaining - HalfDuration) / HalfDuration);
            }
            this.Text.Rect.Left = (int)position.x;
            this.Text.Rect.Top = (int)position.y;
        }

        public void UpdatePosition(in Vector2 position, IScaleHelper scaleHelper)
        {
            this.StartPosition = position;
            this.EndPosition = position + new Vector2(0, scaleHelper.Scaled(-15));
        }

        public long HalfDuration { get; }

        public long TimeRemaining { get; set; }

        public Vector2 EndPosition { get; private set; }

        public Vector2 StartPosition { get; private set; }

        public SharpText Text { get; }
    }
}
