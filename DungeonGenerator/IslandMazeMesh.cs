﻿using DiligentEngine;
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
        private List<MapMeshCollisionPosition> collisionMeshPositions;
        private List<MapMeshPosition> floorCubeCenterPoints; //TODO: these are not populated correctly, but are needed for physics
        private List<Vector3> boundaryCubeCenterPoints; //TODO: these are not populated correctly, but are needed for physics
        private MapMeshSquareInfo[,] squareInfo; //This array is 1 larger in each dimension, use accessor to translate points

        private readonly float uvXStride;
        private readonly float uvYStride;

        public IEnumerable<MapMeshPosition> FloorCubeCenterPoints => floorCubeCenterPoints;

        public IEnumerable<Vector3> BoundaryCubeCenterPoints => boundaryCubeCenterPoints;

        public IEnumerable<MapMeshCollisionPosition> CollisionMeshPositions => collisionMeshPositions;

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

        private float wallTextureIndex = 1.5f;
        public int WallTextureIndex
        {
            get
            {
                return (int)wallTextureIndex;
            }
            set
            {
                wallTextureIndex = value + 0.5f;
            }
        }

        private float lowerGroundTextureIndex = 2.5f;
        public int LowerGroundTextureIndex
        {
            get
            {
                return (int)lowerGroundTextureIndex;
            }
            set
            {
                lowerGroundTextureIndex = value + 0.5f;
            }
        }

        public IslandMazeMesh(csIslandMaze mapbuilder, MeshBLAS floorMesh, float mapUnitX = 2f, float mapUnitY = 2f, float mapUnitZ = 2f)
        {
            this.mapbuilder = mapbuilder;
            this.floorMesh = floorMesh;
            MapUnitX = mapUnitX;
            MapUnitY = mapUnitY;
            MapUnitZ = mapUnitZ;

            //Use the ratio of z to x to determine the uv stride
            uvXStride = 1.0f;
            uvYStride = uvXStride * (mapUnitZ / mapUnitX);
        }

        public void Build() 
        { 
            var halfUnitX = MapUnitX / 2.0f;
            var halfUnitY = MapUnitY / 2.0f;
            var halfUnitZ = MapUnitZ / 2.0f;
            var mapWidth = mapbuilder.MapX;
            var mapHeight = mapbuilder.MapY;

            var map = mapbuilder.Map;

            for (int mapY = mapHeight - 1; mapY > -1; --mapY)
            {
                var previousCell = csIslandMaze.EmptyCell;
                for (int mapX = 0; mapX < mapWidth; ++mapX)
                {
                    var slope = new Slope();
                    var currentCell = map[mapX, mapY];
                    
                    if (currentCell != previousCell)
                    {
                        if (currentCell != csIslandMaze.EmptyCell)
                        {
                            slope.YOffset = 3;
                        }
                        else if (currentCell == csIslandMaze.EmptyCell)
                        {
                            slope.YOffset = -3;
                        }
                    }
                    
                    slope.PreviousPoint = new IntVector2(mapX - 1, mapY);
                    if (slope.PreviousPoint.x < 0)
                    {
                        slope.PreviousPoint = new IntVector2(0, mapY);
                    }

                    previousCell = currentCell;
                }
            }

            var squareCenterMapWidth = mapWidth + 2;
            var squareCenterMapHeight = mapHeight + 2;
            squareInfo = new MapMeshSquareInfo[squareCenterMapWidth, squareCenterMapHeight];
            MapMeshTempSquareInfo[,] tempSquareInfo = new MapMeshTempSquareInfo[squareCenterMapWidth, squareCenterMapHeight];

            //Figure out number of quads
            uint numFloorQuads = 0;
            uint numBoundaryCubes = 0;
            uint numFloorCubes = 0;
            for (int mapY = 0; mapY < mapHeight; ++mapY)
            {
                for (int mapX = 0; mapX < mapWidth; ++mapX)
                {
                    ++numFloorQuads;
                    if (map[mapX, mapY] >= csMapbuilder.RoomCell)
                    {
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
                            ++numBoundaryCubes;
                        }

                        //West wall
                        test = mapX - 1;
                        if (test < 0 || map[test, mapY] == csMapbuilder.EmptyCell)
                        {
                            ++numBoundaryCubes;
                        }

                        //East wall
                        test = mapX + 1;
                        if (test >= mapWidth || map[test, mapY] == csMapbuilder.EmptyCell)
                        {
                            ++numBoundaryCubes;
                        }
                    }
                }
            }

            //Make mesh
            floorMesh.Begin(numFloorQuads);

            boundaryCubeCenterPoints = new List<Vector3>((int)(numBoundaryCubes));
            floorCubeCenterPoints = new List<MapMeshPosition>((int)(numFloorCubes));
            collisionMeshPositions = new List<MapMeshCollisionPosition>((int)(numFloorQuads));

            float yUvBottom = 1.0f;
            if (MapUnitY < 1.0f)
            {
                yUvBottom = MapUnitY / MapUnitX;
            }

            //Walk all corridors and rooms, this forms the baseline heightmap
            var processedSquares = new bool[mapWidth, mapHeight]; //Its too hard to prevent duplicates from the source just record if a room is done

            for (int mapY = mapHeight - 1; mapY > -1; --mapY)
            {
                for (int mapX = 0; mapX < mapWidth; ++mapX)
                {
                    if (map[mapX, mapY] != csMapbuilder.EmptyCell)
                    {
                        processedSquares[mapX, mapY] = true;
                        ProcessSquare(MapUnitY, mapbuilder.TextureOffsets[mapX, mapY] + 0.5f, halfUnitX, halfUnitY, halfUnitZ, mapWidth, mapHeight, map, tempSquareInfo, yUvBottom, mapX, mapY, mapbuilder, true);
                    }
                    else
                    {
                        var north = GetNorthSquare(mapX, mapY, map, mapHeight);
                        var south = GetSouthSquare(mapX, mapY, map);
                        var east = GetEastSquare(mapX, mapY, map, mapWidth);
                        var west = GetWestSquare(mapX, mapY, map);
                        var northEast = GetNorthEastSquare(mapX, mapY, map, mapWidth, mapHeight);
                        var southEast = GetSouthEastSquare(mapX, mapY, map, mapWidth);
                        var northWest = GetNorthWestSquare(mapX, mapY, map, mapHeight);
                        var southWest = GetSouthWestSquare(mapX, mapY, map);

                        if(north == csMapbuilder.EmptyCell
                        && south == csMapbuilder.EmptyCell
                        && east == csMapbuilder.EmptyCell
                        && west == csMapbuilder.EmptyCell
                        && northEast == csMapbuilder.EmptyCell
                        && southEast == csMapbuilder.EmptyCell
                        && northWest == csMapbuilder.EmptyCell
                        && southWest == csMapbuilder.EmptyCell
                        )
                        {
                            processedSquares[mapX, mapY] = true;
                            ProcessSquare(0.0f, lowerGroundTextureIndex, halfUnitX, halfUnitY, halfUnitZ, mapWidth, mapHeight, map, tempSquareInfo, yUvBottom, mapX, mapY, mapbuilder, false);
                        }
                    }
                }
            }

            //Figure out location for remaining squares
            for (int mapY = 0; mapY < mapHeight; ++mapY)
            {
                for (int mapX = 0; mapX < mapWidth; ++mapX)
                {
                    if (!processedSquares[mapX, mapY])
                    {
                        var square = tempSquareInfo[mapX + 1, mapY + 1];
                        {
                            bool finished = false;
                            float accumulatedY = square.LeftFarY;
                            float denominator = 1f;
                            var testX = mapX - 1;
                            var testY = mapY + 1;
                            if (testY < mapHeight)
                            {
                                var north = map[mapX, testY];
                                var testSquare = tempSquareInfo[mapX + 1, testY + 1];
                                var northY = testSquare.LeftNearY;
                                if (north != csMapbuilder.EmptyCell || testSquare.Visited)
                                {
                                    square.LeftFarY = northY;
                                    finished = true;
                                }
                                else
                                {
                                    accumulatedY += northY;
                                    ++denominator;
                                }
                            }
                            if (!finished && testX > 0)
                            {
                                var west = map[testX, mapY];
                                var testSquare = tempSquareInfo[testX + 1, mapY + 1];
                                var westY = testSquare.RightFarY;
                                if (west != csMapbuilder.EmptyCell || testSquare.Visited)
                                {
                                    square.LeftFarY = westY;
                                    finished = true;
                                }
                                else
                                {
                                    accumulatedY += westY;
                                    ++denominator;
                                }
                            }
                            if (!finished && testY < mapHeight && testX > 0)
                            {
                                var northWest = map[testX, testY];
                                var testSquare = tempSquareInfo[testX + 1, testY + 1];
                                var northWestY = testSquare.RightNearY;
                                if (northWest != csMapbuilder.EmptyCell || testSquare.Visited)
                                {
                                    square.LeftFarY = northWestY;
                                    finished = true;
                                }
                                else
                                {
                                    accumulatedY += northWestY;
                                    ++denominator;
                                }
                            }

                            if (!finished)
                            {
                                square.LeftFarY = accumulatedY / denominator;
                            }
                        }

                        {
                            bool finished = false;
                            float accumulatedY = square.RightFarY;
                            float denominator = 1f;
                            var testX = mapX + 1;
                            var testY = mapY + 1;
                            if (testY < mapHeight)
                            {
                                var north = map[mapX, testY];
                                var testSquare = tempSquareInfo[mapX + 1, testY + 1];
                                var northY = testSquare.RightNearY;
                                if (north != csMapbuilder.EmptyCell || testSquare.Visited)
                                {
                                    square.RightFarY = northY;
                                    finished = true;
                                }
                                else
                                {
                                    accumulatedY += northY;
                                    ++denominator;
                                }
                            }
                            if (!finished && testX < mapWidth)
                            {
                                var east = map[testX, mapY];
                                var testSquare = tempSquareInfo[testX + 1, mapY + 1];
                                var eastY = testSquare.LeftFarY;
                                if (east != csMapbuilder.EmptyCell || testSquare.Visited)
                                {
                                    square.RightFarY = eastY;
                                    finished = true;
                                }
                                else
                                {
                                    accumulatedY += eastY;
                                    ++denominator;
                                }
                            }
                            if (!finished && testY < mapHeight && testX < mapWidth)
                            {
                                var northEast = map[testX, testY];
                                var testSquare = tempSquareInfo[testX + 1, testY + 1];
                                var northEastY = testSquare.LeftNearY;
                                if (northEast != csMapbuilder.EmptyCell || testSquare.Visited)
                                {
                                    square.RightFarY = northEastY;
                                    finished = true;
                                }
                                else
                                {
                                    accumulatedY += northEastY;
                                    ++denominator;
                                }
                            }

                            if (!finished)
                            {
                                square.RightFarY = accumulatedY / denominator;
                            }
                        }

                        {
                            bool finished = false;
                            float accumulatedY = square.RightNearY;
                            float denominator = 1f;
                            var testX = mapX + 1;
                            var testY = mapY - 1;
                            if (testY > 0)
                            {
                                var south = map[mapX, testY];
                                var testSquare = tempSquareInfo[mapX + 1, testY + 1];
                                var southY = testSquare.RightFarY;
                                if (south != csMapbuilder.EmptyCell || testSquare.Visited)
                                {
                                    square.RightNearY = southY;
                                    finished = true;
                                }
                                else
                                {
                                    accumulatedY += southY;
                                    ++denominator;
                                }
                            }
                            if (!finished && testX < mapWidth)
                            {
                                var east = map[testX, mapY];
                                var testSquare = tempSquareInfo[testX + 1, mapY + 1];
                                var eastY = testSquare.LeftNearY;
                                if (east != csMapbuilder.EmptyCell || testSquare.Visited)
                                {
                                    square.RightNearY = eastY;
                                    finished = true;
                                }
                                else
                                {
                                    accumulatedY += eastY;
                                    ++denominator;
                                }
                            }
                            if (!finished && testY > 0 && testX < mapWidth)
                            {
                                var southEast = map[testX, testY];
                                var testSquare = tempSquareInfo[testX + 1, testY + 1];
                                var southEastY = testSquare.LeftFarY;
                                if (southEast != csMapbuilder.EmptyCell || testSquare.Visited)
                                {
                                    square.RightNearY = southEastY;
                                    finished = true;
                                }
                                else
                                {
                                    accumulatedY += southEastY;
                                    ++denominator;
                                }
                            }

                            if (!finished)
                            {
                                square.RightNearY = accumulatedY / denominator;
                            }
                        }

                        {
                            bool finished = false;
                            float accumulatedY = square.LeftNearY;
                            float denominator = 1f;
                            var testX = mapX - 1;
                            var testY = mapY - 1;
                            if (testY > 0)
                            {
                                var south = map[mapX, testY];
                                var testSquare = tempSquareInfo[mapX + 1, testY + 1];
                                var southY = testSquare.LeftFarY;
                                if (south != csMapbuilder.EmptyCell || testSquare.Visited)
                                {
                                    square.LeftNearY = southY;
                                    finished = true;
                                }
                                else
                                {
                                    accumulatedY += southY;
                                    ++denominator;
                                }
                            }
                            if (!finished && testX > 0)
                            {
                                var west = map[testX, mapY];
                                var testSquare = tempSquareInfo[testX + 1, mapY + 1];
                                var westY = testSquare.RightNearY;
                                if (west != csMapbuilder.EmptyCell || testSquare.Visited)
                                {
                                    square.LeftNearY = westY;
                                    finished = true;
                                }
                                else
                                {
                                    accumulatedY += westY;
                                    ++denominator;
                                }
                            }
                            if (!finished && testY > 0 && testX > 0)
                            {
                                var southWest = map[testX, testY];
                                var testSquare = tempSquareInfo[testX + 1, testY + 1];
                                var southWestY = testSquare.RightFarY;
                                if (southWest != csMapbuilder.EmptyCell || testSquare.Visited)
                                {
                                    square.LeftNearY = southWestY;
                                    finished = true;
                                }
                                else
                                {
                                    accumulatedY += southWestY;
                                    ++denominator;
                                }
                            }

                            if (!finished)
                            {
                                square.LeftNearY = accumulatedY / denominator;
                            }
                        }

                        square.Visited = true;
                        tempSquareInfo[mapX + 1, mapY + 1] = square;
                    }
                }
            }

            //Render remaining squares
            for (int mapY = 0; mapY < mapHeight; ++mapY)
            {
                for (int mapX = 0; mapX < mapWidth; ++mapX)
                {
                    if (!processedSquares[mapX, mapY])
                    {
                        RenderEmptySquare(tempSquareInfo, mapY, mapX, mapWidth, mapHeight);
                    }
                }
            }
        }

        public Vector3 PointToVector(int x, int y)
        {
            return squareInfo[x + 1, y + 1].Center;
        }

        public MeshBLAS FloorMesh => floorMesh;

        private void ProcessSquare(float centerY, float textureIndex, float halfUnitX, float halfUnitY, float halfUnitZ, int mapWidth, int mapHeight, int[,] map, MapMeshTempSquareInfo[,] tempSquareInfo, float yUvBottom, int mapX, int mapY, csIslandMaze mapbuilder, bool setAsFloor)
        {
            //This is a bit odd, corridors previous points are the previous square, but room previous points are their terminating corrdor square
            //This will work ok with some of the calculations below since rooms are always 0 rotation anyway

            var left = mapX * MapUnitX;
            var right = left + MapUnitX;
            var far = mapY * MapUnitZ;
            var near = far - MapUnitZ;

            //Update our center point in the slope grid
            var floorFarLeftY = centerY;
            var floorFarRightY = centerY;
            var floorNearRightY = centerY;
            var floorNearLeftY = centerY;
            var floorCubeRot = Quaternion.Identity;

            squareInfo[mapX + 1, mapY + 1] = new MapMeshSquareInfo(new Vector3(left + halfUnitX, centerY, far - halfUnitZ), centerY / 2.0f);
            tempSquareInfo[mapX + 1, mapY + 1] = new MapMeshTempSquareInfo
            (
                leftFarY: floorFarLeftY,
                rightFarY: floorFarRightY,
                rightNearY: floorNearRightY,
                leftNearY: floorNearLeftY
            );

            var floorNormal = Quaternion.quatRotate(floorCubeRot, Vector3.Up);

            //if (map[mapX, mapY] >= csMapbuilder.RoomCell)
            {
                //Floor
                GetUvs(mapX, mapY, out var topLeft, out var bottomRight, out var globalTopLeft, out var globalBottomRight, mapWidth, mapHeight);
                floorMesh.AddQuad(
                    new Vector3(left, floorFarLeftY, far),
                    new Vector3(right, floorFarRightY, far),
                    new Vector3(right, floorNearRightY, near),
                    new Vector3(left, floorNearLeftY, near),
                    floorNormal,
                    floorNormal,
                    floorNormal,
                    floorNormal,
                    topLeft,
                    bottomRight,
                    globalTopLeft,
                    globalBottomRight,
                    textureIndex);

                if (setAsFloor)
                {
                    floorCubeCenterPoints.Add(new MapMeshPosition(new Vector3(left + halfUnitX, centerY - halfUnitY, far - halfUnitZ), floorCubeRot));
                }

                collisionMeshPositions.Add(new MapMeshCollisionPosition(
                    new Vector3(left, floorFarLeftY, far),
                    new Vector3(right, floorFarRightY, far),
                    new Vector3(right, floorNearRightY, near),
                    new Vector3(left, floorNearLeftY, near)));

                int test;

                //South wall
                test = mapY - 1;
                if (test < 0 || map[mapX, test] == csMapbuilder.EmptyCell)
                {
                    boundaryCubeCenterPoints.Add(new Vector3(left + halfUnitX, centerY, near - halfUnitZ));
                }

                //North wall
                test = mapY + 1;
                if (test >= mapHeight || map[mapX, test] == csMapbuilder.EmptyCell)
                {
                    boundaryCubeCenterPoints.Add(new Vector3(left + halfUnitX, centerY, far + halfUnitZ));
                }

                //West wall
                test = mapX - 1;
                if (test < 0 || map[test, mapY] == csMapbuilder.EmptyCell)
                {
                    boundaryCubeCenterPoints.Add(new Vector3(left - halfUnitX, centerY, near + halfUnitZ));
                }

                //East wall
                test = mapX + 1;
                if (test >= mapWidth || map[test, mapY] == csMapbuilder.EmptyCell)
                {
                    boundaryCubeCenterPoints.Add(new Vector3(right + halfUnitX, centerY, near + halfUnitZ));
                }
            }
        }

        private int GetNorthSquare(int x, int y, int[,] map, int height)
        {
            y += 1;
            if (y >= height)
            {
                return csMapbuilder.EmptyCell;
            }
            return map[x, y];
        }

        private int GetSouthSquare(int x, int y, int[,] map)
        {
            y -= 1;
            if (y < 0)
            {
                return csMapbuilder.EmptyCell;
            }
            return map[x, y];
        }

        private int GetWestSquare(int x, int y, int[,] map)
        {
            x -= 1;
            if (x < 0)
            {
                return csMapbuilder.EmptyCell;
            }
            return map[x, y];
        }

        private int GetEastSquare(int x, int y, int[,] map, int width)
        {
            x += 1;
            if (x >= width)
            {
                return csMapbuilder.EmptyCell;
            }
            return map[x, y];
        }

        private int GetNorthEastSquare(int x, int y, int[,] map, int width, int height)
        {
            x += 1;
            y += 1;
            if (x >= width)
            {
                return csMapbuilder.EmptyCell;
            }
            if(y >= height)
            {
                return csMapbuilder.EmptyCell;
            }
            return map[x, y];
        }

        private int GetSouthEastSquare(int x, int y, int[,] map, int width)
        {
            x += 1;
            y -= 1;
            if (x >= width)
            {
                return csMapbuilder.EmptyCell;
            }
            if (y < 0)
            {
                return csMapbuilder.EmptyCell;
            }
            return map[x, y];
        }

        private int GetNorthWestSquare(int x, int y, int[,] map, int height)
        {
            x -= 1;
            y += 1;
            if (x < 0)
            {
                return csMapbuilder.EmptyCell;
            }
            if (y >= height)
            {
                return csMapbuilder.EmptyCell;
            }
            return map[x, y];
        }

        private int GetSouthWestSquare(int x, int y, int[,] map)
        {
            x -= 1;
            y -= 1;
            if (x < 0)
            {
                return csMapbuilder.EmptyCell;
            }
            if (y < 0)
            {
                return csMapbuilder.EmptyCell;
            }
            return map[x, y];
        }

        private void RenderEmptySquare(MapMeshTempSquareInfo[,] tempSquareInfo, int mapY, int mapX, int mapWidth, int mapHeight)
        {
            var left = mapX * MapUnitX;
            var right = left + MapUnitX;
            var far = mapY * MapUnitZ;
            var near = far - MapUnitZ;
            var tempSquare = tempSquareInfo[mapX + 1, mapY + 1];

            //
            // LF ----- RF
            //  |       |
            //  |       |
            // LN ----- RN
            //
            Vector3 leftFar = new Vector3(left, tempSquare.LeftFarY, far);
            Vector3 rightFar = new Vector3(right, tempSquare.RightFarY, far);
            Vector3 rightNear = new Vector3(right, tempSquare.RightNearY, near);
            Vector3 leftNear = new Vector3(left, tempSquare.LeftNearY, near);

            //Fix center points so they can be used later
            squareInfo[mapX + 1, mapY + 1].Center = (rightNear - leftFar) / 2 + leftFar;

            var north = tempSquareInfo[mapX + 1, mapY + 1 + 1];
            var west = tempSquareInfo[mapX - 1 + 1, mapY + 1];
            var northWest = tempSquareInfo[mapX - 1 + 1, mapY + 1 + 1];
            var east = tempSquareInfo[mapX + 1 + 1, mapY + 1];
            var south = tempSquareInfo[mapX + 1, mapY - 1 + 1];
            var southEast = tempSquareInfo[mapX + 1 + 1, mapY - 1 + 1];

            Vector3 leftFarNormal;
            {
                //         Far
                //    \|   \|   \|   \| 
                //...--UL---U----+---+--...      
                //     |\   |\   |\   |\          Y
                //     | \  | \  | \  | \         ^
                //   \ |  \ |  \ |  \ |           |
                //    \| NW\| N \|   \|           |
                //...--L----P----R----+--...      +-----> X
                //     |\ W |\ T |\   |\         
                //     | \  | \  | \  | \       
                //   \ |  \ |  \ |  \ |        
                //    \|   \|   \|   \| 
                //...--+----D----DR---+--...
                //     |\   |\   |\   |\
                //         Near

                var yBasis = tempSquare.LeftFarY;

                leftFarNormal = ComputeNormal(
                    west.LeftFarY - yBasis, tempSquare.RightFarY - yBasis,
                    tempSquare.LeftNearY - yBasis, north.LeftFarY - yBasis,
                    northWest.LeftFarY - yBasis, tempSquare.RightNearY - yBasis,
                    MapUnitX, MapUnitZ);
            }

            Vector3 rightFarNormal;
            {
                //         Far
                //    \|   \|   \|   \| 
                //...--UL---U----+---+--...      
                //     |\   |\   |\   |\          Y
                //     | \  | \  | \  | \         ^
                //   \ |  \ |  \ |  \ |           |
                //    \| N \|   \|   \|           |
                //...--L----P----R----+--...      +-----> X
                //     |\ T |\ E |\   |\         
                //     | \  | \  | \  | \       
                //   \ |  \ |  \ |  \ |        
                //    \|   \|   \|   \| 
                //...--+----D----DR---+--...
                //     |\   |\   |\   |\
                //         Near

                var yBasis = tempSquare.RightFarY;

                rightFarNormal = ComputeNormal(
                    tempSquare.LeftFarY - yBasis, east.RightFarY - yBasis,
                    tempSquare.RightNearY - yBasis, north.RightFarY - yBasis,
                    north.LeftFarY - yBasis, east.RightNearY - yBasis,
                    MapUnitX, MapUnitZ);
            }

            Vector3 rightNearNormal;
            {
                //         Far
                //    \|   \|   \|   \| 
                //...--UL---U----+---+--...      
                //     |\   |\   |\   |\          Y
                //     | \  | \  | \  | \         ^
                //   \ |  \ |  \ |  \ |           |
                //    \| T \| E \|   \|           |
                //...--L----P----R----+--...      +-----> X
                //     |\ S |\ SE|\   |\         
                //     | \  | \  | \  | \       
                //   \ |  \ |  \ |  \ |        
                //    \|   \|   \|   \| 
                //...--+----D----DR---+--...
                //     |\   |\   |\   |\
                //         Near

                var yBasis = tempSquare.RightNearY;

                rightNearNormal = ComputeNormal(
                    tempSquare.LeftNearY - yBasis, east.RightNearY - yBasis,
                    south.RightNearY - yBasis, tempSquare.RightFarY - yBasis,
                    tempSquare.LeftFarY - yBasis, southEast.RightNearY - yBasis,
                    MapUnitX, MapUnitZ);
            }

            Vector3 leftNearNormal;
            {
                //         Far
                //    \|   \|   \|   \| 
                //...--UL---U----+---+--...      
                //     |\   |\   |\   |\          Y
                //     | \  | \  | \  | \         ^
                //   \ |  \ |  \ |  \ |           |
                //    \| W \| T \|   \|           |
                //...--L----P----R----+--...      +-----> X
                //     |\   |\ S |\   |\         
                //     | \  | \  | \  | \       
                //   \ |  \ |  \ |  \ |        
                //    \|   \|   \|   \| 
                //...--+----D----DR---+--...
                //     |\   |\   |\   |\
                //         Near

                var yBasis = tempSquare.LeftNearY;

                leftNearNormal = ComputeNormal(
                    west.LeftNearY - yBasis, tempSquare.RightNearY - yBasis,
                    south.LeftNearY - yBasis, tempSquare.LeftFarY - yBasis,
                    west.LeftFarY - yBasis, south.RightNearY - yBasis,
                    MapUnitX, MapUnitZ);
            }

            //Both should work, this is better
            //var u = new Vector3(MapUnitX, tempSquare.RightFarY - tempSquare.LeftFarY, 0);
            //var v = new Vector3(MapUnitX, tempSquare.RightNearY - tempSquare.LeftFarY, -MapUnitZ);

            ////var u = rightFar - leftFar;
            ////var v = rightNear - leftFar;

            //var cross = u.cross(v).normalized();

            GetUvs(mapX, mapY, out var topLeft, out var bottomRight, out var globalTopLeft, out var globalBottomRight, mapWidth, mapHeight);

            floorMesh.AddQuad(
                leftFar,
                rightFar,
                rightNear,
                leftNear,
                leftFarNormal,
                rightFarNormal,
                rightNearNormal,
                leftNearNormal,
                //cross,
                //cross,
                //cross,
                //cross,
                topLeft,
                bottomRight,
                globalTopLeft,
                globalBottomRight,
                wallTextureIndex);

            collisionMeshPositions.Add(new MapMeshCollisionPosition(
                leftFar,
                rightFar,
                rightNear,
                leftNear));
        }

        private void GetUvs(int mapX, int mapY, out Vector2 leftTop, out Vector2 rightBottom, out Vector2 globalLeftTop, out Vector2 globalRightBottom, float mapWidth, float mapHeight)
        {
            //TODO: Can this be kept between 0 and 1? For now the uvs will be large numbers, this is ok with our sampler
            leftTop = new Vector2(mapX * uvXStride, mapY * uvYStride);
            rightBottom = new Vector2((mapX + 1) * uvXStride, (mapY + 1) * uvYStride);

            globalLeftTop = new Vector2(mapX / mapWidth, (mapY + 1) / mapHeight);
            globalRightBottom = new Vector2((mapX + 1) / mapWidth, (mapY) / mapHeight);
        }

        private Vector3 ComputeNormal(float zLeft, float zRight, float zDown, float zUp, float zUpleft, float zDownright, float ax, float ay)
        {
            Vector3 accumulatedNormal = new Vector3(0, 0, 0);
            var pl = new Vector3(-ax, zLeft, 0);
            var pul = new Vector3(-ax, zUpleft, ay);
            var pu = new Vector3(0, zUp, ay);
            var pr = new Vector3(ax, zRight, 0);
            var pdr = new Vector3(ax, zDownright, -ay);
            var pd = new Vector3(0, zDown, -ay);

            //Triangle 1
            {
                var cross = pl.cross(pul);
                accumulatedNormal += cross;
            }

            //Triangle 2
            {
                var cross = pul.cross(pu);
                accumulatedNormal += cross;
            }

            //Triangle 3
            {
                var cross = pu.cross(pr);
                accumulatedNormal += cross;
            }

            //Triangle 4
            {
                var cross = pr.cross(pdr);
                accumulatedNormal += cross;
            }

            //Triangle 5
            {
                var cross = pdr.cross(pd);
                accumulatedNormal += cross;
            }

            //Triangle 6
            {
                var cross = pd.cross(pl);
                accumulatedNormal += cross;
            }

            return accumulatedNormal.normalized();
            /*
             * Based on this, but our mesh is different
             * https://stackoverflow.com/questions/6656358/calculating-normals-in-a-triangle-mesh/6661242#6661242
                    \|   \|   \|   \| 
                ...--UL---U----+---+--...      
                     |\ 2 |\   |\   |\          Y
                     | \  | \  | \  | \         ^
                   \ |  \ |  \ |  \ |           |
                    \| 1 \| 3 \|   \|           |
                ...--L----P----R----+--...      +-----> X
                     |\ 6 |\ 4 |\   |\         
                     | \  | \  | \  | \       
                   \ |  \ |  \ |  \ |        
                    \|   \| 5 \|   \| 
                ...--+----D----DR---+--...
                     |\   |\   |\   |\
             */
        }
    }
}
