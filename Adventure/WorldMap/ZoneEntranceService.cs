using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.WorldMap;

class ZoneEntranceService
{
    private List<ZoneEntrance> zoneEntrances = new List<ZoneEntrance>(10);

    public void Add(ZoneEntrance zoneEntrance)
    {
        zoneEntrances.Add(zoneEntrance);
    }

    public void Remove(ZoneEntrance zoneEntrance)
    {
        zoneEntrances.Remove(zoneEntrance);
    }

    public void UpdateDisplay()
    {
        foreach(var entrance in zoneEntrances)
        {
            entrance.UpdateDisplay();
        }
    }
}
