namespace Adventure.Assets.SoundEffects;

internal class EarthquakeSoundEffect : ISoundEffect
{
    public static readonly ISoundEffect Instance = new EarthquakeSoundEffect();

    public string File => "SoundEffects/uagadugu/222521__uagadugu__cracking-earthquake-cracking-soil-cracking-stone.ogg";

    public bool Streaming => false;
}
