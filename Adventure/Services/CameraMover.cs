using Engine;
using Engine.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure
{
    class CameraMover
    {
        private Vector3 Position = Vector3.Zero;
        private Quaternion Orientation = Quaternion.Identity;

        private Vector3? CurrentPosition;
        private Quaternion? CurrentOrientation;

        public float InterpolateSpeed = 8.0f;

        public void SetPosition(in Vector3 position, in Quaternion orientation)
        {
            CurrentPosition = Position = position;
            CurrentOrientation = Orientation = orientation;
        }

        public void SetInterpolatedGoalPosition(in Vector3 position, in Quaternion orientation)
        {
            Position = position;
            Orientation = orientation;
        }

        public void OffsetCurrentPosition(in Vector3 offset)
        {
            CurrentPosition += offset;
        }

        public void OffsetPosition(in Vector3 offset)
        {
            Position += offset;
        }

        public void GetPosition(Clock clock, out Vector3 position, out Quaternion orientation)
        {
            if (this.CurrentPosition.HasValue)
            {
                this.CurrentPosition = this.CurrentPosition.Value.lerp(this.Position, InterpolateSpeed * clock.DeltaSeconds);
            }
            else
            {
                this.CurrentPosition = Position;
            }
            position = this.CurrentPosition.Value;

            if (this.CurrentOrientation.HasValue)
            {
                this.CurrentOrientation = this.CurrentOrientation.Value.slerp(this.Orientation, InterpolateSpeed * clock.DeltaSeconds);
            }
            else
            {
                this.CurrentOrientation = Orientation;
            }
            orientation = this.CurrentOrientation.Value;
        }

        public Vector3 CameraPosition => CurrentPosition ?? Position;

        public Quaternion CameraOrientation => CurrentOrientation ?? Orientation;
    }
}
