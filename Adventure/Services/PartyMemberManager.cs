using RpgMath;
using System;

namespace Adventure.Services;

class PartyMember
{
    public Persistence.CharacterData CharacterData { get; set; }

    public String Greeting { get; set; }
}

class PartyMemberManager
{
    private readonly Persistence persistence;
    private readonly ILevelCalculator levelCalculator;

    public event Action PartyChanged;

    public PartyMemberManager(Persistence persistence, ILevelCalculator levelCalculator)
    {
        this.persistence = persistence;
        this.levelCalculator = levelCalculator;
    }

    public void AddToParty(PartyMember partyMember)
    {
        var worldLevel = persistence.Current.World.Level;
        var sheet = partyMember.CharacterData.CharacterSheet;
        while (sheet.Level < worldLevel)
        {
            sheet.LevelUp(levelCalculator);
        }
        persistence.Current.Party.Members.Add(partyMember.CharacterData);
        PartyChanged?.Invoke();
    }
}
