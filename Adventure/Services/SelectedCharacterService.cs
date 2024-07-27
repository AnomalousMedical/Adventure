using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Services;

class SelectedCharacterService(Persistence persistence)
{
    private int selectedCharacter;
    public int SelectedCharacter
    {
        get
        {
            if (selectedCharacter >= persistence.Current.Party.Members.Count)
            {
                selectedCharacter = 0;
            }
            return selectedCharacter;
        }
    }

    public void Next()
    {
        ++selectedCharacter;
        if (selectedCharacter >= persistence.Current.Party.Members.Count)
        {
            selectedCharacter = 0;
        }
    }

    public void Previous()
    {
        --selectedCharacter;
        if (selectedCharacter < 0)
        {
            selectedCharacter = persistence.Current.Party.Members.Count - 1;
        }
    }
}
