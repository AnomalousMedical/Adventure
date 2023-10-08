using Adventure.Assets.SoundEffects;
using Engine;
using SoundPlugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Services;

interface ISoundEffectPlayer
{
    void PlaySound(ISoundEffect soundEffect);
}

class SoundEffectPlayer : ISoundEffectPlayer
{
    private readonly SoundManager soundManager;
    private readonly VirtualFileSystem virtualFileSystem;
    private readonly Options options;

    public SoundEffectPlayer
    (
        SoundManager soundManager,
        VirtualFileSystem virtualFileSystem,
        Options options
    )
    {
        this.soundManager = soundManager;
        this.virtualFileSystem = virtualFileSystem;
        this.options = options;
    }

    public void PlaySound(ISoundEffect soundEffect)
    {
        var stream = virtualFileSystem.openStream(soundEffect.File, FileMode.Open, FileAccess.Read, FileShare.Read);
        Source source;
        if (soundEffect.Streaming)
        {
            source = soundManager.StreamPlayAndForgetSound(stream);
        }
        else
        {
            source = soundManager.MemoryPlayAndForgetSound(stream);
        }
        if (source != null)
        {
            source.Gain = options.SfxVolume;
        }
    }
}
