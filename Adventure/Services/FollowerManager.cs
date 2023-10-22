using Engine;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Adventure.Services
{
    interface IFollowerNode
    {
        public void UpdateLocation(FollowerManagerArgs args);
    }

    class FollowerManagerArgs
    {
        public Vector3 NewLocation { get; set; }
        public Vector3 MovementDirection { get; set; }
        public bool Moving { get; set; }
    }

    class FollowerManager
    {
        class Entry
        {
            public Entry(IFollowerNode node, Vector3 startPos)
            {
                this.Node = node;
                this.StartPosition = startPos;
                this.EndPosition = startPos;
            }

            public float DistancePercent { get; set; }

            public Vector3 StartPosition { get; set; }

            public Vector3 EndPosition { get; set; }

            public Vector3 LastPosition { get; set; }

            public Vector3 MovementDirection { get; set; }

            public IFollowerNode Node { get; init; }
        }

        private List<Entry> followers = new List<Entry>();
        private Vector3 leaderStartLocation;
        private float characterDistance = 1.63f;
        private FollowerManagerArgs args = new FollowerManagerArgs();

        public FollowerManager()
        {
        }

        public float CharacterDistance
        {
            get
            {
                return characterDistance;
            }
            set
            {
                characterDistance = value;
            }
        }

        public void AddFollower(IFollowerNode follower)
        {
            followers.Add(new Entry(follower, leaderStartLocation));
        }

        public void RemoveFollower(IFollowerNode follower)
        {
            var count = followers.Count;
            for (int i = 0; i < count; ++i)
            {
                if (followers[i].Node == follower)
                {
                    followers.RemoveAt(i);
                    break;
                }
            }
        }

        public void LeaderMoved(in Vector3 location, bool activeMove)
        {
            var leaderLocDiff = location - leaderStartLocation;
            var distancePercent = leaderLocDiff.length() / (characterDistance);
            if (distancePercent > 2.0f) //More than 2x distance, move to leader position
            {
                leaderStartLocation = location;
                const float zOffset = 0.15f;
                var offset = new Vector3(0, 0, zOffset);
                foreach (var entry in followers)
                {
                    var offsetLoc = location + offset;
                    offset.z += zOffset;
                    args.NewLocation = offsetLoc;
                    args.MovementDirection = new Vector3(0f, 0f, -1f);
                    args.Moving = false;
                    entry.Node.UpdateLocation(args);
                    entry.EndPosition = entry.StartPosition = offsetLoc;
                    entry.DistancePercent = 0.0f;
                    entry.MovementDirection = new Vector3(0f, 0f, -1f);
                }
            }
            else
            {
                if (distancePercent > 1.0f)
                {
                    //Moved more than 1.0, first simulate what would have happened at 1.0
                    leaderStartLocation = leaderStartLocation + leaderLocDiff.normalized() * characterDistance;
                    var inFrontLocation = leaderStartLocation;

                    foreach (var entry in followers)
                    {
                        entry.StartPosition = entry.EndPosition;
                        entry.EndPosition = inFrontLocation;
                        inFrontLocation = entry.StartPosition;
                        entry.DistancePercent = 0.0f;
                        entry.MovementDirection = (entry.EndPosition - entry.StartPosition).normalized();
                    }

                    //Remove 1.0 and allow the rest of the frame to continue below with remainder
                    distancePercent -= 1.0f;
                }

                foreach (var entry in followers)
                {
                    if (entry.DistancePercent < distancePercent)
                    {
                        entry.DistancePercent = distancePercent;

                        args.NewLocation = entry.LastPosition = entry.StartPosition.lerp(entry.EndPosition, distancePercent);
                        args.Moving = activeMove;
                    }
                    else
                    {
                        args.NewLocation = entry.LastPosition;
                        args.Moving = false;
                    }

                    args.MovementDirection = entry.MovementDirection;
                    entry.Node.UpdateLocation(args);
                }
            }
        }
    }
}
