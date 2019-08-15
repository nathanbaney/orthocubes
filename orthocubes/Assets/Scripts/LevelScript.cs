using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LevelScript : MonoBehaviour
{
    public GameObject block;
    public int LEVEL_WIDTH;
    public int LEVEL_HEIGHT;
    public int LEVEL_LENGTH;
    public GridSpace[][][] grid;
    public bool[] levelVis;
    private int blockSize = 4;
    public Material mat1;
    public LevelData levelData;
    public bool levelIsLoaded = false;
    public bool levelIsLoading = false;

    public Entity player;

    public Stack<Move> moveStack = new Stack<Move>();

    public ulong wallPerm = 0x000F000F000F000F;

    // Start is called before the first frame update
    void Start()
    {
        levelData = new LevelData(LEVEL_WIDTH*LEVEL_LENGTH);
        /*int[] paletteValues ={0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};*/
        /*int[] paletteValues =   { 0, 0, 0, 0, 0, 0, 0, 0,
                                    0, 0, 0, 0, 0, 0, 0, 0,
                                    0, 0, 0, 0, 0, 0, 0, 0,
                                    0, 0, 0, 0, 0, 0, 0, 0,
                                    0, 0, 0, 0, 0, 0, 0, 0,
                                    0, 0, 0, 0, 0, 0, 0, 0,
                                    0, 0, 0, 0, 0, 0, 0, 0,
                                    0, 0, 0, 0, 0, 0, 0, 0,};*/

        /*int[] paletteValues =     { 0, 2, 0, 0, 0, 2, 0, 0,
                                    0, 2, 0, 0, 0, 2, 0, 0,
                                    0, 2, 0, 0, 0, 2, 0, 0,
                                    1,11, 1, 1, 1,11, 1, 1,
                                    0, 2, 0, 0, 0, 2, 0, 0,
                                    0, 2, 0, 0, 0, 2, 0, 0,
                                    1,11, 1, 1, 1,11, 1, 1,
                                    0, 2, 0, 0, 0, 2, 0, 0,};*/
        /*int[] paletteValues =  {0, 2, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0,
                                0, 2, 0, 0, 0, 0, 0, 0,11,11, 1, 1,11, 1,11, 1,
                                0, 2, 0, 0, 0,11, 1, 1,11,11, 0, 0, 2, 0, 2, 0,
                                0, 2, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 2, 0, 2, 0,
                                0, 2, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0,11, 0, 2, 0,
                               11,11, 1, 1, 1,11, 0, 0, 0, 2, 0, 0, 0, 0,11, 1,
                                2, 0, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0,
                                2, 0, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0,
                                2, 0, 0, 0, 0, 2, 0, 0, 0,11, 1, 1,11, 0, 0, 0,
                                2, 0, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0,11, 1,11, 0,
                               11, 1, 1, 1, 1,11, 1, 1,11,11, 0, 0, 2, 0, 2, 0,
                                2, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 2, 0, 2, 0,
                                2, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 2, 0, 2, 0,
                               11, 0, 0, 0,11, 0, 0, 0,11, 1, 1, 1,11, 1,11, 1,
                                2, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0,
                               11, 1, 1, 1,11, 1, 1, 1,11, 0, 0, 0, 2, 0, 0, 0,};*/
        int[] paletteValues = {11, 1, 1, 1,11, 0, 2, 0, 0, 2, 0, 0, 0, 0, 0, 2,
                                2, 0, 0, 0, 2, 0,11, 1, 1,11, 0, 0, 0, 0, 0, 2,
                                2, 0, 0, 0, 2, 0, 0, 0, 0,11, 1, 1, 1,11, 0, 2,
                                2, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 2,
                               11, 1, 1, 1,11, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 2,
                                2, 0, 0, 0, 0, 0, 0,11, 1, 1,11, 1, 1,11, 0, 2,
                                2, 0, 0, 0, 0, 0, 0, 2, 0, 0, 2, 0, 0, 0, 0, 2,
                               11, 1, 1,11, 0, 0, 0, 2, 0, 0, 2, 0, 0, 0, 0, 2,
                                0, 0, 0, 2, 0, 0, 0,11, 0, 0,11, 1, 1, 1, 1,11,
                                0, 0, 0,11, 1,11, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0,
                                0, 0, 0, 0, 0, 2, 0,11, 1, 1, 1, 1, 1,11, 0,11,
                                0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 2, 0, 2,
                                0, 0, 0, 0, 0,11,11, 0, 0, 0, 0, 0, 0, 2, 0, 2,
                                0, 0, 0, 0, 0, 0, 2, 0, 0,11, 1, 1, 1,11, 0, 2,
                                0, 0, 0, 0, 0, 0, 2, 0, 0, 2, 0, 0, 0, 0, 0, 2,
                                1, 1, 1, 1,11, 0, 2, 0, 0, 2, 0, 0, 0, 0, 0, 2};
        PaletteMakerScript mkr = new PaletteMakerScript();
        mkr.generateJSON(paletteValues,16,16, true,"bigPalette8");
        WFCScript wfc = new WFCScript(LEVEL_WIDTH, LEVEL_LENGTH, "D:/dev/orthocubes/gamedata/bigPalette8.json");
        wfc.generateWithResets(1);
        buildLevel();
        RoomGeneratorScript roomGen = new RoomGeneratorScript(grid);
        List<Room> roomList = roomGen.findRooms();
        foreach(Room room in roomList)
        {
            //Debug.Log(room.toString());
        }
        

        player = new Entity(new Coordinate(0, 0, 0), GameObject.Find("Player"));
        //debugBuildWall(4);
    }

    // Update is called once per frame
    void Update()
    {
        if (levelIsLoaded)
        {
            handleInput();
            handleMoves();
            checkVisibleFloors();
            //increment turn timer counter thing
            //decide ai moves
            //resolve all moves
            //wait for input
        }
    }

    //initialization functions
    void buildBlocks()
    {
        grid = new GridSpace[LEVEL_HEIGHT][][];
        levelVis = new bool[LEVEL_HEIGHT];
        GameObject tempBlock;
        for (int y = 0; y < LEVEL_HEIGHT; y++)
        {
            grid[y] = new GridSpace[LEVEL_WIDTH][];
            levelVis[y] = true;
            for (int x = 0; x < LEVEL_WIDTH; x++)
            {
                grid[y][x] = new GridSpace[LEVEL_WIDTH];
                for (int z = 0; z < LEVEL_LENGTH; z++)
                {
                    tempBlock = (GameObject)Instantiate(block, new Vector3(x * blockSize, y * blockSize, z * blockSize), transform.rotation, transform);
                    grid[y][x][z] = new GridSpace(x, y, z, tempBlock);
                    grid[y][x][z].block.name = "block" + (x + z * LEVEL_WIDTH + y * LEVEL_WIDTH * LEVEL_LENGTH);
                    grid[y][x][z].block.GetComponent<BlockScript>().instantiateVoxels();
                    //print(x + z * LEVEL_WIDTH + y * LEVEL_WIDTH * LEVEL_LENGTH);
                    grid[y][x][z].block.GetComponent<BlockScript>().deserialize(levelData.blockData[x + z * LEVEL_WIDTH + y * LEVEL_WIDTH * LEVEL_LENGTH]);
                }
            }
        }
    }
    void buildBlockPerms()
    {
        for (int y = 0; y < LEVEL_HEIGHT; y++)
        {
            for (int x = 0; x < LEVEL_WIDTH; x++)
            {
                for (int z = 0; z < LEVEL_LENGTH; z++)
                {
                    grid[y][x][z].block.GetComponent<BlockScript>().buildBlockPerm();
                }
            }
        }
    }
    void deserialize()
    {
        string jsonString = File.ReadAllText("D:/dev/orthocubes/gamedata/floorData.json");
        print(jsonString);
        levelData = JsonUtility.FromJson<LevelData>(jsonString);
    }
    void buildMaterialOverrides()
    {
        for (int y = 0; y < LEVEL_HEIGHT; y++)
        {
            for (int x = 0; x < LEVEL_WIDTH; x++)
            {
                for (int z = 0; z < LEVEL_LENGTH; z++)
                {
                    grid[y][x][z].block.GetComponent<BlockScript>().buildMaterialOverrides();
                }
            }
        }
    }
    void buildBlankFloor()
    {
        //make leveldata and json string representation of a levelwidth x levellength x levelheight flat level
        levelData = new LevelData(LEVEL_WIDTH * LEVEL_LENGTH * LEVEL_HEIGHT);
        for (int y = 0; y < LEVEL_HEIGHT; y++)
        {
            for(int z = 0; z < LEVEL_LENGTH; z++)
            {
                for (int x = 0; x < LEVEL_WIDTH; x++)
                {
                    levelData.blockData[x + z * LEVEL_WIDTH + y * LEVEL_LENGTH * LEVEL_WIDTH] = new BlockData();
                }
            }
        }
        StreamWriter writer = new StreamWriter("Assets/Resources/FloorJSON/floorData.json");
        writer.Write(JsonUtility.ToJson(levelData, true));
        writer.Close();
    }
    void buildLevel()
    {
        levelIsLoaded = true;
        deserialize();
        buildBlocks();
        buildBlockPerms();
        buildMaterialOverrides();
    }
    //generation functions
    void debugBuildWall(int xpos)
    {
        ulong rotatedPerm = 0;
        for(int ii = 0; ii < 10; ii++)
        {
            if (rotatedPerm == 0)
            {
                rotatedPerm =  BlockScript.getRotation(wallPerm, 1);
            }
            grid[0][xpos][ii].block.GetComponent<BlockScript>().combinePerm(rotatedPerm);
        }
    }
    


    //entity + visibility functions
    public void setFloorVisible(int floor, bool visible)
    {
        for (int x = 0; x < LEVEL_WIDTH; x++)
        {
            for (int z = 0; z < LEVEL_LENGTH; z++)
            {
                grid[floor][x][z].block.GetComponent<BlockScript>().setVisible(visible);
            }
        }
        levelVis[floor] = visible;
    }
    public void checkVisibleFloors()
    {
        for(int y = LEVEL_HEIGHT; y > player.position.y+1; y--)
        {
            if (levelVis[y-1] != false)
            {
                setFloorVisible(y-1, false);
            }
        }
        for(int y = player.position.y; y >= 0; y--)
        {
            if (levelVis[y] != true)
            {
                setFloorVisible(y, true);
            }
        }
    }
    public GridSpace getGridSpaceAt(Coordinate coord)
    {
        return grid[coord.y][coord.x][coord.z];
    }
    public void handleInput()
    {
        if (Input.GetKeyUp(KeyCode.Keypad8))
        {
            if (isValidPosition(Coordinate.getNorth(player.position)))
            {
                moveStack.Push(new Move(getGridSpaceAt(Coordinate.getNorth(player.position)), player));
            }
        }
        else if (Input.GetKeyUp(KeyCode.Keypad2))
        {
            if (isValidPosition(Coordinate.getSouth(player.position)))
            {
                moveStack.Push(new Move(getGridSpaceAt(Coordinate.getSouth(player.position)), player));
            }
        }
        else if (Input.GetKeyUp(KeyCode.Keypad6))
        {
            if (isValidPosition(Coordinate.getEast(player.position)))
            {
                moveStack.Push(new Move(getGridSpaceAt(Coordinate.getEast(player.position)), player));
            }
        }
        else if (Input.GetKeyUp(KeyCode.Keypad4))
        {
            if (isValidPosition(Coordinate.getWest(player.position)))
            {
                moveStack.Push(new Move(getGridSpaceAt(Coordinate.getWest(player.position)), player));
            }
        }
        else if (Input.GetKeyUp(KeyCode.KeypadMinus))
        {
            if (isValidPosition(Coordinate.getUp(player.position)))
            {
                moveStack.Push(new Move(getGridSpaceAt(Coordinate.getUp(player.position)), player));
            }
        }
        else if (Input.GetKeyUp(KeyCode.KeypadPlus))
        {
            if (isValidPosition(Coordinate.getDown(player.position)))
            {
                moveStack.Push(new Move(getGridSpaceAt(Coordinate.getDown(player.position)), player));
            }
        }
    }
    public void handleMoves()
    {
        while(moveStack.Count != 0)
        {
            moveStack.Pop().execute();
        }
    }
    public bool isValidPosition(Coordinate position)
    {
        if(position.y < LEVEL_HEIGHT && position.y >= 0)
        {
            if(position.z < LEVEL_LENGTH && position.z >= 0)
            {
                if(position.x < LEVEL_WIDTH && position.x >= 0)
                {
                    return true;
                }
            }
        }
        return false;
    }
}
public class LevelData
{
    public BlockData[] blockData;
    public LevelData(int size)
    {
        this.blockData = new BlockData[size];
    }
}
public class Coordinate
{
    public int x;
    public int y;
    public int z;

    public Coordinate(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    public static Coordinate getNorth(Coordinate coordinate)
    {
        return new Coordinate(coordinate.x + 1, coordinate.y, coordinate.z);
    }
    public static Coordinate getSouth(Coordinate coordinate)
    {
        return new Coordinate(coordinate.x - 1, coordinate.y, coordinate.z);
    }
    public static Coordinate getEast(Coordinate coordinate)
    {
        return new Coordinate(coordinate.x, coordinate.y, coordinate.z - 1);
    }
    public static Coordinate getWest(Coordinate coordinate)
    {
        return new Coordinate(coordinate.x, coordinate.y, coordinate.z + 1);
    }
    public static Coordinate getUp(Coordinate coordinate)
    {
        return new Coordinate(coordinate.x, coordinate.y+1, coordinate.z);
    }
    public static Coordinate getDown(Coordinate coordinate)
    {
        return new Coordinate(coordinate.x, coordinate.y-1, coordinate.z);
    }
    public static Coordinate[] getAdjacent(Coordinate start, bool includeVerticals)
    {
        Coordinate[] coordinates;
        if (includeVerticals)
        {
            coordinates = new Coordinate[6];
        }
        else
        {
            coordinates = new Coordinate[4];
        }
        coordinates[0] = getNorth(start);
        coordinates[1] = getEast(start);
        coordinates[2] = getSouth(start);
        coordinates[3] = getWest(start);
        if (includeVerticals)
        {
            coordinates[4] = getUp(start);
            coordinates[5] = getDown(start);
        }
        return coordinates;
    }
    public static bool isNorthOf(Coordinate coordinate1, Coordinate coordinate2)
    {
        if(coordinate1.x  > coordinate2.x)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public static bool isSouthOf(Coordinate coordinate1, Coordinate coordinate2)
    {
        if (coordinate1.x < coordinate2.x)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public static bool isEastOf(Coordinate coordinate1, Coordinate coordinate2)
    {
        if (coordinate1.z > coordinate2.z)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public static bool isWestOf(Coordinate coordinate1, Coordinate coordinate2)
    {
        if (coordinate1.z < coordinate2.z)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public static bool isAbove(Coordinate coordinate1, Coordinate coordinate2)
    {
        if (coordinate1.y > coordinate2.y)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public static bool isBelow(Coordinate coordinate1, Coordinate coordinate2)
    {
        if (coordinate1.y < coordinate2.y)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public static int getDirectionOf(Coordinate coordinate1, Coordinate coordinate2)
    {
        if(isNorthOf(coordinate1, coordinate2))
        {
            return 0;
        }else if (isEastOf(coordinate1, coordinate2))
        {
            return 1;
        }
        else if (isSouthOf(coordinate1, coordinate2))
        {
            return 2;
        }
        else if (isWestOf(coordinate1, coordinate2))
        {
            return 3;
        }
        else if (isAbove(coordinate1, coordinate2))
        {
            return 4;
        }
        else if (isBelow(coordinate1, coordinate2))
        {
            return 5;
        }
        else
        {
            Debug.Log("coords are in the same position, cant get direction");
            return 0;
        }
    }
    public Vector3 getWorldCoordinate()
    {
        return new Vector3(x*BlockScript.blockSize+2, y * BlockScript.blockSize + 1, z * BlockScript.blockSize + 2);
    }
}
public class Entity
{
    public GameObject gameObject;
    public Coordinate position;

    public Entity(int x, int y, int z, GameObject gameObject)
    {
        this.position = new Coordinate(x, y, z);
        this.gameObject = gameObject;
    }
    public Entity(Coordinate coordinate, GameObject gameObject)
    {
        this.position = coordinate;
        this.gameObject = gameObject;
    }
    public void moveTo(GridSpace gridSpace)
    {
        int direction = Coordinate.getDirectionOf(this.position, gridSpace.position);
        if (gridSpace.block.GetComponent<BlockScript>().walkable[direction])
        {
            this.position = gridSpace.position;
            this.gameObject.transform.position = this.position.getWorldCoordinate();
        }
    }
}
public class GridSpace
{
    public GameObject block;
    public List<GameObject> entities;
    public Coordinate position;
    public int djikValue;

    public GridSpace(int x, int y, int z, GameObject obj)
    {
        this.block = obj;
        this.position = new Coordinate(x, y, z);
        this.djikValue = 1;
    }
    public GridSpace(Coordinate position, GameObject obj)
    {
        this.block = obj;
        this.position = position;
        this.djikValue = 1;
    }
}
public class Move
{
    public Entity entity;
    public GridSpace destination;

    public Move(GridSpace destination, Entity entity)
    {
        this.entity = entity;
        this.destination = destination;
    }
    public void execute()
    {
        entity.moveTo(destination);
    }
}
//A Palette is the datastructure that holds the processed information gathered from a sample image according to the WFC algorithm.
/*public class WFCGeneratorNoRotations
{
    public WFCGeneratorCellNoRotations[][] grid;
    public int xSize;
    public int ySize;
    private int uncollapsedCells;

    private HeapNR heap;
    private Stack<RemovalUpdateNR> removalStack;

    public WFCGeneratorNoRotations(int xSize, int ySize)
    {
        uncollapsedCells = xSize * ySize;
        this.xSize = xSize;
        this.ySize = ySize;
        grid = new WFCGeneratorCellNoRotations[xSize][];
        ((int,int), float)[] input = new ((int, int), float)[xSize * ySize];
        int index = 0;
        for (int x = 0; x < xSize; x++)
        {
            grid[x] = new WFCGeneratorCellNoRotations[ySize];
            for (int y = 0; y < ySize; y++)
            {
                grid[x][y] = new WFCGeneratorCellNoRotations((int)palette.numberOfTiles, Random.value / 100f, palette.frequencies);
                input[index++] = ((x, y), grid[x][y].getEntropy());
            }
        }
        heap = new HeapNR(input, xSize * ySize * 1000);
        removalStack = new Stack<RemovalUpdateNR>();
    }
   
    
    private (int, int)? chooseNextCell()
    {
        while (!heap.isEmpty())
        {
            (int, int) coord = heap.extractMin().Item1;
            if (!grid[coord.Item1][coord.Item2].isCollapsed)
            {
                return coord;
            }
        }
        Debug.Log("empty heap");
        return null;
    }
    private void collapseCellAt((int, int) coord)
    {
        int tileIndex = grid[coord.Item1][coord.Item2].chooseTileIndex(palette.frequencies);
        Debug.Log("collapsing cell# " + coord + " to tile# " + tileIndex);
        grid[coord.Item1][coord.Item2].isCollapsed = true;
        for (int ii = 0; ii < grid[coord.Item1][coord.Item2].possibilities.Length; ii++)
        {
            if (ii != tileIndex)
            {
                grid[coord.Item1][coord.Item2].possibilities[ii] = false;
                removalStack.Push(new RemovalUpdateNR(ii, coord));
            }
        }
    }
    private bool propagate()
    {
        while (removalStack.Count > 0)
        {
            RemovalUpdateNR update = removalStack.Pop();
            for (uint dir = 0; dir < 4; dir++) //each direction
            {
                (int, int)? neighborMaybe = getNeighbor(update.coordinate, (int)dir, false); //get the neighbor
                if (neighborMaybe != null)
                {
                    (int, int) neighbor = ((int, int))neighborMaybe;
                    for (int tileB = 0; tileB < palette.numberOfTiles; tileB++) //check all tiles
                    {
                            if (palette.compatible(palette.tiles[update.index][0], palette.tiles[tileB][0], dir)) //for each compatible tile in the specified direction from the removal update tile...
                            {
                                if (grid[neighbor.Item1][neighbor.Item2].enablerCount[tileB][oppositeDirection((int)dir)] == 1) //if we are about to set a count to zero...
                                {
                                    bool hasZero = false;
                                    for (int direction = 0; direction < 4; direction++)
                                    {
                                        if (grid[neighbor.Item1][neighbor.Item2].enablerCount[tileB][direction] == 0) //if theres already a zero, then the potential has already been removed, and we dont want to re-remove it
                                        {
                                            Debug.Log("tile already removed, skipping");
                                            hasZero = true;
                                        }
                                    }
                                    if (!hasZero) //if is hasnt already been removed, remove it 
                                    {
                                        //Debug.Log("removing tile " + tileB  + " from grid " + neighbor);
                                        grid[neighbor.Item1][neighbor.Item2].removeTile((uint)tileB, palette.frequencies);
                                        if (grid[neighbor.Item1][neighbor.Item2].totalPossibilities(palette.frequencies) == 0)
                                        {
                                            //hit a contradiction, need to restart algo
                                            Debug.Log("hit a contradiction at " + neighbor + ", need to restart algorithm");
                                            return false;
                                        }
                                        heap.insert((neighbor, grid[neighbor.Item1][neighbor.Item2].getEntropy()));
                                        removalStack.Push(new RemovalUpdateNR(tileB, neighbor));
                                    }
                                }
                                grid[neighbor.Item1][neighbor.Item2].enablerCount[tileB][oppositeDirection((int)dir)]--;
                                Debug.Log("setting ec of grid " + neighbor + " tile "+ tileB + " to " + grid[neighbor.Item1][neighbor.Item2].enablerCount[tileB][oppositeDirection((int)dir)]);
                            }
                        }
                    }
                }
            }
        return true;
    }
    private void reset()
    {
        ((int, int), float)[] input = new ((int, int), float)[xSize * ySize];
        int index = 0;
        for (int x = 0; x < xSize; x++)
        {
            grid[x] = new WFCGeneratorCellNoRotations[ySize];
            for (int y = 0; y < ySize; y++)
            {
                grid[x][y] = new WFCGeneratorCellNoRotations((int)palette.numberOfTiles, Random.value / 1000f, palette.frequencies);
                input[index++] = ((x, y), grid[x][y].getEntropy());
            }
        }
        heap = new HeapNR(input, xSize * ySize * 1000);
        removalStack = new Stack<RemovalUpdateNR>();
        initializeEnablerCounts();
    }

    public int[] run()
    {
        int cycle = 0;
        bool resetFlag = false;
        while (uncollapsedCells > 0)
        {
            Debug.Log("cycle " + cycle);
            (int, int) nextCell = ((int, int))chooseNextCell();
            collapseCellAt(nextCell);
            if (!propagate()) //if you hit a contradiction...
            {
                resetFlag = true;
                break;
            }
            uncollapsedCells--;
            cycle++;
            //printEnablerStrings();
        }
        if (resetFlag)
        {
            Debug.Log("resetting");
            reset();
            return run();
        }
        else
        {
            return decodeGrid();
        }
    }
    private int[] decodeGrid()
    {
        int[] decodedGrid = new int[xSize * ySize];
        for (int y = 0; y < ySize; y++)
        {
            for (int x = 0; x < xSize; x++)
            {
                decodedGrid[x + y * xSize] = grid[x][y].chosenTile;
            }
        }
        return decodedGrid;
    }
    private (int, int)? getNeighbor((int, int) start, int direction, bool wrapping)
    {
        int x, y;
        (int, int)? neighbor = start;
        switch (direction)
        {
            case 0: //up
                x = start.Item1;
                y = start.Item2 + 1;
                if (wrapping)
                {
                    neighbor = (x, y % ySize);
                }
                else
                {
                    if (y >= ySize) {
                        neighbor = null;
                    }
                    else
                    {
                        neighbor = (x, y);
                    }
                }
                break;
            case 1: //right
                x = start.Item1 + 1;
                y = start.Item2;
                if (wrapping)
                {
                    neighbor = (x % xSize, y);
                }
                else
                {
                    if (x >= xSize)
                    {
                        neighbor = null;
                    }
                    else
                    {
                        neighbor = (x, y);
                    }
                }
                break;
            case 2: //down
                x = start.Item1;
                y = start.Item2 - 1;
                if (y < 0)
                {
                    if (wrapping)
                    {
                        neighbor = (x, ySize + y);
                    }
                    else
                    {
                        neighbor = null;
                    }
                }
                else
                {
                    neighbor = (x, y);
                }
                break;
            case 3: //left
                x = start.Item1 - 1;
                y = start.Item2;
                if (x < 0)
                {
                    if (wrapping)
                    {
                        neighbor = (xSize + x, y);
                    }
                    else
                    {
                        neighbor = null;
                    }
                }
                else
                {
                    neighbor = (x, y);
                }
                break;
        }
        return neighbor;
    }
    private int oppositeDirection(int direction)
    {
        switch (direction)
        {
            case 0: //up
                return 2;
            case 1: //right
                return 3;
            case 2: //down
                return 0;
            case 3: //left
                return 1;
        }
        return 0;
    }
    private void printEnablerStrings()
    {
        for (int y = 0; y < ySize; y++)
        {
            for (int x = 0; x < xSize; x++)
            {
                Debug.Log("x/y: " + x + " " + y);
                Debug.Log(grid[x][y].getEnablerString());
            }
        }
    }
    private class HeapNR //yeah, c# really doesnt have a heap in their standard lib. im thrilled
    {
        ((int, int), float)[] heapContents; //the int pair is the index of the gridspace, the float is the entropy
        int heapSize;

        public HeapNR(((int, int), float)[] input, int maxSize)
        {
            heapContents = new ((int, int), float)[maxSize];
            heapSize = input.Length;
            buildHeap(input);
        }

        private int getParent(int index)
        {
            return index / 2;
        }
        private int getLeft(int index)
        {
            return 2 * index;
        }
        private int getRight(int index)
        {
            return 2 * index + 1;
        }
        private void heapify(int index)
        {
            int left = getLeft(index);
            int right = getRight(index);
            int smallest = index;
            if (left < heapSize && heapContents[left].Item2 < heapContents[index].Item2)
            {
                smallest = left;
            }
            if (right < heapSize && heapContents[right].Item2 < heapContents[index].Item2)
            {
                smallest = right;
            }
            if (smallest != index)
            {
                swap(index, smallest);
                heapify(smallest);
            }
        }
        private void swap(int index1, int index2)
        {
            ((int, int), float) temp = heapContents[index1];
            heapContents[index1] = heapContents[index2];
            heapContents[index2] = temp;
        }
        private void buildHeap(((int, int), float)[] input)
        {
            for (int ii = 0; ii < input.Length; ii++)
            {
                heapContents[ii] = input[ii];
            }
            for (int ii = heapSize / 2; ii >= 0; ii--)
            {
                heapify(ii);
            }
        }
        private void decreaseKey(int index, float newValue)
        {
            if (newValue > heapContents[index].Item2)
            {
                Debug.Log("heap error: tried to increase key in min heap");
                return;
            }
            else
            {
                int tempIndex = index;
                heapContents[index].Item2 = newValue;
                while (tempIndex > 0 && heapContents[getParent(tempIndex)].Item2 < heapContents[tempIndex].Item2)
                {
                    swap(tempIndex, getParent(tempIndex));
                    tempIndex = getParent(tempIndex);
                }
            }
        }
        public bool isEmpty()
        {
            return heapSize < 1;
        }
        public ((int, int), float) extractMin()
        {
            ((int, int), float) min = heapContents[0];
            heapContents[0] = heapContents[--heapSize];
            heapify(0);
            return min;
        }
        public void insert(((int, int), float) item)
        {
            int index = heapSize++;
            //Debug.Log(index);
            heapContents[index] = item;
            heapContents[index].Item2 = float.MaxValue;
            decreaseKey(index, item.Item2);
        }
    }
    private class RemovalUpdateNR
    {
        public int index;
        public (int, int) coordinate;

        public RemovalUpdateNR(int index, (int, int) coordinate)
        {
            this.index = index;
            this.coordinate = coordinate;
        }
        public string toString()
        {
            return "index: " + index + " coord: " + coordinate;
        }
    }

} //testing
public class WFCGeneratorCellNoRotations
{
    public bool isCollapsed;
    public bool[] possibilities;
    uint sumOfWeights;
    float sumOfWeightLogWeights;
    public float entropyNoise;
    public int[][] enablerCount; //each tile (first array) has 4 rotations(2nd array) and 4 directions of neighbors to contribute enablers(last array)

    public int chosenTile;

    public WFCGeneratorCellNoRotations(int numberOfTiles, float random, uint[][] freqs)
    {
        isCollapsed = false;
        possibilities = new bool[numberOfTiles];
        sumOfWeightLogWeights = 0;
        sumOfWeights = 0;
        for (int ii = 0; ii < numberOfTiles; ii++)
        {
            possibilities[ii] = true;
            sumOfWeights += freqs[ii][0];
            sumOfWeightLogWeights += freqs[ii][0] * Mathf.Log(freqs[ii][0]);
        }
        entropyNoise = random;
        enablerCount = new int[numberOfTiles][]; //each tile has 4 enabler counts, 1 per direction
        for (int ii = 0; ii < numberOfTiles; ii++)
        {
            enablerCount[ii] = new int[4];
        }
    }
    public void removeTile(uint tileIndex, uint[][] freqs)
    {
        possibilities[tileIndex] = false;
        sumOfWeights -= freqs[tileIndex][0];
        sumOfWeightLogWeights -= (freqs[tileIndex][0] * Mathf.Log(freqs[tileIndex][0]));
        Debug.Log("sumofweights: " + sumOfWeights);
    }
    public uint totalPossibilities(uint[][] freqs)
    {
        uint total = 0;
        for (int ii = 0; ii < freqs.Length; ii++)
        {
            if (possibilities[ii])
            {
                total += freqs[ii][0];
            }
        }
        return total;
    }
    public float getEntropy()
    {
        return Mathf.Log(sumOfWeights) - (sumOfWeightLogWeights / sumOfWeights);
    }
    public int chooseTileIndex(uint[][] freqs)
    {
        int index = (int)(Random.value * sumOfWeights);
        Debug.Log("sumofweights: " + sumOfWeights);
        Debug.Log("rng: " + index);
        for (int ii = 0; ii < possibilities.Length; ii++)
        {
            if (index >= freqs[ii][0])
            {
                index -= (int)freqs[ii][0];
                Debug.Log("rng update: " + index);
            }
            else
            {
                chosenTile = ii;
                return chosenTile;
            }
        }
        Debug.Log("chooseTileIndex...how did you end up here?!?");
        return 0;
    }
    public (int, int) getFinalTile()
    {
        for (int index = 0; index < possibilities.Length; index++)
        {
            for (int rot = 0; rot < 4; rot++)
            {
                if (possibilities[index])
                {
                    return (index, rot);
                }
            }
        }
        Debug.Log("couldnt find a final tile for whatever reason :/");
        return (0, 0);
    }
    public string getPossibilityString()
    {
        string message = "";
        for (int index = 0; index < possibilities.Length; index++)
        {
            if (possibilities[index])
            {
                message += ("\nindex: " + index);
            }
        }
        return message;
    }
    public string getEnablerString()
    {
        string message = "";
        for (int index = 0; index < enablerCount.Length; index++)
        {
            for (int dir = 0; dir < 4; dir++)
            {
                message += "index: " + index  + " dir: " + dir + "count: " + enablerCount[index][dir] + "\n";
            }
        }
        return message;
    }
} //testing*/