using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Items
{
    record ItemTextEntry
    (
        String Text,
        String Description
    );

    record ItemText
    (
        //Required
        ItemTextEntry Unarmed
        //Accessories
        , ItemTextEntry CounterAttack
        , ItemTextEntry TargetScope
        , ItemTextEntry Healing
        , ItemTextEntry Doublecast
        , ItemTextEntry ItemUsage
        //Dagger
        , ItemTextEntry Dagger1
        , ItemTextEntry Dagger2
        , ItemTextEntry Dagger3
        //Staff
        , ItemTextEntry Staff1
        , ItemTextEntry Staff2
        , ItemTextEntry Staff3
        //Shield
        , ItemTextEntry Shield1
        , ItemTextEntry Shield2
        , ItemTextEntry Shield3
        //Spear
        , ItemTextEntry Spear1
        , ItemTextEntry Spear2
        , ItemTextEntry Spear3
        , ItemTextEntry StoreSpear1
        , ItemTextEntry StoreSpear2
        //Book
        , ItemTextEntry Book1
        , ItemTextEntry Book2
        , ItemTextEntry Book3
        //Hammers
        , ItemTextEntry Hammer1
        , ItemTextEntry Hammer2
        , ItemTextEntry Hammer3
        , ItemTextEntry StoreHammer1
        , ItemTextEntry StoreHammer2
        //Swords
        , ItemTextEntry Sword1
        , ItemTextEntry Sword2
        , ItemTextEntry Sword3
        , ItemTextEntry StoreSword1
        , ItemTextEntry StoreSword2
        //Plate
        , ItemTextEntry Plate1
        , ItemTextEntry Plate2
        , ItemTextEntry Plate3
        //Leather
        , ItemTextEntry Leather1
        , ItemTextEntry Leather2
        , ItemTextEntry Leather3
        //Cloth
        , ItemTextEntry Cloth1
        , ItemTextEntry Cloth2
        , ItemTextEntry Cloth3
        //Plot
        , ItemTextEntry RuneOfIce
        , ItemTextEntry RuneOfElectricity
        //Potions
        , ItemTextEntry Mana1
        , ItemTextEntry Mana2
        , ItemTextEntry Mana3
        , ItemTextEntry Health1
        , ItemTextEntry Health2
        , ItemTextEntry Health3
        , ItemTextEntry FerrymansBribe
        , ItemTextEntry StrengthBoost
        , ItemTextEntry MagicBoost
        , ItemTextEntry VitalityBoost
        , ItemTextEntry SpiritBoost
        , ItemTextEntry DexterityBoost
        , ItemTextEntry LuckBoost
        , ItemTextEntry LevelBoost
    )
    {
        private Dictionary<String, ItemTextEntry> itemNames = new Dictionary<String, ItemTextEntry>()
        {
            { nameof(Unarmed), Unarmed },
            { nameof(CounterAttack), CounterAttack },
            { nameof(TargetScope), TargetScope },
            { nameof(Healing), Healing },
            { nameof(Doublecast), Doublecast },
            { nameof(ItemUsage), ItemUsage },
            { nameof(Dagger1), Dagger1 },
            { nameof(Dagger2), Dagger2 },
            { nameof(Dagger3), Dagger3 },
            { nameof(Staff1), Staff1 },
            { nameof(Staff2), Staff2 },
            { nameof(Staff3), Staff3 },
            { nameof(Shield1), Shield1 },
            { nameof(Shield2), Shield2 },
            { nameof(Shield3), Shield3 },
            { nameof(Spear1), Spear1 },
            { nameof(Spear2), Spear2 },
            { nameof(Spear3), Spear3 },
            { nameof(StoreSpear1), StoreSpear1 },
            { nameof(StoreSpear2), StoreSpear2 },
            { nameof(Book1), Book1 },
            { nameof(Book2), Book2 },
            { nameof(Book3), Book3 },
            { nameof(Hammer1), Hammer1 },
            { nameof(Hammer2), Hammer2 },
            { nameof(Hammer3), Hammer3 },
            { nameof(StoreHammer1), StoreHammer1 },
            { nameof(StoreHammer2), StoreHammer2 },
            { nameof(Sword1), Sword1 },
            { nameof(Sword2), Sword2 },
            { nameof(Sword3), Sword3 },
            { nameof(StoreSword1), StoreSword1 },
            { nameof(StoreSword2), StoreSword2 },
            { nameof(Plate1), Plate1 },
            { nameof(Plate2), Plate2 },
            { nameof(Plate3), Plate3 },
            { nameof(Leather1), Leather1 },
            { nameof(Leather2), Leather2 },
            { nameof(Leather3), Leather3 },
            { nameof(Cloth1), Cloth1 },
            { nameof(Cloth2), Cloth2 },
            { nameof(Cloth3), Cloth3 },
            { nameof(RuneOfIce), RuneOfIce },
            { nameof(RuneOfElectricity), RuneOfElectricity },
            { nameof(Mana1), Mana1 },
            { nameof(Mana2), Mana2 },
            { nameof(Mana3), Mana3 },
            { nameof(Health1), Health1 },
            { nameof(Health2), Health2 },
            { nameof(Health3), Health3 },
            { nameof(FerrymansBribe), FerrymansBribe },
            { nameof(StrengthBoost), StrengthBoost },
            { nameof(MagicBoost), MagicBoost },
            { nameof(VitalityBoost), VitalityBoost },
            { nameof(SpiritBoost), SpiritBoost },
            { nameof(DexterityBoost), DexterityBoost },
            { nameof(LuckBoost), LuckBoost },
            { nameof(LevelBoost), LevelBoost },
        };

        public String GetText(String itemId)
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

        public String GetDescription(String itemId)
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
}
