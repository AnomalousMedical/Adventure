using DiligentEngine;
using DiligentEngine.RT;
using Engine;
using RogueLikeMapBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DungeonGenerator
{
    public class IslandMazeMesh
    {
        private readonly csIslandMaze mapbuilder;
        public csIslandMaze MapBuilder => mapbuilder;

        private MeshBLAS floorMesh;

        /// <summary>
        /// The number of units on the generated map to make on the real map in the X direction.
        /// </summary>
        public float MapUnitX { get; } = 2f;

        /// <summary>
        /// The number of units on the generated map to make on the real map in the Y direction.
        /// </summary>
        public float MapUnitY { get; } = 2f;

        /// <summary>
        /// The number of units on the generated map to make on the real map in the Z direction.
        /// </summary>
        public float MapUnitZ { get; } = 2f;

        public IslandMazeMesh(csIslandMaze mapbuilder, Random random, MeshBLAS floorMesh, float mapUnitX = 2f, float mapUnitY = 2f, float mapUnitZ = 2f)
        {
            this.floorMesh = floorMesh;

            MapUnitX = mapUnitX;
            MapUnitY = mapUnitY;
            MapUnitZ = mapUnitZ;

            var halfUnitX = MapUnitX / 2.0f;
            var halfUnitY = MapUnitY / 2.0f;
            var halfUnitZ = MapUnitZ / 2.0f;
            var mapWidth = mapbuilder.MapX;
            var mapHeight = mapbuilder.MapY;

            var map = mapbuilder.Map;

            //Make mesh
            floorMesh.Begin((uint)(mapWidth * mapHeight));

            for (int mapY = mapHeight - 1; mapY > -1; --mapY)
            {
                for (int mapX = 0; mapX < mapWidth; ++mapX)
                {
                    var currentCell = map[mapX, mapY];

                    var left = mapX * MapUnitX;
                    var right = left + MapUnitX;
                    var far = mapY * MapUnitZ;
                    var near = far - MapUnitZ;
                    var y = 0f;
                    if (currentCell != csMapbuilder.EmptyCell)
                    {
                        y = 1f;
                    }

                    var topLeft = new Vector2(0, 0);
                    var bottomRight = new Vector2(1, 1);
                    var floorNormal = Vector3.Up;

                    floorMesh.AddQuad(
                      new Vector3(left, y, far),
                      new Vector3(right, y, far),
                      new Vector3(right, y, near),
                      new Vector3(left, y, near),
                      floorNormal,
                      floorNormal,
                      floorNormal,
                      floorNormal,
                      topLeft,
                      bottomRight);
                }
            }
        }

        public MeshBLAS FloorMesh => floorMesh;
    }
}
