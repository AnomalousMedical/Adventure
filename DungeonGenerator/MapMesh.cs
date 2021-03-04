﻿using DiligentEngine;
using DiligentEngine.GltfPbr.Shapes;
using Engine;
using RogueLikeMapBuilder;
using System;
using System.Collections.Generic;
using System.Text;

namespace DungeonGenerator
{
    public class MapMesh : IDisposable
    {
        private Mesh floorMesh;
        private Mesh wallMesh;
        private List<Vector3> floorCubeCenterPoints;
        private List<Vector3> boundaryCubeCenterPoints;
        private Quaternion floorCubeRot;

        public IEnumerable<Vector3> FloorCubeCenterPoints => floorCubeCenterPoints;

        public IEnumerable<Vector3> BoundaryCubeCenterPoints => boundaryCubeCenterPoints;

        public Quaternion FloorCubeRot => floorCubeRot;

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

        public MapMesh(csMapbuilder mapbuilder, IRenderDevice renderDevice, float mapUnitX = 2f, float mapUnitY = 2f, float mapUnitZ = 2f)
        {
            MapUnitX = mapUnitX;
            MapUnitY = mapUnitY;
            MapUnitZ = mapUnitZ;

            var halfUnitX = MapUnitX / 2.0f;
            var halfUnitY = MapUnitY / 2.0f;
            var halfUnitZ = MapUnitZ / 2.0f;
            var mapWidth = mapbuilder.Map_Size.Width;
            var mapHeight = mapbuilder.Map_Size.Height;

            var map = mapbuilder.map;
            var slopeMapWidth = mapWidth + 2;
            var slopeMapHeight = mapHeight + 2;
            var slopeMap = new Slope[slopeMapWidth, slopeMapHeight];

            for (int mapY = slopeMapHeight - 1; mapY > -1; --mapY)
            {
                for (int mapX = 0; mapX < slopeMapWidth; ++mapX)
                {
                    slopeMap[mapX, mapY] = new Slope()
                    {
                        //PreviousPoint = new IntVector2(-1, 0), //for x-flow use -1 the gird is calculated from left to right
                        PreviousPoint = new IntVector2(0, 1), //for y-flow use 1 the grid is calcualated from height to 0
                        YOffset = 0.3f
                    };
                }
            }

            this.floorMesh = new Mesh();
            this.wallMesh = new Mesh();

            //Build map from the bottom up since camera always faces north
            //This will allow depth buffer to cancel pixel shaders

            //Figure out number of quads
            uint numFloorQuads = 0;
            uint numWallQuads = 0;
            uint numBoundaryCubes = 0;
            uint numFloorCubes = 0;
            for (int mapY = mapHeight - 1; mapY > -1; --mapY)
            {
                for (int mapX = 0; mapX < mapWidth; ++mapX)
                {
                    if (map[mapX, mapY] >= csMapbuilder.RoomCell)
                    {
                        ++numFloorQuads;
                        ++numFloorCubes;

                        int test;

                        //South wall
                        test = mapY + 1;
                        if (test >= mapHeight || map[mapX, test] == csMapbuilder.EmptyCell)
                        {
                            ++numBoundaryCubes;
                        }

                        //North wall
                        test = mapY - 1;
                        if (test < 0 || map[mapX, test] == csMapbuilder.EmptyCell)
                        {
                            ++numWallQuads;
                            ++numBoundaryCubes;
                        }

                        //West wall
                        test = mapX - 1;
                        if (test < 0 || map[test, mapY] == csMapbuilder.EmptyCell)
                        {
                            ++numWallQuads;
                            ++numBoundaryCubes;
                        }

                        //East wall
                        test = mapX + 1;
                        if (test >= mapWidth || map[test, mapY] == csMapbuilder.EmptyCell)
                        {
                            ++numWallQuads;
                            ++numBoundaryCubes;
                        }
                    }
                    else
                    {
                        ++numWallQuads;
                    }
                }
            }

            //Make mesh
            floorMesh.Begin(numFloorQuads);
            wallMesh.Begin(numWallQuads);

            boundaryCubeCenterPoints = new List<Vector3>((int)(numBoundaryCubes));
            floorCubeCenterPoints = new List<Vector3>((int)(numFloorCubes));

            float yUvBottom = 1.0f;
            if (MapUnitY < 1.0f)
            {
                yUvBottom = MapUnitY / MapUnitX;
            }

            for (int mapY = mapHeight - 1; mapY > -1; --mapY)
            {
                for (int mapX = 0; mapX < mapWidth; ++mapX)
                {
                    var left = mapX * MapUnitX;
                    var right = left + MapUnitX;
                    var far = (mapHeight - mapY) * MapUnitZ;
                    var near = far - MapUnitZ;

                    var slopeMapX = mapX + 1;
                    var slopeMapY = mapY + 1;

                    var slope = slopeMap[slopeMapX, slopeMapY];
                    bool yIncreasing = slope.YOffset > 0;

                    float halfYOffset = Math.Abs(slope.YOffset / 2f);

                    bool xDir = slope.PreviousPoint.x != 0;
                    float xInfluence = xDir ? 1 : 0; //1 for x 0 for y
                    float yInfluence = 1.0f - xInfluence;

                    float xHeightStep = slope.YOffset * xInfluence;
                    float yHeightStep = slope.YOffset * yInfluence;

                    Vector3 dirInfluence = new Vector3(xHeightStep, 0, yHeightStep).normalized();
                    Vector3 floorCubeRotationVec = new Vector3(halfUnitX * dirInfluence.x, halfYOffset, halfUnitZ * dirInfluence.z).normalized();
                    floorCubeRot = Quaternion.shortestArcQuat(ref dirInfluence, ref floorCubeRotationVec);

                    //Get previous square center
                    var previousSquareX = slopeMapX + slope.PreviousPoint.x;
                    var previousSquareY = slopeMapY + slope.PreviousPoint.y;
                    var previousSlope = slopeMap[previousSquareX, previousSquareY];
                    var centerY = previousSlope.Center.y + slope.YOffset;

                    //Update our center point in the slope grid
                    slope.Center = new Vector3(left + halfUnitX, centerY, far - halfUnitZ);

                    var floorY = centerY - halfUnitY;
                    float floorFarLeftY = 0;
                    float floorFarRightY = 0;
                    float floorNearRightY = 0;
                    float floorNearLeftY = 0;
                    if (yIncreasing)
                    {
                        if (xDir)
                        {
                            floorFarLeftY = floorY - halfYOffset;
                            floorFarRightY = floorY + halfYOffset;
                            floorNearRightY = floorY + halfYOffset;
                            floorNearLeftY = floorY - halfYOffset;
                        }
                        else
                        {
                            floorFarLeftY = floorY + halfYOffset;
                            floorFarRightY = floorY + halfYOffset;
                            floorNearRightY = floorY - halfYOffset;
                            floorNearLeftY = floorY - halfYOffset;
                        }
                    }
                    else
                    {
                        if (xDir)
                        {

                            floorFarLeftY = floorY + halfYOffset;
                            floorFarRightY = floorY - halfYOffset;
                            floorNearRightY = floorY - halfYOffset;
                            floorNearLeftY = floorY + halfYOffset;
                        }
                        else
                        {
                            floorFarLeftY = floorY - halfYOffset;
                            floorFarRightY = floorY - halfYOffset;
                            floorNearRightY = floorY + halfYOffset;
                            floorNearLeftY = floorY + halfYOffset;
                        }
                    }

                    var floorNormal = Quaternion.quatRotate(floorCubeRot, Vector3.Up);

                    if (map[mapX, mapY] >= csMapbuilder.RoomCell)
                    {
                        //Floor
                        floorMesh.AddQuad(
                            new Vector3(left, floorFarLeftY, far),
                            new Vector3(right, floorFarRightY, far),
                            new Vector3(right, floorNearRightY, near),
                            new Vector3(left, floorNearLeftY, near),
                            floorNormal,
                            new Vector2(0, 0),
                            new Vector2(1, 1));

                        floorCubeCenterPoints.Add(new Vector3(left + halfUnitX, floorY - halfUnitY, far - halfUnitZ));

                        int test;

                        //South wall
                        test = mapY + 1;
                        if (test >= mapHeight || map[mapX, test] == csMapbuilder.EmptyCell)
                        {
                            //No mesh needed here, can't see it

                            boundaryCubeCenterPoints.Add(new Vector3(left + halfUnitX, centerY, near - halfUnitZ));
                        }

                        //North wall
                        test = mapY - 1;
                        if (test < 0 || map[mapX, test] == csMapbuilder.EmptyCell)
                        {
                            //Face backward too, north facing camera
                            wallMesh.AddQuad(
                                new Vector3(left, floorFarLeftY + MapUnitY, far),
                                new Vector3(right, floorFarRightY + MapUnitY, far),
                                new Vector3(right, floorFarRightY, far),
                                new Vector3(left, floorFarLeftY, far),
                                Vector3.Backward,
                                new Vector2(0, 0),
                                new Vector2(1, yUvBottom));

                            boundaryCubeCenterPoints.Add(new Vector3(left + halfUnitX, centerY, far + halfUnitZ));
                        }

                        //West wall
                        test = mapX - 1;
                        if (test < 0 || map[test, mapY] == csMapbuilder.EmptyCell)
                        {
                            wallMesh.AddQuad(
                                new Vector3(left, floorNearLeftY + MapUnitY, near),
                                new Vector3(left, floorFarLeftY + MapUnitY, far),
                                new Vector3(left, floorFarLeftY, far),
                                new Vector3(left, floorNearLeftY, near),
                                Vector3.Right,
                                new Vector2(0, 0),
                                new Vector2(1, yUvBottom));

                            boundaryCubeCenterPoints.Add(new Vector3(left - halfUnitX, centerY, near + halfUnitZ));
                        }

                        //East wall
                        test = mapX + 1;
                        if (test >= mapWidth || map[test, mapY] == csMapbuilder.EmptyCell)
                        {
                            wallMesh.AddQuad(
                                new Vector3(right, floorFarRightY + MapUnitY, far),
                                new Vector3(right, floorNearRightY + MapUnitY, near),
                                new Vector3(right, floorNearRightY, near),
                                new Vector3(right, floorFarRightY, far),
                                Vector3.Left,
                                new Vector2(0, 0),
                                new Vector2(1, 1));

                            boundaryCubeCenterPoints.Add(new Vector3(right + halfUnitX, centerY, near + halfUnitZ));
                        }
                    }
                    else
                    {
                        //Floor outside
                        wallMesh.AddQuad(
                            new Vector3(left, floorFarLeftY + MapUnitY, far),
                            new Vector3(right, floorFarRightY + MapUnitY, far),
                            new Vector3(right, floorNearRightY + MapUnitY, near),
                            new Vector3(left, floorNearLeftY + MapUnitY, near),
                            floorNormal,
                            new Vector2(0, 0),
                            new Vector2(1, 1));
                    }
                }
            }

            floorMesh.End(renderDevice);
            wallMesh.End(renderDevice);
        }

        public Vector3 PointToVector(int x, int y)
        {
            var left = x * MapUnitX;
            var far = y * MapUnitZ;
            return new Vector3(left + MapUnitX / 2f, MapUnitY / 2f, far - MapUnitZ / 2f);
        }

        public void Dispose()
        {
            this.floorMesh.Dispose();
            this.wallMesh.Dispose();
        }

        public Mesh FloorMesh => floorMesh;

        public Mesh WallMesh => wallMesh;
    }
}
