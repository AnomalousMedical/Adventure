using Adventure.Items;
using Adventure.Items.Creators;
using Adventure.Services;
using Adventure.WorldMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Text
{
    static class EnglishLanguage
    {
        public static Language Create()
        {
            return new Language
            (
                WorldDatabase: new WorldDatabase.Text
                (
                    FighterName: "Bob",
                    FighterGreeting: "My shield will guard us.",
                    MageName: "Magic Joe",
                    MageGreeting: "Let's get moving.",
                    ThiefName: "Stabby McStabface",
                    ThiefGreeting: "I hope we find lots of great treasure!",
                    ClericName: "Wendy",
                    ClericGreeting: "I wonder what's made everything so agressive?"
                ),
                //World Map
                Airship: new Airship.Text
                (
                    Broken: "Broken",
                    TakeOff: "Take off",
                    Land: "Land"
                ),
                Alchemist: new Alchemist.Text
                (
                    Greeting: "Hello",
                    SalesPitch: "My potions will keep you going.",
                    AncientSalesPitch: "My potions will really pack a punch now."
                ),
                AlchemistUpgrade: new AlchemistUpgrade.Text
                (
                    Check: "Check",
                    GiveUpgrade: "The gargoyle shows you ancient alchemical techniques..."
                ),
                Blacksmith: new Blacksmith.Text
                (
                    Greeting: "Hello",
                    SalesPitch: "I have the best weapons around.",
                    AncientSalesPitch: "With this knowledge I can make my weapons even better."
                ),
                BlacksmithUpgrade: new BlacksmithUpgrade.Text
                (
                    Check: "Check",
                    GiveUpgrade: "The gargoyle whispers ancient blacksmithing secrets into your ear..."
                ),
                ElementalStone: new ElementalStone.Text
                (
                    Check: "Check",
                    ProveMastery: "The stone reads: \"Prove your mastery over the elements and you will be rewarded with great power.\"",
                    ProvenWorthy: "A voice booms in your ear: \"You have proven worthy!\"."
                ),
                FortuneTeller: new FortuneTeller.Text
                (
                    Greeting: "Hello",
                    StartShufflePitch: "Lets see what the cards say about your fate.",
                    CardShuffleNarrator: "The fortune teller shuffles her deck of cards...",
                    ShowResultsNarrator: "The cards are arranged before you on the table.",
                    NoResultsNarrator: "The cards are quiet today."
                ),
                Innkeeper: new Innkeeper.Text
                (
                    Greeting: "Hello",
                    Sleep: "Have a good rest."
                ),
                ZoneEntrance: new ZoneEntrance.Text
                (
                    Enter: "Enter",
                    EnterCompleted: "Enter - Completed"
                ),
                //Items
                Items: new ItemText
                (
                      Unarmed: "Unarmed"
                    , CounterAttack: "Gauntlets of Revenge"
                    , TargetScope: "Target Scope"
                    , Healing: "Ring of Healing"
                    , Doublecast: "Elemental Amplifier"
                    , ItemUsage: "Gloves of Handling"
                    , Dagger1: "Dagger"
                    , Dagger2: "Assassin's Blade"
                    , Dagger3: "Dagger of Shifting Sands"
                    , Staff1: "Cracked Staff"
                    , Staff2: "Mage's Staff"
                    , Staff3: "Arch Mage's Staff"
                    , Shield1: "Buckler"
                    , Shield2: "Common Shield"
                    , Shield3: "Glowing Shield"
                    , Spear1: "Rusty Spear"
                    , Spear2: "Common Spear"
                    , Spear3: "Runic Spear"
                    , StoreSpear1: "Smithed Spear"
                    , StoreSpear2: "Ancient Spear"
                    , Book1: "Torn Book of Healing"
                    , Book2: "Book of Healing"
                    , Book3: "Book of Forbidden Healing"
                    , Hammer1: "Tiny Hammer"
                    , Hammer2: "Giant's Hammer"
                    , Hammer3: "Hammer of the Gods"
                    , StoreHammer1: "Smithed Hammer"
                    , StoreHammer2: "Ancient Hammer"
                    , Sword1: "Busted Sword"
                    , Sword2: "Common Sword"
                    , Sword3: "Ultimate Sword"
                    , StoreSword1: "Smithed Sword"
                    , StoreSword2: "Ancient Sword"
                    , Plate1: "Tarnished Plate Armor"
                    , Plate2: "Iron Plate Armor"
                    , Plate3: "Steel Plate Armor"
                    , Leather1: "Cracked Leather Armor"
                    , Leather2: "Common Leather Armor"
                    , Leather3: "Fine Leather Armor"
                    , Cloth1: "Linen Cloth Armor"
                    , Cloth2: "Common Cloth Armor"
                    , Cloth3: "Silk Cloth Armor"
                    , RuneOfIce: "Rune of Ice"
                    , RuneOfElectricity: "Rune of Electricity"
                    , Mana1: "Mana Potion"
                    , Mana2: "Big Mana Potion"
                    , Mana3: "Giant Mana Potion"
                    , Health1: "Health Potion"
                    , Health2: "Big Health Potion"
                    , Health3: "Giant Health Potion"
                    , FerrymansBribe: "Ferryman's Bribe"
                    , StrengthBoost: "Strength Boost"
                    , MagicBoost: "Magic Boost"
                    , VitalityBoost: "Vitality Boost"
                    , SpiritBoost: "Spirit Boost"
                    , DexterityBoost: "Dexterity Boost"
                    , LuckBoost: "Luck Boost"
                    , LevelBoost: "Mysterious Bubbling Potion"
                )
            );
        }
    }
}
