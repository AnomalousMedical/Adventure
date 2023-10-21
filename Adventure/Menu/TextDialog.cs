using Engine;
using Engine.Platform;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Text;

namespace Adventure.Menu
{
    class TextDialog : IExplorationSubMenu
    {
        private const int NumLines = 2;
        private IEnumerator<String> words;
        private string lastWord = "";

        private readonly ISharpGui sharpGui;
        private readonly IScreenPositioner screenPositioner;
        private readonly IScaleHelper scaleHelper;
        private SharpPanel panel = new SharpPanel();
        private SharpText text = new SharpText()
        {
            Color = Color.Black,
        };

        public TextDialog
        (
            ISharpGui sharpGui,
            IScreenPositioner screenPositioner,
            IScaleHelper scaleHelper
        )
        {
            this.sharpGui = sharpGui;
            this.screenPositioner = screenPositioner;
            this.scaleHelper = scaleHelper;
        }

        public void SetText(String contents)
        {
            words = FindWords(contents).GetEnumerator();
            DoSetText();
        }

        private void DoSetText()
        {
            var foundLines = 0;
            var sb = new StringBuilder(500);
            sb.Append(lastWord);
            var screenWidth = screenPositioner.ScreenSize.Width - scaleHelper.Scaled(300);
            var lineWidth = 0;
            var addSpace = false;
            while (words.MoveNext() && foundLines < NumLines)
            {
                var word = words.Current;
                var wordWidth = sharpGui.MeasureText(word).Width;
                if (lineWidth + wordWidth >= screenWidth)
                {
                    foundLines++;
                    if (foundLines < NumLines)
                    {
                        sb.Append('\n');
                        lineWidth = 0;
                    }
                    else
                    {
                        lastWord = word;
                    }
                }
                else if(addSpace)
                {
                    sb.Append(' ');
                }
                sb.Append(word);
                lineWidth += wordWidth;
                addSpace = true;
            }

            this.text.UpdateText(sb.ToString());
        }

        private IEnumerable<String> FindWords(string contents)
        {
            var contentsLength = contents.Length;
            var foundLines = 0;
            int wordStart = 0;
            for (int textPosition = 0; textPosition < contentsLength && foundLines < NumLines; ++textPosition)
            {
                var i = contents[textPosition];
                if (char.IsWhiteSpace(i))
                {
                    var word = contents.Substring(wordStart, textPosition - wordStart);
                    wordStart = textPosition + 1;
                    yield return word;
                }
            }

            if (wordStart < contentsLength)
            {
                yield return contents.Substring(wordStart, contentsLength - wordStart);
            }
        }

        public void Update(IExplorationGameState explorationGameState, IExplorationMenu menu, GamepadId gamepadId)
        {
            var layout = new PanelLayout(panel, this.text);
            layout.SetRect(screenPositioner.GetCenterTopRect(layout.GetDesiredSize(sharpGui)));

            sharpGui.Panel(panel);
            sharpGui.Text(text);
        }
    }
}
