using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Items
{
    record ItemText
    (
        //Required
        String Unarmed
        //Accessories
        , String CounterAttack
        , String TargetScope
        , String Healing
        , String Doublecast
        , String ItemUsage
        //Dagger
        , String Dagger1
        , String Dagger2
        , String Dagger3
        //Staff
        , String Staff1
        , String Staff2
        , String Staff3
        //Shield
        , String Shield1
        , String Shield2
        , String Shield3
        //Spear
        , String Spear1
        , String Spear2
        , String Spear3
        , String StoreSpear1
        , String StoreSpear2
        //Book
        , String Book1
        , String Book2
        , String Book3
        //Hammers
        , String Hammer1
        , String Hammer2
        , String Hammer3
        , String StoreHammer1
        , String StoreHammer2
        //Swords
        , String Sword1
        , String Sword2
        , String Sword3
        , String StoreSword1
        , String StoreSword2
        //Plate
        , String Plate1
        , String Plate2
        , String Plate3
        //Leather
        , String Leather1
        , String Leather2
        , String Leather3
        //Cloth
        , String Cloth1
        , String Cloth2
        , String Cloth3
        //Plot
        , String RuneOfIce
        , String RuneOfElectricity
        //Potions
        , String Mana1
        , String Mana2
        , String Mana3
        , String Health1
        , String Health2
        , String Health3
        , String FerrymansBribe
        , String StrengthBoost
        , String MagicBoost
        , String VitalityBoost
        , String SpiritBoost
        , String DexterityBoost
        , String LuckBoost
        , String LevelBoost
    )
    {
        private Dictionary<String, String> itemNames = new Dictionary<String, String>()
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
            if (itemNames.TryGetValue(itemId, out var text))
            {
                return text;
            }
            else
            {
                return itemId + "_MissingID";
            }
        }
    }
}
