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

    public SoundEffectPlayer
    (
        SoundManager soundManager,
        VirtualFileSystem virtualFileSystem
    )
    {
        this.soundManager = soundManager;
        this.virtualFileSystem = virtualFileSystem;
    }

    public void PlaySound(ISoundEffect soundEffect)
    {
        var stream = virtualFileSystem.openStream(soundEffect.File, FileMode.Open, FileAccess.Read, FileShare.Read);
        if(soundEffect.Streaming)
        {
            soundManager.StreamPlayAndForgetSound(stream);
        }
        else
        {
            soundManager.MemoryPlayAndForgetSound(stream);
        }
    }
}
