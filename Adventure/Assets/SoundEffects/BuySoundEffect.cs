namespace Adventure.Assets.SoundEffects;

internal class BuySoundEffect : ISoundEffect
{
    public static readonly ISoundEffect Instance = new BuySoundEffect();

    public string File => "SoundEffects/altfuture/174629__altfuture__coins.ogg";

    public bool Streaming => false;
}
