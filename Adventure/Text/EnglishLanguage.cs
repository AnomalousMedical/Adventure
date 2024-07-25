using Adventure.Exploration;
using Adventure.Items;
using Adventure.Menu;
using Adventure.Services;
using Adventure.Skills;
using Adventure.WorldMap;

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
                    FighterGreeting: "Don't worry. My shield will guard us.",
                    FighterClass: "Fighter",
                    FighterDescription: "Active Ability: Block enemy attacks with your shield.",
                    MageName: "Rabras",
                    MageGreeting: "Times a wasting. Let's get moving.",
                    MageClass: "Mage",
                    MageDescription: "Active Ability: Enhance attack spells while they travel to their target.",
                    ThiefName: "Malissa",
                    ThiefGreeting: "If we are going to help, hopefully we will find some good treasure.",
                    ThiefClass: "Thief",
                    ThiefDescription: "Active Ability: Enhance attacks using your offhand dagger.",
                    ClericName: "Adali",
                    ClericGreeting: "I wonder if these quakes are with what's made everything so agressive?",
                    ClericClass: "Cleric",
                    ClericDescription: "Active Ability: Enhance healing spells while they travel to their target."
                ),
                EndGameTrigger: new EndGameTrigger.Text
                (
                    Warning: "You feel that interacting with this artifact will end your time in this realm.",
                    Prompt: "Touch the artifact?",
                    FinalText: "You reach out to touch the artifact. As you do your vision goes black..."
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
                    Intro1: "Hello. I have a huge variety of potions and tonics.",
                    Intro2: "If I can keep them from falling off my shelves.",
                    SalesPitch: "What can I get you today?",
                    Ancient1: "Wow this information is amazing! You found this out exploring?",
                    Ancient2: "My potions will really pack a punch now.",
                    Goodbye: "See you next time.",
                    LevelPotion1: "Hmm this is interesting. The ingredients for this are quite rare, since they aren't used very often, but fortunately I have a large stockpile of them.",
                    LevelPotion2: "Oh no! Almost all of it was ruined in that last one. I can still brew one up for you. Just a second."
                ),
                AlchemistUpgrade: new AlchemistUpgrade.Text
                (
                    Check: "Check",
                    TeaseUpgrade: "For 200 gold I will share ancient and powerful alchemical recipes with you.",
                    UpgradePrompt: "Pay 200 gold?",
                    NotEnoughGold: "You do not have enough gold.",
                    GiveUpgrade: "The gargoyle shows you ancient alchemical techniques..."
                ),
                Blacksmith: new Blacksmith.Text
                (
                    Greeting: "Hello",
                    Intro1: "Hail adventurers. I have the best weapons around.",
                    Intro2: "I can make you anything you need. For a small fee of course.",
                    SalesPitch: "Let me know if anything catches your eye.",
                    Ancient1: "You're telling me if I heat my steel just so and then strike it it will be even stronger?",
                    Ancient2: "This is amazing! With this knowledge I can make my weapons even better.",
                    Goodbye: "Goodbye now."
                ),
                BlacksmithUpgrade: new BlacksmithUpgrade.Text
                (
                    Check: "Check",
                    TeaseUpgrade: "For 200 gold I will share ancient and powerful blacksmithing techniques with you.",
                    UpgradePrompt: "Pay 200 gold?",
                    NotEnoughGold: "You do not have enough gold.",
                    GiveUpgrade: "The gargoyle whispers ancient blacksmithing secrets into your ear..."
                ),
                AirshipEngineer: new AirshipEngineer.Text
                (
                    Greeting: "Hello",
                    InitialInfo: "Hold on a second.",
                    InitialInfo2: "Just as I suspected. These quakes are centered around the volcano that has started acting up recently.",
                    InitialInfo3: "However, we won't be able to get there with the airship in its current condition.",
                    InitialInfo4: "It seems like monsters are making off with our parts.",
                    NoAirshipItems: "Find the ship's wheel and some fuel. Look in the nearby wilderness areas. The signposts will guide you.",
                    HasFuelOnly: "You found the fuel now just find the wheel and we can fly.",
                    HasWheelOnly: "You found the wheel. We just need to fuel up and we can fly.",
                    BothAirshipItems: "You found everything. Give me a minute and I will fix her right up.",
                    AirshipFixed: "Got the wheel reattached and she's all fueled up. Here are the keys so you can fly whenever you're ready.",
                    FinalMessage: "Don't just rush to the volcano. I'm sure the monsters there are very strong. See what else you can find if you explore around."
                ),
                ItemStorage: new ItemStorage.Text
                (
                    Greeting: "Hello",
                    Intro1: "This statue stares at you so intently you feel it piercing your very soul.",
                    Intro2: "You feel that it could recreate important things you may have carelessly discarded on your journey. If they had enough value.",
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
                    Intro1: "Greetings. My cards can help you find things your thieving fingers may have missed.",
                    StartShufflePitch: "Let us see what the cards say about your fate.",
                    CardShuffleNarrator: "The fortune teller shuffles her deck of cards...",
                    ShowResultsNarrator: "The cards are arranged before you on the table.",
                    NoResultsNarrator: "The cards are quiet today.",
                    FirstCardIntro: "The first card shows ",
                    SecondCardIntro: "The second card shows ",
                    AssassinFortune: "a long dagger.",
                    HourglassFortune: "a hourglass next to curved dagger.",
                    LuckFortune: "various lucky animals including a rabbit and an elephant.",
                    CountrysideFortune: "a countryside with green trees and grass.",
                    SnowyFortune: "a snow filled tundra.",
                    ForestFortune: "a deep forest with tall trees.",
                    BeachFortune: "a scenic beach with palm trees.",
                    SwampFortune: "a dangerous swamp.",
                    MountainFortune: "a tall mountain."
                ),
                Innkeeper: new Innkeeper.Text
                (
                    Greeting: "Hello",
                    Intro1: "You don't think all these quakes are caused by a monster thats going to come eat us, do you?",
                    Intro2: "Eeek! I sure hope that isn't the case!",
                    Intro3: "Sorry, I know I shouldn't scare people like that who are just looking for rest.",
                    SleepQuestionDialog: "Would you like to get some sleep?"
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
                    , Spear2: new(Text: "Hoplite's Spear", Description: "A mid-tier spear. Delivers moderate piercing damage do your enemies.")
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
                    , Sword1: new(Text: "Soldier's Sword", Description: "A basic blade. Does a small amount of slashing damage to your opponents.")
                    , Sword2: new(Text: "Commander's Sword", Description: "A more refied blade. Does a moderate amount of slashing damage to your opponents.")
                    , Sword3: new(Text: "Ultimate Sword", Description: "A blade of legend. Nothing delivers more slashing damage to your opponents in the entire world.")
                    , StoreSword1: new(Text: "Smithed Sword", Description: "A hand made sword for slashing your opponents.")
                    , StoreSword2: new(Text: "Ancient Sword", Description: "A hand made sword created with ancient techniques. Will slash your opponents better than most.")
                    , Plate1: new(Text: "Tarnished Plate Armor", Description: "This suit of plate mail is worn out, but still offers some protection.")
                    , Plate2: new(Text: "Iron Plate Armor", Description: "Solid plate mail, but a bit too heavy to offer full protection.")
                    , Plate3: new(Text: "Steel Plate Armor", Description: "A suit of the finest plate mail. Delivers the ultimate protection that can be found.")
                    , Leather1: new(Text: "Cracked Leather Armor", Description: "Leather armor that needs some care. Still offers an extra pocket compared to most armor.")
                    , Leather2: new(Text: "Thief's Leather Armor", Description: "Leather armor suitable for infiltration. Has two extra pockets compared to most armor.")
                    , Leather3: new(Text: "Fine Leather Armor", Description: "The finest suit of leather armor in the world. Its immense quality allows it to carry three more items than most armor")
                    , Cloth1: new(Text: "Linen Cloth Armor", Description: "Basic cloth wear. Offers some protection, but not a lot. Its light weight enhances the wearer's magic and is required to hit effectively with attack spells.")
                    , Cloth2: new(Text: "Quilted Cloth Armor", Description: "Solid cloth armor that offers more protection that what is normally found. Its light weight enhances the wearer's magic and is required to hit effectively with attack spells.")
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
                    , StrengthBoost: new(Text: "Strength Boost", Description: "Permanently increase the target's strength. This will make them do more damage with attacks.")
                    , MagicBoost: new(Text: "Magic Boost", Description: "Permanently increase the target's magic. This will make them do more damage and healing with magic.")
                    , VitalityBoost: new(Text: "Vitality Boost", Description: "Permanently increase the target's vitality. This will make them take less damage from physical attacks.")
                    , SpiritBoost: new(Text: "Spirit Boost", Description: "Permanently increase the target's spirit. This will make them take less damage from magic attacks.")
                    , DexterityBoost: new(Text: "Dexterity Boost", Description: "Permanently increase the target's dexterity. This will make them faster in battle.")
                    , LuckBoost: new(Text: "Luck Boost", Description: "Permanently increase the target's luck. This will make them do more critical hits.")
                    , LevelBoost: new(Text: "Mysterious Bubbling Potion", Description: "Its sure to be safe...")
                ),
                PlotItems: new PlotItemText
                (
                    AirshipFuel: new(Text: "Airship Fuel", Description: "Pure elemental fire energy that has been captured and converted to an energy source."),
                    AirshipWheel: new(Text: "Airship's Wheel", Description: "A sturdy wheel to control the flight path of an airship."),
                    AirshipKey: new(Text: "Airship Keys", Description: "The keys to the airship. The final piece needed to fly."),
                    BlacksmithUpgrade: new(Text: "Ancient Blacksmithing Secrets", Description: "Ancient blacksmithing recipes. Provides instructions for smithing powerful exotic weapons."),
                    AlchemistUpgrade: new(Text: "Ancient Alchemy Secrets", Description: "Ancient alchemical recipes. Provides instructions for brewing powerful potions and tonics."),
                    RuneOfFire: new(Text: "Rune of Fire", Description: "A rune showing power over fire."),
                    RuneOfIce: new(Text: "Rune of Ice", Description: "A rune showing power over ice."),
                    RuneOfElectricity: new(Text: "Rune of Electricity", Description: "A rune showing power over electricity."),
                    ElementalStone: new(Text: "Elemental Stone", Description: "A stone that serves as proof of elemental mastery."),
                    GuideToPowerAndMayhem: new(Text: "Guide to Power and Mayhem", Description: "A helpful guide to gaining power and causing destruction. Some of its pages are missing."),
                    GuideToPowerAndMayhemChapter4: new(Text: "Guide to Power and Mayhem Chapter 4", Description: "The missing pages from Chapter 4."),
                    GuideToPowerAndMayhemChapter5: new(Text: "Guide to Power and Mayhem Chapter 5", Description: "The missing pages from Chapter 5."),
                    GuideToPowerAndMayhemChapter6: new(Text: "Guide to Power and Mayhem Chapter 6", Description: "The missing pages from Chapter 6.")
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
                ),
                PartyMemberTrigger: new Exploration.PartyMemberTrigger.Text
                (
                    GameIntroSetup: "Things seem to be getting worse. I wonder if there is anything we can do to help?"
                ),
                HelpBook: new HelpBook.Text
                (
                    Greeting: "Check",
                    BookIntro: "Before you lies \"The Guide to Power and Mayhem.\" Would you like to read it?",
                    ReadBookPrompt: "Read \"The Guide to Power and Mayhem?\"",
                    TakeBookPrompt: "Take \"The Guide to Power and Mayhem?\"",

                    PageIntro: "Before you lies a missing page from \"The Guide to Power and Mayhem.\" Would you like to read it?",
                    ReadPagePrompt: "Read the page from \"The Guide to Power and Mayhem?\"",
                    TakePagePrompt: "Take the page from \"The Guide to Power and Mayhem?\"",

                    Chapter1Title: "Page 1: Active Abilities",
                    Chapter1: "Active abilities are used with the right trigger or spacebar.\n \nThese will enhance your magic spells, allow you to use an offhand weapon or block during an enemy attack.\n \nCheck the item descriptions for more information.\n \nOnly press the button one time or the ability will not work correctly.",
                    
                    Chapter2Title: "Page 2: Thievery",
                    Chapter2: "When you try to steal from a pack of enemies you have a 100% chance to steal if they have an item.\n \nTrying again will not get you anything.",
                    
                    Chapter3Title: "Page 3: Weapon Damage and Weaknesses",
                    Chapter3: "Each weapon does either slashing, piercing or bludgeoning damage to your opponents.\n \nEach enemy type will be strong, weak and neutral to these damage types.\n \nExploit them to do even more damage.",
                    
                    Chapter4Title: "Page 4: Elemental Damage and Weaknesses",
                    Chapter4: "You can cast spells of ice, fire and electricity.\n \nEnemies will be weak, strong and neutral to these types, which will cause the damage to increase or decrease accordingly.\n \nSome enemies can even shift their elements and absorb damage as healing instead.",
                    Chapter4Part2Missing: "The rest of this page seems to be missing.",
                    Chapter4Part2: "Mid and high level elemental spells can be applied to your own party's weapons causing the elemental multiplier to apply in addition to the weapon damage and its type multiplier.\n \nBe careful though since elemental damage could end up weaker or even being absorbed and turned into hp.",
                    
                    Chapter5Title: "Page 5: Exploiting Weaknesses",
                    Chapter5: "Enemies will have weaknesses you can see with the Target Scope item.",
                    Chapter5Part2Missing: "The rest of this page seems to be missing.",
                    Chapter5Part2: "If you pay attention during your attacks you can hear and see how effective they are.",
                    
                    Chapter6Title: "Page 6: Inventory and equipment",
                    Chapter6: "Each character has their own inventory and can only use and equip items they are carrying.",
                    Chapter6Part2Missing: "The rest of this page seems to be missing.",
                    Chapter6Part2: "You can use anything in battle, even weapons.\n \nTry to switch your equipment on the fly to adapt to what you're fighting.",

                    Chapter7Title: "Page 7: Exploration",
                    Chapter7: "Be sure to explore as much as you can.\n \nIf you can't beat one area go to another. You never know what you will find.\n \nIf you don't know where to go next, let the signposts guide you.\n \nA flag marks a spot you have already visited.",

                    Chapter8Title: "Page 8: Teamwork",
                    Chapter8: "Everyone in your party has their own strengths and weaknesses.\n \nLook at each of their stats to determine their best role.\n \nStats like Heal% and Item% improve how well that character heals or uses items. Block% determines how much damage is reduced when guarding with a shield.",

                    BookTaken: "You pick up the book. Maybe you can find some of the pages that are missing from it later. You can read it any time from the Help section of the main menu.",
                    Page1Found: "You pick up the page and add it back to the book. It seems to repair itself with some kind of magic.",
                    Page2Found: "You pick up the page and add it back to the book. Like the first page this one also repairs itself.",
                    AllPagesFound: "You put the last page into the book and then you hear a click. A small panel opens revealing an envelope labeled \"Potion of Experience.\" Inside it appears to be an alchemical recipe.",
                    CannotTakePage: "If you had your copy of \"The Guide to Power and Mayhem\" you could add this page to it. But since you don't you must leave the page behind."
                ),
                RootMenu: new RootMenu.Text
                (
                    Skills: "Skills",
                    Items: "Items",
                    Files: "Files",
                    Help: "Help",
                    Options: "Options",
                    Debug: "Debug",
                    Close: "Close",
                    Undefeated: "Undefeated",
                    OldSchool: "Old School",
                    Gold: " Gold"
                ),
                Levels: new LevelText
                (
                    l17: "Scout",
                    l29: "Hunter",
                    l36: "Adventurer",
                    l44: "Pilot",
                    l51: "World Traveler",
                    l56: "Slayer",
                    l61: "Dragon Slayer",
                    l64: "Reaper",
                    l67: "Unstoppable",
                    l70: "Lesser God",
                    l73: "Boundary Breaker",
                    l77: "The One"
                ),
                KeybindMenu: new KeybindMenu.Text
                (
                    Confirm: "Confirm",
                    Cancel: "Cancel",
                    ActiveAbility: "Active Ability",
                    SwitchCharacter: "Switch Character",
                    OpenMenu: "Open Menu",
                    Previous: "Previous",
                    Next: "Next",
                    Up: "Up",
                    Down: "Down",
                    Left: "Left",
                    Right: "Right",
                    MoveUp: "Move Up",
                    MoveDown: "Move Down",
                    MoveLeft: "Move Left",
                    MoveRight: "Move Right",
                    Pause: "Pause"
                )
            );
        }
    }
}