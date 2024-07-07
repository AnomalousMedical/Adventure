using Adventure.Services;
using Adventure.WorldMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Items;

record PlotItemText
(
    ItemTextEntry AirshipFuel,
    ItemTextEntry AirshipWheel,
    ItemTextEntry AirshipKey,
    ItemTextEntry BlacksmithUpgrade,
    ItemTextEntry AlchemistUpgrade,
    ItemTextEntry RuneOfFire,
    ItemTextEntry RuneOfIce,
    ItemTextEntry RuneOfElectricity,
    ItemTextEntry ElementalStone,
    ItemTextEntry GuideToPowerAndMayhem,
    ItemTextEntry GuideToPowerAndMayhemChapter4,
    ItemTextEntry GuideToPowerAndMayhemChapter5,
    ItemTextEntry GuideToPowerAndMayhemChapter6
)
{
    private Dictionary<PlotItems, ItemTextEntry> itemNames = new Dictionary<PlotItems, ItemTextEntry>()
    {
        { PlotItems.AirshipFuel, AirshipFuel },
        { PlotItems.AirshipWheel, AirshipWheel },
        { PlotItems.AirshipKey, AirshipKey },
        { PlotItems.BlacksmithUpgrade, BlacksmithUpgrade },
        { PlotItems.AlchemistUpgrade, AlchemistUpgrade },
        { PlotItems.RuneOfFire, RuneOfFire },
        { PlotItems.RuneOfIce, RuneOfIce },
        { PlotItems.RuneOfElectricity, RuneOfElectricity },
        { PlotItems.ElementalStone, ElementalStone },
        { PlotItems.GuideToPowerAndMayhem, GuideToPowerAndMayhem },
        { PlotItems.GuideToPowerAndMayhemChapter4, GuideToPowerAndMayhemChapter4 },
        { PlotItems.GuideToPowerAndMayhemChapter5, GuideToPowerAndMayhemChapter5 },
        { PlotItems.GuideToPowerAndMayhemChapter6, GuideToPowerAndMayhemChapter6 },        
    };

    public String GetText(PlotItems itemId)
    {
        if (itemNames.TryGetValue(itemId, out var entry))
        {
            return entry.Text;
        }
        else
        {
            return itemId + "_MissingID";
        }
    }

    public String GetDescription(PlotItems itemId)
    {
        if (itemNames.TryGetValue(itemId, out var entry))
        {
            return entry.Description;
        }
        else
        {
            return itemId + "_MissingID";
        }
    }
}
