namespace Adventure.Assets.SoundEffects;

class SpearSoundEffects : IWeaponSoundEffects
{
    public static readonly IWeaponSoundEffects Instance = new SpearSoundEffects();

    public ISoundEffect Blocked => BlockedSpearSoundEffect.Instance;

    public ISoundEffect Normal => NormalSpearSoundEffect.Instance;

    public ISoundEffect Heavy => HeavySpearSoundEffect.Instance;
}

class NormalSpearSoundEffect : ISoundEffect
{
    public static readonly ISoundEffect Instance = new NormalSpearSoundEffect();

    public string File => "SoundEffects/hits/hit15.mp3.ogg";

    public bool Streaming => false;
}

class HeavySpearSoundEffect : ISoundEffect
{
    public static readonly ISoundEffect Instance = new HeavySpearSoundEffect();

    public string File => "SoundEffects/hits/hit24.mp3.ogg";

    public bool Streaming => false;
}

class BlockedSpearSoundEffect : ISoundEffect
{
    public static readonly ISoundEffect Instance = new BlockedSpearSoundEffect();

    public string File => "SoundEffects/100-CC0-wood-metal-SFX/metal_hit_04.ogg";

    public bool Streaming => false;
}