using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Services;

//Levels: 17, 29, 36, 44, 51, 56, 61
//Bonus levels: up to +3 3 times so 64, 67, 70 and 73
//17 - scout
//29 - hunter
//36 - pilot
//44 - adventurer
//51 - world traveler
//56 - slayer
//61 - dragon slayer
//64 - reaper
//67 - lesser god
//70 - bounds breaker
//73 - the one

internal class LevelText
(
    string l17,
    string l29,
    string l36,
    string l44,
    string l51,
    string l56,
    string l61,
    string l64,
    string l67,
    string l70,
    string l73,
    string l77
)
{
    public string GetText(long level)
    {
        if(level < 17 + 1)
        {
            return l17;
        }

        if (level < 29 + 1)
        {
            return l29;
        }

        if (level < 36 + 1)
        {
            return l36;
        }

        if (level < 44 + 1)
        {
            return l44;
        }

        if (level < 51 + 1)
        {
            return l51;
        }

        if (level < 56 + 1)
        {
            return l56;
        }

        if (level < 61 + 1)
        {
            return l61;
        }

        if (level < 64 + 1)
        {
            return l64;
        }

        if (level < 67 + 1)
        {
            return l67;
        }

        if (level < 70 + 1)
        {
            return l70;
        }

        if (level < 73 + 1)
        {
            return l73;
        }

        return l77;
    }
}
