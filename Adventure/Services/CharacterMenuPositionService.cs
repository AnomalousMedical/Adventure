﻿using Engine;
using RpgMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Services
{
    interface ICharacterMenuPositionEntry
    {
        Vector3 Position { get; }

        Quaternion CameraRotation { get; }

        void FaceCamera();
    }

    public class CharacterMenuPositionEntry(Func<Vector3> GetPosition, Func<Quaternion> GetCameraRotation, Action FaceCameraCb) : ICharacterMenuPositionEntry
    {
        public Vector3 Position => GetPosition();

        public Quaternion CameraRotation => GetCameraRotation();

        public void FaceCamera() => FaceCameraCb();
    }

    class CharacterMenuPositionTracker
    {
        private Dictionary<CharacterSheet, ICharacterMenuPositionEntry> entries = new Dictionary<CharacterSheet, ICharacterMenuPositionEntry>();

        public void Set(CharacterSheet sheet, ICharacterMenuPositionEntry entry)
        {
            entries[sheet] = entry;
        }

        public void Remove(CharacterSheet sheet, ICharacterMenuPositionEntry entry)
        {
            if(entries.TryGetValue(sheet, out var lookup))
            {
                if (entry == lookup)
                {
                    entries.Remove(sheet);
                }
            }
        }

        internal bool TryGetEntry(CharacterSheet sheet, out ICharacterMenuPositionEntry entry)
        {
            return entries.TryGetValue(sheet, out entry);
        }
    }

    class CharacterMenuPositionTracker<T> : CharacterMenuPositionTracker
    {
    }

    class CharacterMenuPositionService
    (
        CharacterMenuPositionTracker<WorldMapScene> worldMapCharacterPositionTracker,
        CharacterMenuPositionTracker<ZoneScene> zoneSceneCharacterPositionTracker
    )
    {
        private CharacterMenuPositionTracker currentTracker;

        public void SetTrackerActive(Type trackerType)
        {
            if(trackerType == typeof(WorldMapScene))
            {
                currentTracker = worldMapCharacterPositionTracker;
            }
            else if(trackerType == typeof(ZoneScene))
            {
                currentTracker = zoneSceneCharacterPositionTracker;
            }
            else
            {
                currentTracker = null;
            }
        }

        public bool TryGetEntry(CharacterSheet sheet, out ICharacterMenuPositionEntry entry)
        {
            if(currentTracker == null)
            {
                entry = null;
                return false;
            }

            return currentTracker.TryGetEntry(sheet, out entry);
        }
    }
}