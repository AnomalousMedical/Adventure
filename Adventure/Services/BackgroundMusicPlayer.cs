using Adventure.Services;
using Engine;
using SoundPlugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure
{
    interface IBackgroundMusicPlayer
    {
        void SetBackgroundSong(string songFile);
        void SetBattleTrack(string songFile);
    }

    class BackgroundMusicPlayer : IDisposable, IBackgroundMusicPlayer
    {
        private readonly VirtualFileSystem virtualFileSystem;
        private readonly SoundManager soundManager;
        private readonly ICoroutineRunner coroutineRunner;
        private readonly Options options;
        private SoundAndSource bgMusicSound;
        private SoundAndSource battleMusicSound;
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
            battleMusicSound?.Dispose();
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
            if (battleMusicSound == null && songFile != null)
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

        public void SetBattleTrack(String songFile)
        {
            battleMusicSound?.Dispose();
            battleMusicSound = null;

            if (songFile == null)
            {
                if (bgMusicSound != null && !bgMusicFinished && !bgMusicSound.Source.Playing)
                {
                    bgMusicSound.Source.resume();
                }
                else if (bgMusicSound == null && currentBackgroundSong != null)
                {
                    //If we should play a song, but it hasn't started yet, this would happen if the bg music changes during a battle.
                    ResetBackgroundSong();
                }
            }
            else
            {
                var stream = virtualFileSystem.openStream(songFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                battleMusicSound = soundManager.StreamPlaySound(stream);
                battleMusicSound.Sound.Repeat = true;
                battleMusicSound.Source.Gain = options.MusicVolume;

                if (bgMusicSound != null)
                {
                    bgMusicSound.Source.pause();
                }
            }
        }
    }
}
