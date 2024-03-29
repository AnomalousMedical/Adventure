﻿namespace Adventure.Assets.SoundEffects;

internal class BlockedSoundEffect : ISoundEffect
{
    public static readonly ISoundEffect Instance = new BlockedSoundEffect();

    public string File => "SoundEffects/hits/hit06.mp3.ogg";

    public bool Streaming => false;
}
