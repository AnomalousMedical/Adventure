namespace Adventure.Assets.SoundEffects;

class PunchSoundEffect : ISoundEffect
{
    public static readonly ISoundEffect Instance = new PunchSoundEffect();

    public string File => "SoundEffects/hits/hit14.mp3.ogg";
}
