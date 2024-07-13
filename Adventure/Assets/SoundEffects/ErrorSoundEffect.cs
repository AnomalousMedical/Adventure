namespace Adventure.Assets.SoundEffects;

internal class ErrorSoundEffect : ISoundEffect
{
    public static readonly ISoundEffect Instance = new ErrorSoundEffect();

    public string File => "SoundEffects/LorenzoTheGreat/417794__lorenzothegreat__error.ogg";

    public bool Streaming => false;
}
