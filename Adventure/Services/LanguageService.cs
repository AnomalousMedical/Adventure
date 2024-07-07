using Adventure.Exploration;
using Adventure.Items;
using Adventure.Skills;
using Adventure.WorldMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Services
{
    interface ILanguageService
    {
        Language Current { get; }
    }

    class LanguageService : ILanguageService
    {
        public LanguageService(Language current)
        {
            this.Current = current;
        }

        public Language Current { get; init; }
    }

    record Language
    (
        WorldDatabase.Text WorldDatabase,
        //World
        Airship.Text Airship,
        Alchemist.Text Alchemist,
        AlchemistUpgrade.Text AlchemistUpgrade,
        Blacksmith.Text Blacksmith,
        AirshipEngineer.Text AirshipEngineer,
        BlacksmithUpgrade.Text BlacksmithUpgrade,
        ItemStorage.Text ItemStorage,
        ElementalStone.Text ElementalStone,
        FortuneTeller.Text FortuneTeller,
        Innkeeper.Text Innkeeper,
        ZoneEntrance.Text ZoneEntrance,
        //Items
        ItemText Items,
        PlotItemText PlotItems,
        SkillText Skills,
        //Zones
        PartyMemberTrigger.Text PartyMemberTrigger,
        HelpBook.Text HelpBook
    );
}
