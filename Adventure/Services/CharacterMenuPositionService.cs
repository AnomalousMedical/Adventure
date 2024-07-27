using Engine;
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
        Vector3 MagicHitLocation { get; }

        Vector3 Scale { get; }

        Vector3 CameraPosition { get; }

        Quaternion CameraRotation { get; }

        void FaceCamera();
    }

    public class CharacterMenuPositionEntry(Func<Vector3> GetCameraPosition, Func<Quaternion> GetCameraRotation, Action FaceCameraCb, Func<Vector3> GetMagicHitLocation, Func<Vector3> GetScale) : ICharacterMenuPositionEntry
    {
        public Vector3 MagicHitLocation => GetMagicHitLocation();

        public Vector3 Scale => GetScale();

        public Vector3 CameraPosition => GetCameraPosition();

        public Quaternion CameraRotation => GetCameraRotation();

        public void FaceCamera() => FaceCameraCb();
    }

    interface ICharacterMenuPositionTracker
    {
        void Remove(CharacterSheet sheet, ICharacterMenuPositionEntry entry);
        void Set(CharacterSheet sheet, ICharacterMenuPositionEntry entry);
        bool TryGetEntry(CharacterSheet sheet, out ICharacterMenuPositionEntry entry);
        void GetNormalCameraPosition(out Vector3 pos, out Quaternion rot);
    }

    interface ICharacterMenuPositionTracker<T> : ICharacterMenuPositionTracker
    {
    }

    class CharacterMenuPositionTracker<T>
    (
        MultiCameraMover<T> multiCameraMover
    )
    : ICharacterMenuPositionTracker<T>
    {
        private Dictionary<CharacterSheet, ICharacterMenuPositionEntry> entries = new Dictionary<CharacterSheet, ICharacterMenuPositionEntry>();

        public void Set(CharacterSheet sheet, ICharacterMenuPositionEntry entry)
        {
            entries[sheet] = entry;
        }

        public void Remove(CharacterSheet sheet, ICharacterMenuPositionEntry entry)
        {
            if (entries.TryGetValue(sheet, out var lookup))
            {
                if (entry == lookup)
                {
                    entries.Remove(sheet);
                }
            }
        }

        public bool TryGetEntry(CharacterSheet sheet, out ICharacterMenuPositionEntry entry)
        {
            return entries.TryGetValue(sheet, out entry);
        }

        public void GetNormalCameraPosition(out Vector3 pos, out Quaternion rot)
        {
            multiCameraMover.ComputeCameraPosition(out pos, out rot);
        }
    }

    class WrappingCharacterMenuPositionTracker<T> : ICharacterMenuPositionTracker<T>
    {
        private readonly ICharacterMenuPositionTracker wrapped;
        private ICharacterMenuPositionEntry overrideEntry;

        public bool UseOverrideEntry { get; set; }

        public WrappingCharacterMenuPositionTracker(ICharacterMenuPositionTracker wrapped)
        {
            this.wrapped = wrapped;
        }

        public void SetOverrideEntry(ICharacterMenuPositionEntry overrideEntry)
        {
            this.overrideEntry = overrideEntry;
        }

        public void UnsetOverrideEntry(ICharacterMenuPositionEntry overrideEntry)
        {
            if (overrideEntry == this.overrideEntry)
            {
                this.overrideEntry = null;
            }
        }

        public void Remove(CharacterSheet sheet, ICharacterMenuPositionEntry entry)
        {
            wrapped.Remove(sheet, entry);
        }

        public void Set(CharacterSheet sheet, ICharacterMenuPositionEntry entry)
        {
            wrapped.Set(sheet, entry);
        }

        public bool TryGetEntry(CharacterSheet sheet, out ICharacterMenuPositionEntry entry)
        {
            if (UseOverrideEntry)
            {
                entry = overrideEntry;
                return overrideEntry != null;
            }
            else
            {
                return wrapped.TryGetEntry(sheet, out entry);
            }
        }

        public void GetNormalCameraPosition(out Vector3 pos, out Quaternion rot)
        {
            if (UseOverrideEntry && overrideEntry != null)
            {
                pos = overrideEntry.CameraPosition;
                rot = overrideEntry.CameraRotation;
            }
            else
            {
                wrapped.GetNormalCameraPosition(out pos, out rot);
            }
        }
    }

    class CharacterMenuPositionService
    (
        ICharacterMenuPositionTracker<WorldMapScene> worldMapCharacterPositionTracker,
        ICharacterMenuPositionTracker<ZoneScene> zoneSceneCharacterPositionTracker
    )
    {
        private ICharacterMenuPositionTracker currentTracker;
        private Type activeTrackerType;

        public Type ActiveTrackerType => activeTrackerType;

        public void SetTrackerActive(Type trackerType)
        {
            activeTrackerType = trackerType;
            if (trackerType == typeof(WorldMapScene))
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

        public void GetNormalCameraPosition(out Vector3 pos, out Quaternion rot)
        {
            if (currentTracker == null)
            {
                pos = Vector3.Zero;
                rot = Quaternion.Identity;
            }
            else
            {
                currentTracker.GetNormalCameraPosition(out pos, out rot);
            }
        }
    }
}
