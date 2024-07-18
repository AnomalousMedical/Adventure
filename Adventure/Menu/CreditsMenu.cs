using Adventure.Services;
using Engine;
using Engine.Platform;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Menu;

internal class CreditsMenu
(
    ICreditsService creditsService,
    OSWindow window,
    ISharpGui sharpGui,
    IScaleHelper scaleHelper,
    IClockService clockService,
    IScreenPositioner screenPositioner
) : IExplorationSubMenu
{
    class CreditText
    {
        public SharpText Text;
        public float Age = 0.0f;
    }

    record TextInfo(SharpText Text, long NextItemDelaySeconds);

    private SharpButton back = new SharpButton() { Text = "Back" };
    private IEnumerator<TextInfo> currentCredits;
    private List<CreditText> creditsText = new List<CreditText>();
    private long nextItemTime = 0;

    public IExplorationSubMenu Previous { get; set; }

    public void Update(IExplorationMenu menu, GamepadId gamepadId)
    {
        if(currentCredits == null)
        {
            currentCredits = TextGenerator().GetEnumerator();
        }

        nextItemTime -= clockService.Clock.DeltaTimeMicro;
        if (nextItemTime < 0)
        {
            if (currentCredits.MoveNext())
            {
                nextItemTime = currentCredits.Current.NextItemDelaySeconds;
                var currentText = currentCredits.Current.Text;
                if (currentText != null)
                {
                    var desiredSize = currentText.GetDesiredSize(sharpGui);
                    currentText.Rect = new IntRect(
                        (window.WindowWidth - desiredSize.Width) / 2,
                        window.WindowHeight,
                        desiredSize.Width,
                        desiredSize.Height);

                    creditsText.Add(new CreditText { Text = currentText });
                }
            }
        }

        var pixelsPerSecond = scaleHelper.Scaled(50);

        foreach(var text in creditsText)
        {
            text.Age += clockService.Clock.DeltaSeconds;
            text.Text.Rect.Top = window.WindowHeight - (int)(pixelsPerSecond * text.Age);
            text.Text.Rect.Left = (window.WindowWidth - text.Text.Rect.Width) / 2;

            sharpGui.Text(text.Text);
        }

        //Remove text that is off the screen
        for(int i = 0; i < creditsText.Count; ++i)
        {
            if (creditsText[i].Text.Rect.Bottom < 0)
            {
                creditsText.RemoveAt(i);
                --i;
            }
            else //These are in order from top to bottom, so once one is above 0 the loop is done
            {
                break;
            }
        }

        var layout = new MarginLayout(new IntPad(scaleHelper.Scaled(10)), back);
        layout.SetRect(screenPositioner.GetBottomRightRect(layout.GetDesiredSize(sharpGui)));

        if (sharpGui.Button(back, gamepadId))
        {
            Close(menu, gamepadId);
        }
    }

    private void Close(IExplorationMenu menu, GamepadId gamepadId)
    {
        currentCredits = null;
        creditsText.Clear();
        menu.RequestSubMenu(Previous, gamepadId);
        Previous = null;
    }

    private long NormalDelayMicro = (long)(.65f * Clock.SecondsToMicro);

    private IEnumerable<TextInfo> TextGenerator()
    {
        yield return new(new SharpText("Anomalous Adventure") { Color = Color.UIWhite }, NormalDelayMicro * 2);
        yield return new(new SharpText("Created By") { Color = Color.UIWhite }, NormalDelayMicro);
        yield return new(new SharpText("Andrew Piper") { Color = Color.UIWhite }, NormalDelayMicro * 2);
        yield return new(new SharpText("https://github.com/AnomalousMedical/Adventure") { Color = Color.UIWhite }, NormalDelayMicro);
        yield return new(new SharpText("MIT License") { Color = Color.UIWhite }, NormalDelayMicro * 2);
        yield return new(new SharpText("The opinions expressed in this game are my own.") { Color = Color.UIWhite }, NormalDelayMicro);
        yield return new(new SharpText("The following people contributed works used under various licenses.") { Color = Color.UIWhite }, NormalDelayMicro);
        yield return new(new SharpText("They do not directly support or condone this game.") { Color = Color.UIWhite }, NormalDelayMicro);
        yield return new(new SharpText("However, I wish to thank them, since without thier contributions this game would not exist.") { Color = Color.UIWhite }, NormalDelayMicro * 3);

        foreach(var credit in creditsService.GetCredits())
        {
            if (credit.Title != null)
            {
                yield return new(new SharpText(credit.Title) { Color = Color.UIWhite }, NormalDelayMicro);
            }
            if (credit.Author != null)
            {
                yield return new(new SharpText(credit.Author) { Color = Color.UIWhite }, NormalDelayMicro);
            }
            if (credit.License != null)
            {
                yield return new(new SharpText(credit.License) { Color = Color.UIWhite }, NormalDelayMicro);
            }
            if (credit.Source != null)
            {
                yield return new(new SharpText(credit.Source) { Color = Color.UIWhite }, NormalDelayMicro);
            }

            yield return new(null, NormalDelayMicro * 3);
        }

        yield return new(new SharpText("Thank you for playing!") { Color = Color.UIWhite }, NormalDelayMicro);
    }
}
