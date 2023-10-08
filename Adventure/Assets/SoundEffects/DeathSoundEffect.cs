namespace Adventure.Assets.SoundEffects;

internal class DeathSoundEffect : ISoundEffect
{
    public static readonly ISoundEffect Instance = new DeathSoundEffect();

    public string File => "SoundEffects/IwanGabovitch/synthetic_explosion_1.ogg";

    public bool Streaming => false;
}
