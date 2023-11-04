using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.SoundEffects;

internal class OpenChestSoundEffect : ISoundEffect
{
    public static readonly ISoundEffect Instance = new OpenChestSoundEffect();

    public string File => "SoundEffects/spookymodem/202092__spookymodem__chest-opening.ogg";

    public bool Streaming => false;
}
