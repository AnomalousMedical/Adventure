﻿using Engine;
using System;
using System.Threading.Tasks;

namespace Adventure
{
    interface ILevelManager
    {

        event Action<ILevelManager> LevelChanged;

        bool ChangingLevels { get; }
        Level CurrentLevel { get; }
        bool IsPlayerMoving { get; }
        Task GoNextLevel();
        Task GoPreviousLevel();
        Task Restart();
        Task WaitForCurrentLevel();
        Task WaitForNextLevel();
        Task WaitForPreviousLevel();
        void StopPlayer();
        void GoStartPoint();
        void GoEndPoint();
        void RebuildPhysics();
        Vector3 GetPlayerLoc();
    }
}