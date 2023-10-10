using Adventure.Services;
using Engine;
using SoundPlugin;
using System;
using System.Collections.Generic;
using System.IO;

namespace Adventure
{
    class ResumeMusicToken
    {
        public ResumeMusicToken(String songFile)
        {
            this.SongFile = songFile;
        }

        /// <summary>
        /// The time to resume playback at. Note this is not updated until the song that provided the token has been replaced with another.
        /// </summary>
        public TimeSpan PlaybackTime { get; set; }

        /// <summary>
        /// The original song file.
        /// </summary>
        public String SongFile { get; set; }
    }

    interface IBackgroundMusicPlayer
    {
        /// <summary>
        /// Set the background song. Playback only changes if the song is different from what is currently playing. If the song changes
        /// and the new song matches the song in the optional resume token that song will be played from the point specified by the token.
        /// </summary>
        /// <param name="songFile">The new song to play.</param>
        /// <param name="resumeToken">The token to use to resume playback. Can be null to always play the song from the start.</param>
        /// <returns></returns>
        ResumeMusicToken SetBackgroundSong(string songFile, ResumeMusicToken resumeToken = null);
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
        private DateTime playbackStartTime;
        private ResumeMusicToken resumeMusicToken;

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
                resumeMusicToken.PlaybackTime = DateTime.Now - playbackStartTime;
                bgMusicSound?.Dispose();
                bgMusicSound = null;
            }
        }

        public ResumeMusicToken SetBackgroundSong(String songFile, ResumeMusicToken resumeToken = null)
        {
            if (currentBackgroundSong == songFile)
            {
                return this.resumeMusicToken;
            }

            DisposeBgSound();
            if (songFile != null)
            {
                var stream = virtualFileSystem.openStream(songFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                bgMusicSound = soundManager.StreamPlaySound(stream);
                bgMusicSound.Sound.Repeat = true;
                playbackStartTime = DateTime.Now;
                if (resumeToken != null && resumeToken.SongFile == songFile)
                {
                    var playbackOffset = (float)(resumeToken.PlaybackTime.TotalSeconds % bgMusicSound.Sound.Duration);
                    bgMusicSound.Source.PlaybackPosition = playbackOffset;
                    playbackStartTime -= TimeSpan.FromSeconds(playbackOffset);
                }
                bgMusicSound.Source.Gain = options.MusicVolume;
                bgMusicSound.Source.PlaybackFinished += BgMusic_PlaybackFinished;
                bgMusicFinished = false;
            }
            currentBackgroundSong = songFile;
            this.resumeMusicToken = new ResumeMusicToken(songFile);
            return this.resumeMusicToken;
        }

        private void ResetBackgroundSong()
        {
            //This makes the song actually restart, otherwise it will be detected as the same
            var songChange = currentBackgroundSong;
            var originalResumeToken = this.resumeMusicToken;

            currentBackgroundSong = null;
            SetBackgroundSong(songChange);

            //Ignore the new token that was created, since it was not returned anywhere, instead set back the original
            this.resumeMusicToken = originalResumeToken;
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
