namespace Adventure.Assets.SoundEffects;

class SwordSoundEffects : IWeaponSoundEffects
{
    public static readonly IWeaponSoundEffects Instance = new SwordSoundEffects();

    public ISoundEffect Blocked => BlockedSwordHitSoundEffect.Instance;

    public ISoundEffect Normal => NormalSwordHitSoundEffect.Instance;

    public ISoundEffect Heavy => HeavySwordHitSoundEffect.Instance;
}

class NormalSwordHitSoundEffect : ISoundEffect
{
    public static readonly ISoundEffect Instance = new NormalSwordHitSoundEffect();

    public string File => "SoundEffects/hits/hit16.mp3.ogg";

    public bool Streaming => false;
}

class HeavySwordHitSoundEffect : ISoundEffect
{
    public static readonly ISoundEffect Instance = new HeavySwordHitSoundEffect();

    public string File => "SoundEffects/hits/hit33.mp3.ogg";

    public bool Streaming => false;
}

class BlockedSwordHitSoundEffect : ISoundEffect
{
    public static readonly ISoundEffect Instance = new BlockedSwordHitSoundEffect();

    public string File => "SoundEffects/100-CC0-wood-metal-SFX/metal_hit_05.ogg";

    public bool Streaming => false;
}