using Adventure.Exploration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Services;

class PartyMemberTriggerManager
{
    private List<PartyMemberTrigger> partyMemberTriggers = new List<PartyMemberTrigger>();

    public void Add(PartyMemberTrigger partyMemberTrigger)
    {
        partyMemberTriggers.Add(partyMemberTrigger);
    }

    public void Remove(PartyMemberTrigger partyMemberTrigger)
    {
        partyMemberTriggers.Remove(partyMemberTrigger);
    }

    public PartyMemberTrigger Get(int index)
    {
        return partyMemberTriggers[index];
    }

    public int Count => partyMemberTriggers.Count;
}
