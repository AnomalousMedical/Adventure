namespace Adventure.Assets.SoundEffects;

interface ISoundEffect
{
    public string File { get; }

    public bool Streaming { get; }
}


interface IWeaponSoundEffects
{
    public ISoundEffect Blocked { get; }

    public ISoundEffect Normal { get; }

    public ISoundEffect Heavy { get; }
}