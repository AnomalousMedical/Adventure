namespace Adventure.Assets.SoundEffects;

internal class PageFixSoundEffect : ISoundEffect
{
    public static readonly ISoundEffect Instance = new PageFixSoundEffect();

    public string File => "SoundEffects/dmunk/331969__dmunk__flipping-through-pages.ogg";

    public bool Streaming => false;
}
