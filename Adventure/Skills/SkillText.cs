using Adventure.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Skills;

record SkillTextEntry
(
    String Text,
    String Description
);

internal class SkillText
(
    SkillTextEntry Cure
    , SkillTextEntry MegaCure
    , SkillTextEntry UltraCure
    , SkillTextEntry Reanimate
    , SkillTextEntry Steal
    , SkillTextEntry Fire
    , SkillTextEntry StrongFire
    , SkillTextEntry ArchFire
    , SkillTextEntry Ice
    , SkillTextEntry StrongIce
    , SkillTextEntry ArchIce
    , SkillTextEntry Lightning
    , SkillTextEntry StrongLightning
    , SkillTextEntry ArchLightning
    , SkillTextEntry IonShread
    , SkillTextEntry BattleCry
    , SkillTextEntry WarCry
    , SkillTextEntry Focus
    , SkillTextEntry IntenseFocus
    , SkillTextEntry Haste
)
{
    private Dictionary<String, SkillTextEntry> skillNames = new Dictionary<String, SkillTextEntry>()
    {
        { "Cure", Cure },
        { "MegaCure", MegaCure },
        { "UltraCure", UltraCure },
        { "Reanimate", Reanimate },
        { "Steal", Steal },
        { "Fire", Fire },
        { "StrongFire", StrongFire },
        { "ArchFire", ArchFire },
        { "Ice", Ice },
        { "StrongIce", StrongIce },
        { "ArchIce", ArchIce },
        { "Lightning", Lightning },
        { "StrongLightning", StrongLightning },
        { "ArchLightning", ArchLightning },
        { "IonShread", IonShread },
        { "BattleCry", BattleCry },
        { "WarCry", WarCry },
        { "Focus", Focus },
        { "IntenseFocus", IntenseFocus },
        { "Haste", Haste  },
    };

    public String GetText(String skillId)
    {
        if (skillNames.TryGetValue(skillId, out var entry))
        {
            return entry.Text;
        }
        else
        {
            return skillId + "_MissingID";
        }
    }

    public String GetDescription(String skillId)
    {
        if (skillNames.TryGetValue(skillId, out var entry))
        {
            return entry.Description;
        }
        else
        {
            return skillId + "_MissingID";
        }
    }
}
