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
                )
            );
        }
    }
}
