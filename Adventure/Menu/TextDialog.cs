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
        private string lastWord = String.Empty;

        private readonly ISharpGui sharpGui;
        private readonly IScreenPositioner screenPositioner;
        private readonly IScaleHelper scaleHelper;
        private SharpPanel panel = new SharpPanel();
        private SharpText text = new SharpText()
        {
            Color = Color.Black,
        };

        private SharpButton nextButton = new SharpButton()
        {
            Text = "Next"
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
            UpdateText();
        }

        private bool UpdateText()
        {
            var screenWidth = Math.Min(screenPositioner.ScreenSize.Width - scaleHelper.Scaled(300), scaleHelper.Scaled(700));
            var lineWidth = 0;
            var foundLines = 0;
            var sb = new StringBuilder(500);

            var addSpace = lastWord != String.Empty;
            if (addSpace)
            {
                sb.Append(lastWord);
                lastWord = String.Empty;
            }

            while (words.MoveNext())
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
                        addSpace = false;
                    }
                    else
                    {
                        lastWord = word;
                        break;
                    }
                }

                if (addSpace)
                {
                    sb.Append(' ');
                }
                sb.Append(word);
                lineWidth += wordWidth;
                addSpace = true;
            }

            this.text.UpdateText(sb.ToString());

            return sb.Length > 0;
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
            {
                var layout = new MarginLayout(new IntPad(0, scaleHelper.Scaled(10), 0, 0), new PanelLayout(panel, this.text));
                layout.SetRect(screenPositioner.GetCenterTopRect(layout.GetDesiredSize(sharpGui)));
            }

            {
                var layout = new MarginLayout(new IntPad(scaleHelper.Scaled(10)), nextButton);
                layout.SetRect(screenPositioner.GetBottomRightRect(layout.GetDesiredSize(sharpGui)));
            }

            sharpGui.Panel(panel);
            sharpGui.Text(text);

            if (sharpGui.Button(nextButton) || sharpGui.IsStandardNextPressed(gamepadId))
            {
                if (!UpdateText())
                {
                    menu.RequestSubMenu(null, gamepadId);
                }
            }
        }
    }
}
