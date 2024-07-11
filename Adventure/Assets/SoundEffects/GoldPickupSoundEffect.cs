namespace Adventure.Assets.SoundEffects;

internal class GoldPickupSoundEffect : ISoundEffect
{
    public static readonly ISoundEffect Instance = new GoldPickupSoundEffect();

    public string File => "SoundEffects/IENBA/698768__ienba__game-pickup.ogg";

    public bool Streaming => false;
}
