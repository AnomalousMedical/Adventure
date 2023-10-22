using Engine;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Adventure.Services
{
    interface IFollowerNode
    {
        public Vector3 CurrentLocation { get; }

        public void UpdateLocation(FollowerManagerArgs args);
    }

    class FollowerManagerArgs
    {
        public Vector3 NewLocation { get; set; }
    }

    class FollowerManager
    {
        class Entry
        {
            public Entry(IFollowerNode node)
            {
                this.Node = node;
            }

            public float DistancePercent { get; set; }

            public Vector3 StartPosition { get; set; }

            public Vector3 EndPosition { get; set; }

            public IFollowerNode Node { get; init; }
        }

        private List<Entry> leadersToFollowers = new List<Entry>();
        private Vector3 leaderStartLocation;
        private float characterDistance = 2.0f;
        private FollowerManagerArgs args = new FollowerManagerArgs();
        private readonly ILogger<FollowerManager> logger;

        public FollowerManager(ILogger<FollowerManager> logger)
        {
            this.logger = logger;
        }

        public void AddFollower(IFollowerNode follower)
        {
            leadersToFollowers.Add(new Entry(follower));
        }

        public void RemoveFollower(IFollowerNode follower)
        {
            var count = leadersToFollowers.Count;
            for (int i = 0; i < count; ++i)
            {
                if (leadersToFollowers[i].Node == follower)
                {
                    leadersToFollowers.RemoveAt(i);
                    break;
                }
            }
        }

        public void LeaderMoved(in Vector3 location)
        {
            var distancePercent = (location - leaderStartLocation).length2() / characterDistance;
            if (distancePercent > 1.0f)
            {
                leaderStartLocation = location;
                var inFrontLocation = leaderStartLocation;

                logger.LogInformation("Rollover Leader at {0}", leaderStartLocation);

                foreach(var entry in leadersToFollowers)
                {
                    args.NewLocation = entry.EndPosition;
                    entry.Node.UpdateLocation(args);
                    entry.EndPosition = inFrontLocation;
                    inFrontLocation = entry.StartPosition = entry.Node.CurrentLocation;
                    entry.DistancePercent = 0.0f;
                }
            }
            else
            {
                foreach (var entry in leadersToFollowers)
                {
                    if (entry.DistancePercent < distancePercent)
                    {
                        args.NewLocation = entry.StartPosition.lerp(entry.EndPosition, distancePercent);
                        entry.Node.UpdateLocation(args);
                        entry.DistancePercent = distancePercent;
                    }
                }
            }
        }
    }
}
