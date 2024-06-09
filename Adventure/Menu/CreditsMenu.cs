using Adventure.Services;
using Engine.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Menu;

internal class CreditsMenu
(
    ICreditsService creditsService
) : IExplorationSubMenu
{
    public void Update(IExplorationGameState explorationGameState, IExplorationMenu menu, GamepadId gamepadId)
    {
        foreach(var credit in creditsService.GetCredits())
        {
            Console.WriteLine(credit);
        }
        menu.RequestSubMenu(null, gamepadId);
    }
}
