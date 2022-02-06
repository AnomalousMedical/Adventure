using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure
{
    interface ITreasure
    {
        string InfoText { get; }
    }

    class Treasure : ITreasure
    {
        public string InfoText { get; }

        public Treasure(string infoText)
        {
            this.InfoText = infoText;
        }
    }
}
