using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGeneratorScript
{
    GridSpace[][][] grid;
    bool[][][] isCounted;

    public RoomGeneratorScript(GridSpace[][][] grid)
    {
        isCounted = new bool[grid.Length][][];
        for(int y = 0; y < grid.Length; y++)
        {
            isCounted[y] = new bool[grid[y].Length][];
            for(int x = 0; x < grid[y].Length; x++)
            {
                isCounted[y][x] = new bool[grid[y][x].Length];
                for(int z = 0; z < grid[y][x].Length; z++)
                {
                    isCounted[y][x][z] = false;
                }
            }
        }
        this.grid = grid;
    }
    public List<Room> findRooms()
    {
        List<Room> roomList = new List<Room>();
        GridSpace currentPoint = grid[0][0][0];
        int roomCount = 0;
        for (int y = 0; y < grid.Length; y++)
        {
            for (int x = 0; x < grid[y].Length; x++)
            {
                for (int z = 0; z < grid[y][x].Length; z++)
                {
                    if (isValidPosition(grid[y][x][z].position)
                        && !isWall(grid[y][x][z])
                        && !isCounted[y][x][z])
                    {
                        HashSet<GridSpace> gridSpaces = new HashSet<GridSpace>();
                        Stack<GridSpace> stack = new Stack<GridSpace>();
                        stack.Push(grid[y][x][z]);
                        while(stack.Count > 0)
                        {
                            currentPoint = stack.Pop();
                            gridSpaces.Add(currentPoint);
                            countGridSpace(currentPoint);
                            foreach (GridSpace grid in findNextGrid(currentPoint))
                            {
                                stack.Push(grid);
                            }
                        }
                        Room room = new Room(gridSpaces);
                        roomList.Add(room);
                        roomCount++;
                    }
                }
            }
        }

        return roomList;
    }
    private void countGridSpace(GridSpace gridSpace)
    {
        if (!isCounted[gridSpace.position.y][gridSpace.position.x][gridSpace.position.z])
        {
            isCounted[gridSpace.position.y][gridSpace.position.x][gridSpace.position.z] = true;
        }
    }
    private List<GridSpace> findNextGrid(GridSpace currentLocation)
    {
        Coordinate[] adjacentCoordinates = Coordinate.getAdjacent(currentLocation.position, false);
        List<GridSpace> validNeighbors = new List<GridSpace>();
        for (int ii = 0; ii < 4; ii++)
        {
            Coordinate temp = adjacentCoordinates[ii];
            if( isValidPosition(temp)
                && grid[temp.y][temp.x][temp.z].block.GetComponent<BlockScript>().walkable[ii]
                && !isCounted[temp.y][temp.x][temp.z])
            {
                validNeighbors.Add(grid[temp.y][temp.x][temp.z]);
                countGridSpace(grid[temp.y][temp.x][temp.z]);
            }
        }
        return validNeighbors;
    }
    public bool isValidPosition(Coordinate position)
    {
        if (position.y < grid.Length && position.y >= 0)
        {
            if (position.x < grid[0].Length && position.x >= 0)
            {
                if (position.z < grid[0][0].Length && position.z >= 0)
                {
                    return true;
                }
            }
        }
        return false;
    }
    public bool isWall(GridSpace grid)
    {
        bool isWall = false;
        for(int dir = 0; dir < 4; dir++)
        {
            if (!grid.block.GetComponent<BlockScript>().walkable[dir])
            {
                isWall = true;
            }
        }
        return isWall;
    }
}
public class Room
{
    (int, int) origin;
    (int, int) size;
    int y;
    GridSpace[][] area;

    public Room(HashSet<GridSpace> gridSpaces)
    {
        origin = findOrigin(gridSpaces);
        size = findSize(gridSpaces);
        area = new GridSpace[size.Item1][];
        for(int x = 0; x < size.Item1; x++)
        {
            area[x] = new GridSpace[size.Item2];
        }
        populateRoomGrid(gridSpaces);
    }
    private (int,int) findOrigin(HashSet<GridSpace> gridSpaces)
    {
        (int, int) origin = (int.MaxValue, int.MaxValue);
        foreach(GridSpace gridSpace in gridSpaces)
        {
            if(gridSpace.position.x < origin.Item1)
            {
                origin.Item1 = gridSpace.position.x;
            }
            if (gridSpace.position.z < origin.Item2)
            {
                origin.Item2 = gridSpace.position.z;
            }
        }
        return origin;
    }
    private (int, int) findSize(HashSet<GridSpace> gridSpaces)
    {
        (int, int) size = (0, 0);
        foreach (GridSpace gridSpace in gridSpaces)
        {
            if (gridSpace.position.x - origin.Item1 > size.Item1)
            {
                size.Item1 = gridSpace.position.x - origin.Item1;
            }
            if (gridSpace.position.z - origin.Item2 > size.Item2)
            {
                size.Item2 = gridSpace.position.z - origin.Item2;
            }
        }
        size.Item1++;
        size.Item2++;
        return size;
    }
    private void populateRoomGrid(HashSet<GridSpace> gridSpaces)
    {
        foreach(GridSpace gridSpace in gridSpaces)
        {
            int xOffset = gridSpace.position.x - origin.Item1;
            int zOffset = gridSpace.position.z - origin.Item2;
            area[xOffset][zOffset] = gridSpace;
        }
    }
    public string toString()
    {
        return "room origin: " + origin + " size: " + size;
    }
}
