using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// IslandMaze class - generates simple islands and mazes.
/// 
/// For more info on it's use see http://www.evilscience.co.uk/?p=53
/// 
/// Map generation is controlled by the following variables:
/// 
/// p(int) - close cell probability.Between 0 and 100.
/// 
/// h (bool) - cell operation specifier.
/// i (int) - counter.
/// n (int) - number of cell's neighbours.
/// 
/// c (int) - examined cell's closed neighbours. Between 0 and 8.
/// 
/// </summary>
/// 

public class IslandInfo
{
    public IslandInfo(int id, int x, int y)
    {
        this.Id = id;
        this.Northmost = new IntVector2(x, y);
        this.Southmost = new IntVector2(x, y);
        this.Eastmost = new IntVector2(x, y);
        this.Westmost = new IntVector2(x, y);
    }

    public int Id;
    public int Size;
    public IntVector2 Northmost;
    public IntVector2 Southmost;
    public IntVector2 Eastmost;
    public IntVector2 Westmost;
    public List<IntVector2> islandPoints = new List<IntVector2>(30);
}

public class csIslandMaze
{
    public const int EmptyCell = 0;
    public const int RoomCell = 1;

    private Random r;

    public int Neighbours { get; set; }
    public int Iterations { get; set; }
    public int MapX { get; set; }
    public int MapY { get; set; }
    public int CloseCellProb { get; set; }
    public bool ProbExceeded { get; set; }

    public int[,] Map;

    public int NumIslands { get; private set; }

    public List<int> IslandSizeOrder { get; private set; }

    public List<IslandInfo> IslandInfo { get; private set; }

    public csIslandMaze(Random random)
    {
        this.r = random;
        Neighbours = 4;
        Iterations = 50000;
        ProbExceeded = true;
        MapX = 99;
        MapY = 99;
        CloseCellProb = 45;

    }

    /// <summary>
    /// Build a Map
    /// </summary>
    /// <param name="closeCellProb">Probability of closing a cell</param>
    /// <param name="neighbours">The number of cells required to trigger</param>
    /// <param name="iterations">Number of iterations</param>
    /// <param name="Map">Map array to opearate on</param>
    /// <param name="reset">Clear the Map before operation</param>
    /// <param name="probExceeded">probability exceeded</param>
    /// <param name="invert"></param>
    /// <returns></returns>
    public void go()
    {

        Map = new int[MapX, MapY];


        //go through each cell and use the specified probability to determine if it's open
        for (int x = 0; x < Map.GetLength(0); x++)
        {
            for (int y = 0; y < Map.GetLength(1); y++)
            {
                if (r.Next(0, 100) < CloseCellProb)
                {
                    Map[x, y] = 1;
                }
            }
        }

        //pick some cells at random
        for (int x = 0; x <= Iterations; x++)
        {
            int rX = r.Next(0, Map.GetLength(0));
            int rY = r.Next(0, Map.GetLength(1));

            if (ProbExceeded == true)
            {
                if (examineNeighbours(rX, rY) > Neighbours)
                {
                    Map[rX, rY] = 1;
                }
                else
                {
                    Map[rX, rY] = 0;
                }
            }
            else
            {
                if (examineNeighbours(rX, rY) > Neighbours)
                {
                    Map[rX, rY] = 0;
                }
                else
                {
                    Map[rX, rY] = 1;
                }
            }


        }
    }

    public void makeEdgesEmpty()
    {
        var bottom = MapY - 1;
        for(int x = 0; x < MapX; ++x)
        {
            Map[x, 0] = EmptyCell;
            Map[x, bottom] = EmptyCell;
        }

        var right = MapX - 1;
        for (int y = 0; y < MapY; ++y)
        {
            Map[0, y] = EmptyCell;
            Map[right, y] = EmptyCell;
        }
    }

    public void findIslands()
    {
        var islandFinder = new IslandFinder(MapX, MapY);
        Map = islandFinder.findIslands(Map);
        NumIslands = islandFinder.NumIslands;
        IslandInfo = islandFinder.IslandInfo;
        IslandSizeOrder = islandFinder.IslandInfo.OrderByDescending(i => i.Size).Select(i => i.Id - 1).ToList();
    }

    /// <summary>
    /// Count all the closed cells around the specified cell and return that number
    /// </summary>
    /// <param name="xVal">cell X value</param>
    /// <param name="yVal">cell Y value</param>
    /// <returns>Number of surrounding cells</returns>
    private int examineNeighbours(int xVal, int yVal)
    {
        int count = 0;

        for (int x = -1; x < 2; x++)
        {
            for (int y = -1; y < 2; y++)
            {
                if (checkCell(xVal + x, yVal + y) == true)
                    count += 1;
            }
        }

        return count;
    }

    public void RemoveExtraIslands(int count)
    {
        var toRemove = new List<IslandInfo>();

        for (int i = NumIslands - 1; i >= count; --i) //Smallest islands first
        {
            var index = IslandSizeOrder[i];
            var island = IslandInfo[index];
            foreach(var square in island.islandPoints)
            {
                Map[square.x, square.y] = csIslandMaze.EmptyCell;
            }
            toRemove.Add(island);
        }

        IslandInfo.RemoveAll(i => toRemove.Contains(i));

        for (int i = 0; i < count; ++i) //Just go in current order, it will sort again below
        {
            var island = IslandInfo[i];
            var id = i + 1;
            foreach (var square in island.islandPoints)
            {
                Map[square.x, square.y] = id;
            }
            island.Id = id;
        }

        NumIslands = count;
        IslandSizeOrder = IslandInfo.OrderByDescending(i => i.Size).Select(i => i.Id - 1).ToList();
    }

    /// <summary>
    /// Check the examined cell is legal and closed
    /// </summary>
    /// <param name="x">cell X value</param>
    /// <param name="y">cell Y value</param>
    /// <returns>Cell state - true if closed, false if open or illegal</returns>
    private Boolean checkCell(int x, int y)
    {
        if (x >= 0 & x < Map.GetLength(0) &
            y >= 0 & y < Map.GetLength(1))
        {
            if (Map[x, y] > 0)
                return true;
            else
                return false;
        }
        else
        {
            return false;
        }
    }
}



/**
 * Originally based on code with the following license.
 * 
 * https://github.com/AndyStobirski/RogueLike
 * 

This is free and unencumbered software released into the public domain.

Anyone is free to copy, modify, publish, use, compile, sell, or
distribute this software, either in source code form or as a compiled
binary, for any purpose, commercial or non-commercial, and by any
means.

In jurisdictions that recognize copyright laws, the author or authors
of this software dedicate any and all copyright interest in the
software to the public domain. We make this dedication for the benefit
of the public at large and to the detriment of our heirs and
successors. We intend this dedication to be an overt act of
relinquishment in perpetuity of all present and future rights to this
software under copyright law.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.

For more information, please refer to <http://unlicense.org> 
*/

//-------------------------------------------------------------------

// This code is contributed
// by shiv_bhakt.
//https://www.geeksforgeeks.org/find-number-of-islands/
class IslandFinder
{
    // No of rows
    // and columns
    int ROW = 5, COL = 5;

    public int NumIslands { get; private set; }
    public List<IslandInfo> IslandInfo { get; private set; }

    public IslandFinder(int row, int col)
    {
        this.ROW = row; ;
        this.COL = col;
    }

    // A function to check if
    // a given cell (row, col)
    // can be included in DFS
    bool isSafe(int[,] M, int row,
                       int col, bool[,] visited)
    {
        // row number is in range,
        // column number is in range
        // and value is 1 and not
        // yet visited
        return (row >= 0) && (row < ROW) && (col >= 0) && (col < COL) && (M[row, col] == 1 && !visited[row, col]);
    }

    // A utility function to do
    // DFS for a 2D boolean matrix.
    // It only considers the 8
    // neighbors as adjacent vertices
    void DFS(int[,] M, int row,
                    int col, bool[,] visited, int id, int[,] marked)
    {
        // These arrays are used to
        // get row and column numbers
        // of 4 neighbors of a given cell
        // this needs to be walkable, so only
        // cardinal directions work 

        int[] rowNbr = new int[] { -1,  0, 0, 1 };
        int[] colNbr = new int[] {  0, -1, 1, 0 };

        // Mark this cell
        // as visited
        visited[row, col] = true;
        marked[row, col] = id;
        var index = id - 1;
        var islandInfo = IslandInfo[index];
        ++islandInfo.Size;
        islandInfo.islandPoints.Add(new IntVector2(row, col));

        if (col > islandInfo.Northmost.y)
        {
            islandInfo.Northmost = new IntVector2(row, col);
        }

        if (col < islandInfo.Southmost.y)
        {
            islandInfo.Southmost = new IntVector2(row, col);
        }

        if (row > islandInfo.Eastmost.x)
        {
            islandInfo.Eastmost = new IntVector2(row, col);
        }

        if (row < islandInfo.Westmost.x)
        {
            islandInfo.Westmost = new IntVector2(row, col);
        }

        // Recur for all
        // connected neighbours
        for (int k = 0; k < 4; ++k)
        {
            if (isSafe(M, row + rowNbr[k], col + colNbr[k], visited))
            {
                DFS(M, row + rowNbr[k],
                    col + colNbr[k], visited,
                    id, marked);
            }
        }
    }

    // The main function that
    // returns count of islands
    // in a given boolean 2D matrix
    public int[,] findIslands(int[,] M)
    {
        // Make a bool array to
        // mark visited cells.
        // Initially all cells
        // are unvisited
        bool[,] visited = new bool[ROW, COL];
        int[,] marked = new int[ROW, COL];
        IslandInfo = new List<IslandInfo>(50);

        // Initialize count as 0 and
        // traverse through the all
        // cells of given matrix
        int id = 1;
        for (int i = 0; i < ROW; ++i)
        {
            for (int j = 0; j < COL; ++j)
            {
                if (M[i, j] == 1 && !visited[i, j])
                {
                    // If a cell with value 1 is not
                    // visited yet, then new island
                    // found, Visit all cells in this
                    // island and increment island count
                    IslandInfo.Add(new IslandInfo(id, i, j));
                    DFS(M, i, j, visited, id, marked);
                    ++id;
                }
            }
        }

        NumIslands = id - 1;

        return marked;
    }
}