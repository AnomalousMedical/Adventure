using Adventure.Items;
using Adventure.Items.Creators;
using Adventure.Services;
using Adventure.Skills;
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
                    FighterName: "Bolar",
                    FighterGreeting: "My shield will guard us.",
                    MageName: "Rabras",
                    MageGreeting: "Let's get moving.",
                    ThiefName: "Malissa",
                    ThiefGreeting: "I hope we find lots of great treasure!",
                    ClericName: "Adali",
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
                    AncientSalesPitch: "My potions will really pack a punch now.",
                    Goodbye: "See you next time."
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
                    AncientSalesPitch: "With this knowledge I can make my weapons even better.",
                    Goodbye: "Goodbye now."
                ),
                BlacksmithUpgrade: new BlacksmithUpgrade.Text
                (
                    Check: "Check",
                    GiveUpgrade: "The gargoyle whispers ancient blacksmithing secrets into your ear..."
                ),
                ItemStorage: new ItemStorage.Text
                (
                    Greeting: "Hello",
                    RecoverItems: "The gargoyle whispers the echos of discarded power in your ear...",
                    NoItems: "The gargoyle whispers the sounds of an empty void in your ear..."
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
                      Unarmed: new(Text: "Unarmed", Description: "The ol one two punch")
                    , CounterAttack: new(Text: "Gauntlets of Revenge", Description: "Hit your foes back when they hit you. Nothing beats a good counter attack.")
                    , TargetScope: new(Text: "Target Scope", Description: "Get a leg up on the competition. Good for the mage who wants to hit em where it hurts.")
                    , Healing: new(Text: "Ring of Healing", Description: "Why heal one person when you could heal them all? Use your active ability to help the whole group. Packs more healing punch as an added bonus.")
                    , Doublecast: new(Text: "Elemental Amplifier", Description: "Pure power. Your enemies will crumble before you.")
                    , ItemUsage: new(Text: "Gloves of Handling", Description: "With these gloves you can use items like a pro. Get every last drop from those expensive potions.")
                    , Dagger1: new(Text: "Dagger", Description: "Hit em extra hard with this cutpurse classic. Comes with the active ability to do even more damage.")
                    , Dagger2: new(Text: "Assassin's Blade", Description: "Your enemies won't know what hit them. Use your active ability to do even more damage.")
                    , Dagger3: new(Text: "Dagger of Shifting Sands", Description: "Alter the fabric of reality itself and grant immense speed to yourself and your allies. Still includes your favorites like bonus damage and even more damage on your active ability.")
                    , Staff1: new(Text: "Cracked Staff", Description: "This staff has seen better days, but you can still exploit your foes elemental weakness. Use your active ability for even more damage.")
                    , Staff2: new(Text: "Mage's Staff", Description: "A time tested classic. Your active ability will now hit all the enemies. You can also use elemental magic on your own party to enhance their attacks with an elemental multiplier.")
                    , Staff3: new(Text: "Arch Mage's Staff", Description: "True power. This staff has the new non-elemental spell Ionic Shread. Be sure to use your active ability when casting it or it will drain your resources. Use your active ability with the elemental spells to hit all your enemies and don't forget to buff your party with elemental multiplier buffs.")
                    , Shield1: new(Text: "Buckler", Description: "A basic shield. Use your active ability once during an enemy attack to reduce damage taken by 15% and to guard your allies.")
                    , Shield2: new(Text: "Knights Shield", Description: "A shield that helps a brave knight stand guard. Use your active ability once during an enemy attack to reduce damage taken by 35% and to guard your allies.")
                    , Shield3: new(Text: "Glowing Shield", Description: "The ultimate in damage reduction technology. Use your active ability once during an enemy attack to reduce damage taken by 45% and to guard your allies.")
                    , Spear1: new(Text: "Rusty Spear", Description: "This spear has seen better days, but still does good enough piercing damage to your enemies.")
                    , Spear2: new(Text: "Common Spear", Description: "A mid-tier spear you find everywhere to deliver piercing damage do your enemies.")
                    , Spear3: new(Text: "Runic Spear", Description: "A one of a kind spear dealing the most piercing damage to your enemies of anything in the world.")
                    , StoreSpear1: new(Text: "Smithed Spear", Description: "A hand made spear for piercing your enemies.")
                    , StoreSpear2: new(Text: "Ancient Spear", Description: "A hand made spear created with ancient techniques. Delivers piercing damage to your enemies better than most.")
                    , Book1: new(Text: "Torn Book of Healing", Description: "A book of healing, but most of the pages are missing. Allows you to cast a basic cure spell. Restorative magic can only be used with weapons that do bludgeoning damage.")
                    , Book2: new(Text: "Book of Healing", Description: "A book of healing with all its pages. Mega Cure is a mid-tier healing spell. Battle Cry improves strength and vitality. Focus improves magic and spirit. Restorative magic can only be used with weapons that do bludgeoning damage.")
                    , Book3: new(Text: "Book of Forbidden Healing", Description: "A book of healing containing powerful, forbidden healing magic. Reanimate will raise your fallen allies in battle. Ultra Cure provides the most powerful healing in the world. War Cry improves strength and vitality. Intense Focus improves magic and spirit. Restorative magic can only be used with weapons that do bludgeoning damage.")
                    , Hammer1: new(Text: "Tiny Hammer", Description: "A very small hammer, but can still deliver some bludgeoning damage to your foes. Does not restrict using restorative magic.")
                    , Hammer2: new(Text: "Giant's Hammer", Description: "A large hammer almost too heavy to wield. Delivers mid-tier bludgeoning damage to your foes. Does not restrict using restorative magic.")
                    , Hammer3: new(Text: "Hammer of the Gods", Description: "A unique hammer suitable for divine use. Delivers the most bludgeoning damage to your foes out of anything in the world. Does not restrict using restorative magic.")
                    , StoreHammer1: new(Text: "Smithed Hammer", Description: "A hand made hammer for bludgeoning your foes.")
                    , StoreHammer2: new(Text: "Ancient Hammer", Description: "A hand made hammer created with ancient techniques. It will deliver bludgeoning damage to your foes better than most.")
                    , Sword1: new(Text: "Busted Sword", Description: "A soldier's blade, inevitably replaced by something better. Still does an ok amount of slashing damage to your opponents.")
                    , Sword2: new(Text: "Common Sword", Description: "The most common type of blade found through the land. Many rely on its slashing damage to defeat their opponents.")
                    , Sword3: new(Text: "Ultimate Sword", Description: "A blade of legend. Nothing delivers more slashing damage to your opponents in the entire world.")
                    , StoreSword1: new(Text: "Smithed Sword", Description: "A hand made sword for slashing your opponents.")
                    , StoreSword2: new(Text: "Ancient Sword", Description: "A hand made sword creted with ancient techniques. Will slash your opponents better than most.")
                    , Plate1: new(Text: "Tarnished Plate Armor", Description: "Once fine plate mail. This suit is worn out, but still offers decent protection.")
                    , Plate2: new(Text: "Iron Plate Armor", Description: "Solid plate mail, but a bit too heavy to offer full protection.")
                    , Plate3: new(Text: "Steel Plate Armor", Description: "A suit of the finest plate mail. Delivers the ultimate protection that can be found.")
                    , Leather1: new(Text: "Cracked Leather Armor", Description: "Leather armor that needs some care. Still offers an extra pocket compared to most armor.")
                    , Leather2: new(Text: "Thief's Leather Armor", Description: "Leather armor suitable for infiltration. Has two extra pockets compared to most armor.")
                    , Leather3: new(Text: "Fine Leather Armor", Description: "The finest suit of leather armor in the world. Its immense quality allows it to carry three more items than most armor")
                    , Cloth1: new(Text: "Linen Cloth Armor", Description: "Basic cloth wear. Offers some protection, but not a lot. Its light weight enhances the wearer's magic and is required to hit effectively with attack spells.")
                    , Cloth2: new(Text: "Common Cloth Armor", Description: "Solid cloth armor that offers more protection that what is normally found. Its light weight enhances the wearer's magic and is required to hit effectively with attack spells.")
                    , Cloth3: new(Text: "Silk Cloth Armor", Description: "Cloth armor made of the finest silk. Its light weight enhances the wearer's magic and is required to hit effectively with attack spells.")
                    , RuneOfIce: new(Text: "Rune of Ice", Description: "Part of a greater mystery...")
                    , RuneOfElectricity: new(Text: "Rune of Electricity", Description: "Part of a greater mystery...")
                    , Mana1: new(Text: "Small Mana Potion", Description: "A small potion that restores 45 mp.")
                    , Mana2: new(Text: "Mana Potion", Description: "A medium potion that restores 75 mp.")
                    , Mana3: new(Text: "Giant Mana Potion", Description: "A large potion that restores 150 mp.")
                    , Health1: new(Text: "Small Health Potion", Description: "A small potion that restores 50 hp.")
                    , Health2: new(Text: "Health Potion", Description: "A potion that restores 300 hp.")
                    , Health3: new(Text: "Giant Health Potion", Description: "A large potion that restores 1500 hp.")
                    , FerrymansBribe: new(Text: "Ferryman's Bribe", Description: "Call back an ally from the brink of death.")
                    , StrengthBoost: new(Text: "Strength Boost", Description: "Permanently increase the target's strength.")
                    , MagicBoost: new(Text: "Magic Boost", Description: "Permanently increase the target's magic.")
                    , VitalityBoost: new(Text: "Vitality Boost", Description: "Permanently increase the target's vitality.")
                    , SpiritBoost: new(Text: "Spirit Boost", Description: "Permanently increase the target's spirit.")
                    , DexterityBoost: new(Text: "Dexterity Boost", Description: "Permanently increase the target's dexterity.")
                    , LuckBoost: new(Text: "Luck Boost", Description: "Permanently increase the target's luck.")
                    , LevelBoost: new(Text: "Mysterious Bubbling Potion", Description: "Its sure to be safe...")
                ),
                Skills: new SkillText
                (
                    Cure: new(Text: "Cure", Description: "Restore a small amount of hp. Use your active ability to restore more hp.")
                    , MegaCure: new(Text: "Mega Cure", Description: "Restore a medium amount of hp. Use your active ability to restore more hp.")
                    , UltraCure: new(Text: "Ultra Cure", Description: "Restore a large amount of hp. Use your active ability to restore more hp.")
                    , Reanimate: new(Text: "Reanimate", Description: "Revive an ally from the brink of death. Use your active ability to heal all hp.")
                    , Steal: new(Text: "Steal", Description: "Steal an item from a group of enemies. If the enemies have an item it will always be stolen.")
                    , Fire: new(Text: "Fire", Description: "A basic fire spell. Use your active ability to do more damage.")
                    , StrongFire: new(Text: "Strong Fire", Description: "A strong fire spell. Use your active ability to do more damage and hit all enemies. Cast this on your allies to add elemental multipliers to their attack.")
                    , ArchFire: new(Text: "Arch Fire", Description: "The ultimate fire spell. Use your active ability to do more damage and hit all enemies. Cast this on your allies to add elemental multipliers to their attack.")
                    , Ice: new(Text: "Ice", Description: "A basic ice spell. Use your active ability to do more damage.")
                    , StrongIce: new(Text: "Strong Ice", Description: "A strong ice spell. Use your active ability to do more damage and hit all enemies. Cast this on your allies to add elemental multipliers to their attack.")
                    , ArchIce: new(Text: "Arch Ice", Description: "The ultimate ice spell. Use your active ability to do more damage and hit all enemies. Cast this on your allies to add elemental multipliers to their attack.")
                    , Lightning: new(Text: "Lightning", Description: "A basic electrical spell. Use your active ability to do more damage.")
                    , StrongLightning: new(Text: "Strong Lightning", Description: "A strong electrical spell. Use your active ability to do more damage and hit all enemies. Cast this on your allies to add elemental multipliers to their attack.")
                    , ArchLightning: new(Text: "Arch Lightning", Description: "The ultimate electrical spell. Use your active ability to do more damage and hit all enemies. Cast this on your allies to add elemental multipliers to their attack.")
                    , IonShread: new(Text: "Ion Shread", Description: "The ultimate magical attack. Be sure to use your active ability or it will drain your mana.")
                    , BattleCry: new(Text: "Battle Cry", Description: "Raise an ally's strength and vitality by 30.")
                    , WarCry: new(Text: "War Cry", Description: "Raise an ally's strength and vitality by 45.")
                    , Focus: new(Text: "Focus", Description: "Raise an ally's magic and spirit by 30.")
                    , IntenseFocus: new(Text: "Intense Focus", Description: "Raise an ally's magic and spirit by 45.")
                    , Haste: new(Text: "Haste", Description: "Raise an ally's dexterity by 100 and give their turns priority in battle.")
                )
            );
        }
    }
}
