using Adventure.Services;
using Engine;
using SoundPlugin;
using System;
using System.Collections.Generic;
using System.IO;

namespace Adventure
{
    interface IBackgroundMusicPlayer
    {
        void SetBackgroundSong(string songFile);
    }

    class BackgroundMusicPlayer : IDisposable, IBackgroundMusicPlayer
    {
        private readonly VirtualFileSystem virtualFileSystem;
        private readonly SoundManager soundManager;
        private readonly ICoroutineRunner coroutineRunner;
        private readonly Options options;
        private SoundAndSource bgMusicSound;
        private bool bgMusicFinished = false;
        private String currentBackgroundSong;

        public BackgroundMusicPlayer(
            VirtualFileSystem virtualFileSystem, 
            SoundManager soundManager,
            ICoroutineRunner coroutineRunner,
            Options options)
        {
            this.virtualFileSystem = virtualFileSystem;
            this.soundManager = soundManager;
            this.coroutineRunner = coroutineRunner;
            this.options = options;
        }

        public void Dispose()
        {
            DisposeBgSound();
        }

        private void DisposeBgSound()
        {
            if (bgMusicSound != null)
            {
                bgMusicSound.Source.PlaybackFinished -= BgMusic_PlaybackFinished;
                bgMusicSound?.Dispose();
                bgMusicSound = null;
            }
        }

        public void SetBackgroundSong(String songFile)
        {
            if (currentBackgroundSong == songFile) { return; }

            DisposeBgSound();
            if (songFile != null)
            {
                var stream = virtualFileSystem.openStream(songFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                bgMusicSound = soundManager.StreamPlaySound(stream);
                bgMusicSound.Sound.Repeat = true;
                bgMusicSound.Source.Gain = options.MusicVolume;
                bgMusicSound.Source.PlaybackFinished += BgMusic_PlaybackFinished;
                bgMusicFinished = false;
            }
            currentBackgroundSong = songFile;
        }

        private void ResetBackgroundSong()
        {
            //This makes the song actually restart, otherwise it will be detected as the same
            var songChange = currentBackgroundSong;
            currentBackgroundSong = null;
            SetBackgroundSong(songChange);
        }

        private void BgMusic_PlaybackFinished(Source source)
        {
            bgMusicFinished = true;
            IEnumerator<YieldAction> co()
            {
                yield return coroutineRunner.WaitSeconds(0);
                if (bgMusicFinished) //Double check that the song was not changed.
                {
                    ResetBackgroundSong();
                }
            }
            coroutineRunner.Run(co());
        }
    }
}
