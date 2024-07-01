using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonGenerator
{
    public class MapMeshCollisionPosition
    {
        public MapMeshCollisionPosition(in Vector3 topLeft, in Vector3 topRight, in Vector3 bottomRight, in Vector3 bottomLeft)
        {
            TopLeft = topLeft;
            TopRight = topRight;
            BottomRight = bottomRight;
            BottomLeft = bottomLeft;
        }

        public Vector3 TopLeft { get; }
        public Vector3 TopRight { get; }
        public Vector3 BottomRight { get; }
        public Vector3 BottomLeft { get; }
    }
}
