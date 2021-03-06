﻿using DiligentEngine;
using DiligentEngine.GltfPbr.Shapes;
using Engine;
using RogueLikeMapBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DungeonGenerator
{
    public class MapMesh : IDisposable
    {
        private Mesh floorMesh;
        private Mesh wallMesh;
        private List<MapMeshPosition> floorCubeCenterPoints;
        private List<Vector3> boundaryCubeCenterPoints;
        private MapMeshSquareInfo[,] squareInfo; //This array is 1 larger in each dimension, use accessor to translate points

        public IEnumerable<MapMeshPosition> FloorCubeCenterPoints => floorCubeCenterPoints;

        public IEnumerable<Vector3> BoundaryCubeCenterPoints => boundaryCubeCenterPoints;

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

        public float MaxSlopeY { get; set; } = 1f;

        public MapMesh(csMapbuilder mapbuilder, Random random, IRenderDevice renderDevice, float mapUnitX = 2f, float mapUnitY = 2f, float mapUnitZ = 2f)
        {
            if (mapbuilder.AllowOtherCorridors)
            {
                //The 'other' connection corridors between rooms that are not part of the main pathway for rooms
                //are not supported by this algorithm since z can end up way off.
                throw new InvalidOperationException($"Cannot create map meshes for MapBuilders with '{nameof(csMapbuilder.AllowOtherCorridors)}' turned on.");
            }

            MapUnitX = mapUnitX;
            MapUnitY = mapUnitY;
            MapUnitZ = mapUnitZ;

            var halfUnitX = MapUnitX / 2.0f;
            var halfUnitY = MapUnitY / 2.0f;
            var halfUnitZ = MapUnitZ / 2.0f;
            var mapWidth = mapbuilder.Map_Size.Width;
            var mapHeight = mapbuilder.Map_Size.Height;

            var map = mapbuilder.map;
            var slopeMap = new Slope[mapWidth, mapHeight];

            IntVector2 previousCorridor = new IntVector2();
            UInt16 currentCorridor = 0;
            bool[] seenRooms = new bool[mapbuilder.Rooms.Count];
            float corridorSlope = 0; //This will be changed before its used
            foreach (var corridor in mapbuilder.Corridors)
            {
                var mapX = corridor.x;
                var mapY = corridor.y;
                var cellType = map[mapX, mapY];

                var north = GetNorthSquare(mapX, mapY, map, mapHeight);
                var south = GetSouthSquare(mapX, mapY, map);
                var east = GetEastSquare(mapX, mapY, map, mapWidth);
                var west = GetWestSquare(mapX, mapY, map);

                //New corridor, find new starting point
                if (cellType != currentCorridor)
                {
                    corridorSlope = (float)random.NextDouble() * random.Next(2) == 0 ? -1f : 1f;
                    currentCorridor = cellType;

                    if (north != currentCorridor && north != csMapbuilder.EmptyCell)
                    {
                        previousCorridor = new IntVector2(mapX, mapY + 1);
                    }
                    else
                    {
                        if (south != currentCorridor && south != csMapbuilder.EmptyCell)
                        {
                            previousCorridor = new IntVector2(mapX, mapY - 1);
                        }
                        else
                        {
                            if (east != currentCorridor && east != csMapbuilder.EmptyCell)
                            {
                                previousCorridor = new IntVector2(mapX + 1, mapY);
                            }
                            else
                            {
                                if (west != currentCorridor && west != csMapbuilder.EmptyCell)
                                {
                                    previousCorridor = new IntVector2(mapX - 1, mapY);
                                }
                            }
                        }
                    }
                }

                var slope = new Slope()
                {
                    PreviousPoint = previousCorridor
                };
                previousCorridor = corridor;
                if (north == cellType || north == csMapbuilder.EmptyCell)
                {
                    if (south == cellType || south == csMapbuilder.EmptyCell)
                    {
                        if (east == cellType || east == csMapbuilder.EmptyCell)
                        {
                            if (west == cellType || west == csMapbuilder.EmptyCell)
                            {
                                //Allowed to slope if going in 1 direction, not a corner
                                if ((north == cellType && south == cellType)
                                    || (east == cellType && west == cellType))
                                {
                                    slope.YOffset = corridorSlope;
                                }
                            }
                        }
                    }
                }

                //Check for terminating rooms
                if(north < csMapbuilder.CorridorCell && north >= csMapbuilder.RoomCell && !seenRooms[north - csMapbuilder.RoomCell])
                {
                    seenRooms[north - csMapbuilder.RoomCell] = true;
                    SetRoomPrevious(mapbuilder.Rooms[north - csMapbuilder.RoomCell], corridor, slopeMap);
                }
                if (south < csMapbuilder.CorridorCell && south >= csMapbuilder.RoomCell && !seenRooms[south - csMapbuilder.RoomCell])
                {
                    seenRooms[south - csMapbuilder.RoomCell] = true;
                    SetRoomPrevious(mapbuilder.Rooms[south - csMapbuilder.RoomCell], corridor, slopeMap);
                }
                if (east < csMapbuilder.CorridorCell && east >= csMapbuilder.RoomCell && !seenRooms[east - csMapbuilder.RoomCell])
                {
                    seenRooms[east - csMapbuilder.RoomCell] = true;
                    SetRoomPrevious(mapbuilder.Rooms[east - csMapbuilder.RoomCell], corridor, slopeMap);
                }
                if (west < csMapbuilder.CorridorCell && west >= csMapbuilder.RoomCell && !seenRooms[west - csMapbuilder.RoomCell])
                {
                    seenRooms[west - csMapbuilder.RoomCell] = true;
                    SetRoomPrevious(mapbuilder.Rooms[west - csMapbuilder.RoomCell], corridor, slopeMap);
                }

                slopeMap[mapX, mapY] = slope;
            }

            var squareCenterMapWidth = mapWidth + 2;
            var squareCenterMapHeight = mapHeight + 2;
            squareInfo = new MapMeshSquareInfo[squareCenterMapWidth, squareCenterMapHeight];

            this.floorMesh = new Mesh();
            this.wallMesh = new Mesh();

            //Build map from the bottom up since camera always faces north
            //This will allow depth buffer to cancel pixel shaders

            //Figure out number of quads
            uint numFloorQuads = 0;
            uint numWallQuads = 0;
            uint numBoundaryCubes = 0;
            uint numFloorCubes = 0;
            for (int mapY = 0; mapY < mapHeight; ++mapY)
            {
                for (int mapX = 0; mapX < mapWidth; ++mapX)
                {
                    if (map[mapX, mapY] >= csMapbuilder.RoomCell)
                    {
                        ++numFloorQuads;
                        ++numFloorCubes;

                        int test;

                        //South wall
                        test = mapY - 1;
                        if (test < 0 || map[mapX, test] == csMapbuilder.EmptyCell)
                        {
                            ++numBoundaryCubes;
                        }

                        //North wall
                        test = mapY + 1;
                        if (test >= mapHeight || map[mapX, test] == csMapbuilder.EmptyCell)
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
            floorCubeCenterPoints = new List<MapMeshPosition>((int)(numFloorCubes));

            float yUvBottom = 1.0f;
            if (MapUnitY < 1.0f)
            {
                yUvBottom = MapUnitY / MapUnitX;
            }

            var processedSquares = new bool[mapWidth, mapHeight]; //Its too hard to prevent duplicates from the source just record if a room is done
            var firstCorridor = mapbuilder.Corridors[0];
            currentCorridor = map[firstCorridor.x, firstCorridor.y];
            ProcessRoom(mapbuilder, halfUnitX, halfUnitY, halfUnitZ, mapWidth, mapHeight, map, slopeMap, yUvBottom, processedSquares, 0);
            foreach (var corridor in mapbuilder.Corridors)
            {
                var mapX = corridor.x;
                var mapY = corridor.y;
                var cellType = map[mapX, mapY];
                if (cellType != currentCorridor)
                {
                    //Check for terminating room
                    var roomId = mapbuilder.GetCorridorTerminatingRoom(currentCorridor);
                    if(roomId != csMapbuilder.CorridorCell)
                    {
                        ProcessRoom(mapbuilder, halfUnitX, halfUnitY, halfUnitZ, mapWidth, mapHeight, map, slopeMap, yUvBottom, processedSquares, roomId);
                    }

                    currentCorridor = cellType;
                }

                if (!processedSquares[mapX, mapY])
                {
                    processedSquares[mapX, mapY] = true;
                    ProcessSquare(halfUnitX, halfUnitY, halfUnitZ, mapWidth, mapHeight, map, slopeMap, yUvBottom, mapX, mapY);
                }
            }
            UInt16 lastRoomId = (UInt16)(mapbuilder.Rooms.Count - 1);
            ProcessRoom(mapbuilder, halfUnitX, halfUnitY, halfUnitZ, mapWidth, mapHeight, map, slopeMap, yUvBottom, processedSquares, lastRoomId);

            //Figure out heights for remaining squares, which should be just empty squares
            //Walk vertical
            var walkHeight = mapHeight + 1;
            for (int mapX = 0; mapX < mapWidth; ++mapX)
            {
                var currentCell = map[mapX, 0];
                var emptyCellStart = -1;
                for (int mapY = 0; mapY < walkHeight; ++mapY)
                {
                    var cellType = mapY == mapHeight ? UInt16.MaxValue : map[mapX, mapY];
                    if (cellType != currentCell)
                    {
                        if (currentCell == csMapbuilder.EmptyCell) //Coming from an empty cell
                        {
                            var end = squareInfo[mapX + 1, mapY + 1];
                            var start = squareInfo[mapX + 1, emptyCellStart + 1];
                            float yOffset = (end.Center.y - start.Center.y) / (mapY - emptyCellStart - 1);
                            float realHalfY = yOffset / 2f;
                            for (var walk = emptyCellStart + 1; walk < mapY; ++walk)
                            {
                                slopeMap[mapX, walk] = new Slope()
                                {
                                    PreviousPoint = new IntVector2(mapX, walk - 1), //This is safe since previous points are read from an array 1 size larger
                                    YOffset = yOffset
                                };
                                //var left = mapX * MapUnitX;
                                //var far = mapY * MapUnitZ;
                                //var centerY = yOffset * walk;

                                //squareInfo[mapX + 1, mapY + 1] = new MapMeshSquareInfo(new Vector3(left + halfUnitX, centerY, far - halfUnitZ), realHalfY);
                            }
                        }
                        else if (cellType == csMapbuilder.EmptyCell) //Going to an empty cell
                        {
                            emptyCellStart = mapY - 1;
                        }

                        currentCell = cellType;
                    }
                }
            }

            //Walk Horizontal
            var walkWidth = mapWidth + 1;
            //for (int mapY = 0; mapY < mapHeight; ++mapY)
            //{
            //    var currentCell = map[0, mapY];
            //    var emptyCellStart = -1;
            //    for (int mapX = 0; mapX < walkWidth; ++mapX)
            //    {
            //        var cellType = mapX == mapWidth ? UInt16.MaxValue : map[mapX, mapY];
            //        if (cellType != currentCell)
            //        {
            //            if (currentCell == csMapbuilder.EmptyCell) //Coming from an empty cell
            //            {
            //                var end = squareInfo[mapX + 1, mapY + 1];
            //                var start = squareInfo[emptyCellStart + 1, mapY + 1];
            //                float yOffset = (end.Center.y - start.Center.y) / (mapX - emptyCellStart - 1);
            //                float realHalfY = yOffset / 2f;
            //                for (var walk = emptyCellStart + 1; walk < mapX; ++walk)
            //                {
            //                    slopeMap[walk, mapY] = new Slope()
            //                    {
            //                        PreviousPoint = new IntVector2(walk - 1, mapY), //This is safe since previous points are read from an array 1 size larger
            //                        YOffset = yOffset
            //                    };
            //                    //var left = mapX * MapUnitX;
            //                    //var far = mapY * MapUnitZ;
            //                    //var centerY = yOffset * walk;

            //                    //squareInfo[mapX + 1, mapY + 1] = new MapMeshSquareInfo(new Vector3(left + halfUnitX, centerY, far - halfUnitZ), realHalfY);
            //                }
            //            }
            //            else if (cellType == csMapbuilder.EmptyCell) //Going to an empty cell
            //            {
            //                emptyCellStart = mapX - 1;
            //            }

            //            currentCell = cellType;
            //        }
            //    }
            //}

            //Render remaining squares that have not been processed
            for (int mapY = 0; mapY < mapHeight; ++mapY)
            {
                for (int mapX = 0; mapX < mapWidth; ++mapX)
                {
                    if (!processedSquares[mapX, mapY])
                    {
                        ProcessSquare(halfUnitX, halfUnitY, halfUnitZ, mapWidth, mapHeight, map, slopeMap, yUvBottom, mapX, mapY);
                    }
                }
            }

            floorMesh.End(renderDevice);
            wallMesh.End(renderDevice);
        }

        private void ProcessRoom(csMapbuilder mapbuilder, float halfUnitX, float halfUnitY, float halfUnitZ, int mapWidth, int mapHeight, ushort[,] map, Slope[,] slopeMap, float yUvBottom, bool[,] processedSquares, ushort roomId)
        {
            var room = mapbuilder.Rooms[roomId];
            var bottom = room.Bottom;
            var right = room.Right;
            for (var roomY = room.Top; roomY <= bottom; ++roomY)
            {
                for (var roomX = room.Left; roomX <= right; ++roomX)
                {
                    if (!processedSquares[roomX, roomY])
                    {
                        processedSquares[roomX, roomY] = true;
                        ProcessSquare(halfUnitX, halfUnitY, halfUnitZ, mapWidth, mapHeight, map, slopeMap, yUvBottom, roomX, roomY);
                    }
                }
            }
        }

        private void ProcessSquare(float halfUnitX, float halfUnitY, float halfUnitZ, int mapWidth, int mapHeight, ushort[,] map, Slope[,] slopeMap, float yUvBottom, int mapX, int mapY)
        {
            //This is a bit odd, corridors previous points are the previous square, but room previous points are their terminating corrdor square
            //This will work ok with some of the calculations below since rooms are always 0 rotation anyway

            var left = mapX * MapUnitX;
            var right = left + MapUnitX;
            var far = mapY * MapUnitZ;
            var near = far - MapUnitZ;

            var slope = slopeMap[mapX, mapY];
            var previousOffset = slope.PreviousPoint - new IntVector2(mapX, mapY);
            bool positivePrevious = previousOffset.x > 0 || previousOffset.y > 0;

            var realHalfY = slope.YOffset / 2f;
            float halfYOffset = Math.Abs(realHalfY);

            bool xDir = previousOffset.x != 0;
            float xInfluence = xDir ? 1 : 0; //1 for x 0 for y
            float yInfluence = 1.0f - xInfluence;

            float xHeightStep = slope.YOffset * xInfluence;
            float yHeightStep = slope.YOffset * yInfluence;

            Vector3 dirInfluence = new Vector3(xHeightStep, 0, yHeightStep).normalized();
            Vector3 floorCubeRotationVec = new Vector3(halfUnitX * dirInfluence.x, halfYOffset, halfUnitZ * dirInfluence.z).normalized();
            var floorCubeRot = Quaternion.shortestArcQuat(ref dirInfluence, ref floorCubeRotationVec);
            if (positivePrevious)
            {
                floorCubeRot = floorCubeRot.inverse();
            }

            //Get previous square center
            var previousSlope = squareInfo[slope.PreviousPoint.x + 1, slope.PreviousPoint.y + 1];

            var totalYOffset = previousSlope.HalfYOffset + realHalfY;
            var centerY = previousSlope.Center.y + totalYOffset;

            //Update our center point in the slope grid
            squareInfo[mapX + 1, mapY + 1] = new MapMeshSquareInfo(new Vector3(left + halfUnitX, centerY, far - halfUnitZ), realHalfY);

            var floorY = centerY - halfUnitY;
            float floorFarLeftY = 0;
            float floorFarRightY = 0;
            float floorNearRightY = 0;
            float floorNearLeftY = 0;
            if (slope.YOffset > 0 && !positivePrevious || slope.YOffset < 0 && positivePrevious)
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

                floorCubeCenterPoints.Add(new MapMeshPosition(new Vector3(left + halfUnitX, floorY - halfUnitY, far - halfUnitZ), floorCubeRot));

                int test;

                //South wall
                test = mapY - 1;
                if (test < 0 || map[mapX, test] == csMapbuilder.EmptyCell)
                {
                    //No mesh needed here, can't see it
                    boundaryCubeCenterPoints.Add(new Vector3(left + halfUnitX, centerY, near - halfUnitZ));
                }

                //North wall
                test = mapY + 1;
                if (test >= mapHeight || map[mapX, test] == csMapbuilder.EmptyCell)
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

        public Vector3 PointToVector(int x, int y)
        {
            return squareInfo[x + 1, y + 1].Center;
        }

        public void Dispose()
        {
            this.floorMesh.Dispose();
            this.wallMesh.Dispose();
        }

        public Mesh FloorMesh => floorMesh;

        public Mesh WallMesh => wallMesh;

        private UInt16 GetNorthSquare(int x, int y, ushort[,] map, int height)
        {
            y += 1;
            if (y >= height)
            {
                return csMapbuilder.EmptyCell;
            }
            return map[x, y];
        }

        private UInt16 GetSouthSquare(int x, int y, ushort[,] map)
        {
            y -= 1;
            if (y < 0)
            {
                return csMapbuilder.EmptyCell;
            }
            return map[x, y];
        }

        private UInt16 GetWestSquare(int x, int y, ushort[,] map)
        {
            x -= 1;
            if (x < 0)
            {
                return csMapbuilder.EmptyCell;
            }
            return map[x, y];
        }

        private UInt16 GetEastSquare(int x, int y, ushort[,] map, int width)
        {
            x += 1;
            if (x >= width)
            {
                return csMapbuilder.EmptyCell;
            }
            return map[x, y];
        }

        private void SetRoomPrevious(IntRect room, in IntVector2 previous, Slope[,] slopeMap)
        {
            var slope = new Slope()
            {
                PreviousPoint = previous
            };

            //Set the slope to be the same for everything in the room, note that this makes the previous point for the whole room the same terminating ramp point
            var bottom = room.Bottom;
            var right = room.Right;
            for(var y = room.Top; y <= bottom; ++y)
            {
                for(var x = room.Left; x <= right; ++x)
                {
                    slopeMap[x, y] = slope;
                }
            }
        }
    }
}
