namespace Adventure.Assets.SoundEffects;

class IceSpellSoundEffect : ISoundEffect
{
    public static readonly ISoundEffect Instance = new IceSpellSoundEffect();

    public string File => "SoundEffects/Fabrizio84/457590__fabrizio84__ice-cubes.ogg";

    public bool Streaming => false;
}

class FireSpellSoundEffect : ISoundEffect
{
    public static readonly ISoundEffect Instance = new FireSpellSoundEffect();

    public string File => "SoundEffects/qubodup/159725__qubodup__fire-magic-spell-sound-effect.ogg";

    public bool Streaming => false;
}

class LightningSpellSoundEffect : ISoundEffect
{
    public static readonly ISoundEffect Instance = new LightningSpellSoundEffect();

    public string File => "SoundEffects/csengeri/434359__csengeri__close-lightning-strike-2018-07-07.ogg";

    public bool Streaming => false;
}

class IonShreadSpellSoundEffect : ISoundEffect
{
    public static readonly ISoundEffect Instance = new IonShreadSpellSoundEffect();

    public string File => "SoundEffects/Aleks41/406063__aleks41__magic-strike.ogg";

    public bool Streaming => false;
}

class WarCrySpellSoundEffect : ISoundEffect
{
    public static readonly ISoundEffect Instance = new WarCrySpellSoundEffect();

    public string File => "SoundEffects/qubodup/166540__qubodup__success-quest-complete-rpg-sound.ogg";

    public bool Streaming => false;
}

class CureSpellSoundEffect : ISoundEffect
{
    public static readonly ISoundEffect Instance = new CureSpellSoundEffect();

    public string File => "SoundEffects/shyguy014/458533__shyguy014__healpop.ogg";

    public bool Streaming => false;
}

class LevelBoostSoundEffect : ISoundEffect
{
    public static readonly ISoundEffect Instance = new LevelBoostSoundEffect();

    public string File => "SoundEffects/shyguy014/458533__shyguy014__healpop.ogg";

    public bool Streaming => false;
}