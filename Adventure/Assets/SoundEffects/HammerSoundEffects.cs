namespace Adventure.Assets.SoundEffects;

class HammerSoundEffects : IWeaponSoundEffects
{
    public static readonly IWeaponSoundEffects Instance = new HammerSoundEffects();

    public ISoundEffect Blocked => BlockedHammerSoundEffect.Instance;

    public ISoundEffect Normal => NormalHammerSoundEffect.Instance;

    public ISoundEffect Heavy => HeavyHammerSoundEffect.Instance;
}

class NormalHammerSoundEffect : ISoundEffect
{
    public static readonly ISoundEffect Instance = new NormalHammerSoundEffect();

    public string File => "SoundEffects/hits/hit23.mp3.ogg";

    public bool Streaming => false;
}

class HeavyHammerSoundEffect : ISoundEffect
{
    public static readonly ISoundEffect Instance = new HeavyHammerSoundEffect();

    public string File => "SoundEffects/hits/hit35.mp3.ogg";

    public bool Streaming => false;
}

class BlockedHammerSoundEffect : ISoundEffect
{
    public static readonly ISoundEffect Instance = new BlockedHammerSoundEffect();

    public string File => "SoundEffects/100-CC0-wood-metal-SFX/hammer_04.ogg";

    public bool Streaming => false;
}