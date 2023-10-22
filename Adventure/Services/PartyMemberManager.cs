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

    public event Action PartyChanged;

    public PartyMemberManager(Persistence persistence)
    {
        this.persistence = persistence;
    }

    public void AddToParty(PartyMember partyMember)
    {
        persistence.Current.Party.Members.Add(partyMember.CharacterData);
        PartyChanged?.Invoke();
    }
}
