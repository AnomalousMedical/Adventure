namespace Adventure.Assets.SoundEffects;

public interface ISoundEffect
{
    public string File { get; }

    public bool Streaming => false;
}


public interface IWeaponSoundEffects
{
    public ISoundEffect Blocked { get; }

    public ISoundEffect Normal { get; }

    public ISoundEffect Heavy { get; }
}