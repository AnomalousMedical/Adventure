using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.SoundEffects;

internal class FixAirshipSoundEffect : ISoundEffect
{
    public static readonly ISoundEffect Instance = new FixAirshipSoundEffect();

    public string File => "SoundEffects/pablodavilla/592111__pablodavilla__hammer_2.ogg";

    public bool Streaming => false;
}
