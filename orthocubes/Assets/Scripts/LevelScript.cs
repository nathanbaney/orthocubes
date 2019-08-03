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
    public GridSpace[][,] grid;
    public bool[] levelVis;
    private int blockSize = 4;
    public Material mat1;
    public LevelData levelData;

    public Entity player;

    public Stack<Move> moveStack = new Stack<Move>();

    public ulong wallPerm = 0x000F000F000F000F;
    public Palette debugPalette;

    // Start is called before the first frame update
    void Start()
    {
        debugPalette = deserializePalette("PaletteJSON/debugPalette2");
        debugPalette.processTiles();
        //debugPalette.debugPrint();
        generateFromPalette(debugPalette);
        /*if (Resources.Load<TextAsset>("FloorJSON/floorData") != null)
        {
            print("deserializing");
            deserialize();
        }
        else
        {
            print("not deserializing");
        }
        /*else
        {
            print("building blank");
            buildBlankFloor();
        }*/
        buildBlocks();
        buildBlockPerms();
        buildMaterialOverrides();
        player = new Entity(new Coordinate(0, 0, 0), GameObject.Find("Player"));
        //debugBuildWall(4);
    }

    // Update is called once per frame
    void Update()
    {
        handleInput();
        handleMoves();
        checkVisibleFloors();
        //increment turn timer counter thing
        //decide ai moves
        //resolve all moves
        //wait for input
    }

    //initialization functions
    void buildBlocks()
    {
        grid = new GridSpace[LEVEL_HEIGHT][,];
        levelVis = new bool[LEVEL_HEIGHT];
        GameObject tempBlock;
        for (int y = 0; y < LEVEL_HEIGHT; y++)
        {
            grid[y] = new GridSpace[LEVEL_WIDTH, LEVEL_LENGTH];
            levelVis[y] = true;
            for (int x = 0; x < LEVEL_WIDTH; x++)
            {
                for (int z = 0; z < LEVEL_LENGTH; z++)
                {
                    tempBlock = (GameObject)Instantiate(block, new Vector3(x * blockSize, y * blockSize, z * blockSize), transform.rotation, transform);
                    grid[y][x, z] = new GridSpace(x, y, z, tempBlock);
                    grid[y][x,z].block.name = "block" + (x + z * LEVEL_WIDTH + y * LEVEL_WIDTH * LEVEL_LENGTH);
                    grid[y][x,z].block.GetComponent<BlockScript>().instantiateVoxels();
                    //print(x + z * LEVEL_WIDTH + y * LEVEL_WIDTH * LEVEL_LENGTH);
                    grid[y][x,z].block.GetComponent<BlockScript>().deserialize(levelData.blockData[x + z * LEVEL_WIDTH + y * LEVEL_WIDTH * LEVEL_LENGTH]);
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
                    grid[y][x, z].block.GetComponent<BlockScript>().buildBlockPerm();
                }
            }
        }
    }
    void deserialize()
    {
        TextAsset jsonString = Resources.Load<TextAsset>("FloorJSON/floorData");
        levelData = JsonUtility.FromJson<LevelData>(jsonString.ToString());
    }
    void buildMaterialOverrides()
    {
        for (int y = 0; y < LEVEL_HEIGHT; y++)
        {
            for (int x = 0; x < LEVEL_WIDTH; x++)
            {
                for (int z = 0; z < LEVEL_LENGTH; z++)
                {
                    grid[y][x, z].block.GetComponent<BlockScript>().buildMaterialOverrides();
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

    //generation functions
    Palette deserializePalette(string path)
    {
        TextAsset jsonString = Resources.Load<TextAsset>(path);
        PaletteData data = JsonUtility.FromJson<PaletteData>(jsonString.ToString());
        Palette palette = new Palette();
        palette.deserialize(data);
        return palette;
    }
    void debugBuildWall(int xpos)
    {
        ulong rotatedPerm = 0;
        for(int ii = 0; ii < 10; ii++)
        {
            if (rotatedPerm == 0)
            {
                rotatedPerm =  BlockScript.getRotation(wallPerm, 1);
            }
            grid[0][xpos, ii].block.GetComponent<BlockScript>().combinePerm(rotatedPerm);
        }
    }
    void generateFromPalette(Palette palette)
    {
        WFCGenerator gen = new WFCGenerator(palette, LEVEL_WIDTH, LEVEL_LENGTH);
        (int,int)[] decodedGrid = gen.run();
        levelData = new LevelData(LEVEL_WIDTH * LEVEL_LENGTH * LEVEL_HEIGHT);
        for (int y = 0; y < LEVEL_HEIGHT; y++)
        {
            for (int z = 0; z < LEVEL_LENGTH; z++)
            {
                for (int x = 0; x < LEVEL_WIDTH; x++)
                {
                    levelData.blockData[x + z * LEVEL_WIDTH + y * LEVEL_LENGTH * LEVEL_WIDTH] = new BlockData();
                    levelData.blockData[x + z * LEVEL_WIDTH + y * LEVEL_LENGTH * LEVEL_WIDTH].blockPerm = palette.tiles[decodedGrid[x + z * LEVEL_WIDTH].Item1][decodedGrid[x + z * LEVEL_WIDTH].Item2].blocks[0,0].blockPerm;
                    Debug.Log("index:" + decodedGrid[x + z * LEVEL_WIDTH].Item1 + " rot:" + decodedGrid[x + z * LEVEL_WIDTH].Item2 + " " + palette.tiles[decodedGrid[x + z * LEVEL_WIDTH].Item1][decodedGrid[x + z * LEVEL_WIDTH].Item2].blocks[0, 0].blockPerm + " whats set: " + levelData.blockData[x + z * LEVEL_WIDTH + y * LEVEL_LENGTH * LEVEL_WIDTH].blockPerm);
                }
            }
        }
        StreamWriter writer = new StreamWriter("Assets/Resources/FloorJSON/floorData.json");
        writer.Write(JsonUtility.ToJson(levelData, true));
        writer.Close();
    }

    //entity + visibility functions
    public void setFloorVisible(int floor, bool visible)
    {
        for (int x = 0; x < LEVEL_WIDTH; x++)
        {
            for (int z = 0; z < LEVEL_LENGTH; z++)
            {
                grid[floor][x, z].block.GetComponent<BlockScript>().setVisible(visible);
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
    public void handleInput()
    {
        if (Input.GetKeyUp(KeyCode.Keypad8))
        {
            moveStack.Push(new Move(Coordinate.getNorth(player.position),player));
        }
        else if (Input.GetKeyUp(KeyCode.Keypad2))
        {
            moveStack.Push(new Move(Coordinate.getSouth(player.position), player));
        }
        else if (Input.GetKeyUp(KeyCode.Keypad6))
        {
            moveStack.Push(new Move(Coordinate.getEast(player.position), player));
        }
        else if (Input.GetKeyUp(KeyCode.Keypad4))
        {
            moveStack.Push(new Move(Coordinate.getWest(player.position), player));
        }
        else if (Input.GetKeyUp(KeyCode.KeypadMinus))
        {
            moveStack.Push(new Move(Coordinate.getUp(player.position), player));
        }
        else if (Input.GetKeyUp(KeyCode.KeypadPlus))
        {
            moveStack.Push(new Move(Coordinate.getDown(player.position), player));
        }
    }
    public void handleMoves()
    {
        while(moveStack.Count != 0)
        {
            if (isValidMove(moveStack.Peek())){
                moveStack.Pop().execute();
            }
        }
    }
    public bool isValidMove(Move move)
    {
        if(move.destination.y < LEVEL_HEIGHT && move.destination.y >= 0)
        {
            return true;
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
        return new Coordinate(coordinate.x, coordinate.y, coordinate.z + 1);
    }
    public static Coordinate getWest(Coordinate coordinate)
    {
        return new Coordinate(coordinate.x, coordinate.y, coordinate.z - 1);
    }
    public static Coordinate getUp(Coordinate coordinate)
    {
        return new Coordinate(coordinate.x, coordinate.y+1, coordinate.z);
    }
    public static Coordinate getDown(Coordinate coordinate)
    {
        return new Coordinate(coordinate.x, coordinate.y-1, coordinate.z);
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
    public void moveTo(Coordinate destination)
    {
        position = destination;
        gameObject.transform.position = position.getWorldCoordinate();
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
    public Coordinate destination;

    public Move(Coordinate destination, Entity entity)
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
public class Palette
{
    public PaletteData paletteData;
    public uint xSize; //horizontal size of sample image
    public uint ySize; //vertical size of sample image
    public BlockData[,] sampleBlockArray; //the original sample "image", made 2d array
    public uint numberOfTiles;

    public uint tileSize; //ALWAYS 3, IM TIRED OF MATRIX STUFF, NO EXCEPTIONS
    public Tile[][] tiles; //the set of tiles that make up the sample image. tileindex, rotation
    public uint[][] frequencies; //the amount that each tile occurs in the sample, rotations counted separately
    public bool[,,,,] adjacencyRules; //whether each tile and their rotations can exist 1 unit cardinal direction away from another tile without contradicting overlap

    public Palette()
    {

    }
    //sets tiles, frequencies, adjacency rules
    public void processTiles()
    {
        tileSize = 3;
        numberOfTiles = xSize * ySize;
        getTiles();
        getFrequencies();
        getAdjacencyRules();
    }
    private void getTiles()
    {
        tiles = new Tile[numberOfTiles][];
        for (int ii = 0; ii < numberOfTiles; ii++)
        {
            tiles[ii] = new Tile[4];
        }
        int index = 0;
        for(uint y = 0; y < ySize; y++)
        {
            for(uint x = 0; x < xSize; x++)
            {
                tiles[index][0] = getTileAtPosition(x, y);
                for (uint rot = 1; rot < 4; rot++)
                {
                    //try this line next
                    tiles[index][rot] = getRotatedTile(tiles[index][0], rot);
                }
                index++;
            }
        }
    }
    private Tile getTileAtPosition(uint x, uint y)
    {
        Tile tile = new Tile(tileSize);
        for(int ii = 0; ii < tileSize; ii++)
        {
            for(int jj = 0; jj < tileSize; jj++)
            {
                tile.setBlock(ii,jj,sampleBlockArray[(x + ii)%xSize, (y + jj)%ySize]);
            }
        }
        return tile;
    }
    private bool tileEquals(Tile tileA, Tile tileB)
    {
        for (int ii = 0; ii < tileSize; ii++)
        {
            for (int jj = 0; jj < tileSize; jj++)
            {
                if (!string.Equals(tileB.blocks[ii, jj].blockPerm, tileA.blocks[ii, jj].blockPerm))
                {
                    return false;
                }
            }
        }
        return true;
    }
    private void getFrequencies()
    {
        frequencies = new uint[numberOfTiles][];
        for (int ii = 0; ii < numberOfTiles; ii++)
        {
            frequencies[ii] = new uint[4];
            for (int rot = 0; rot < 4; rot++)
            {
                frequencies[ii][rot] = 1;
            }
        }
        for (uint tileA = 0; tileA < numberOfTiles; tileA++)
        {
            for (uint tileARots = 0; tileARots < 4; tileARots++)
            {
                for (uint tileB = 0; tileB < numberOfTiles; tileB++)
                {
                    for (uint tileBRots = 0; tileBRots < 4; tileBRots++)
                    {
                        if (tileEquals(tiles[tileA][tileARots], tiles[tileB][tileBRots]))
                        {
                            frequencies[tileA][tileARots]++;
                            frequencies[tileB][tileBRots]++;
                        }
                    }
                }
            }
        }
    }
    /*cutting this for now, this is for 4x4 tiles
     * private (int, int)[,] tileRotationArray =
    {
        {(0,3), (0,2), (0,1), (0,0) },
        {(1,3), (1,2), (1,1), (1,0) },
        {(2,3), (2,2), (2,1), (2,0) },
        {(3,3), (3,2), (3,1), (3,0) }
    };*/
    public static (int, int)[,] tileRotationArray =
    {
        {(0,2), (0,1), (0,0) },
        {(1,2), (1,1), (1,0) },
        {(2,2), (2,1), (2,0) }
    };
    public Tile getRotatedTile(Tile tile, uint rotations)
    {
        Tile rotatedTile = new Tile(tileSize);
        rotatedTile.setBlocks(tile.blocks);
        for (int rots = 0; rots < rotations; rots++)
        {
            rotatedTile = getRotatedTile(rotatedTile);
        }
        return rotatedTile;
    }
    public Tile getRotatedTile(Tile tile) 
    {
        Tile rotatedTile = new Tile(tileSize);
        for (uint x = 0; x < tileSize; x++)
        {
            for (uint y = 0; y < tileSize; y++)
            {
                (int, int) rot = tileRotationArray[x, y];
                rotatedTile.setBlock(rot.Item1, rot.Item2, tile.blocks[x, y]);
                rotatedTile.blocks[rot.Item1, rot.Item2].blockPerm = BlockScript.getRotation(rotatedTile.blocks[rot.Item1, rot.Item2].blockPerm, 1);
            }
        }
        return rotatedTile;
    }
    //wow, quintuple nested for loop. i feel so dirty
    public void getAdjacencyRules()
    {
        adjacencyRules = new bool[numberOfTiles, 4, numberOfTiles, 4, 4]; //tileindex, tilerotation, tileindex, tilerotation, direction
        for(uint tileA = 0; tileA < numberOfTiles; tileA++)
        {
            for(uint tileARots = 0; tileARots < 4; tileARots++)
            {
                for(uint tileB = 0; tileB < numberOfTiles; tileB++)
                {
                    for(uint tileBRots = 0; tileBRots < 4; tileBRots++)
                    {
                        for(uint direction = 0; direction < 4; direction++)
                        {
                            adjacencyRules[tileA, tileARots, tileB, tileBRots, direction] = compatible(tiles[tileA][tileARots], tiles[tileB][tileBRots], direction);
                        }
                    }
                }
            }
        }
    }
    public bool compatible(Tile tileA, Tile tileB, uint direction) //0 is up, 1 is right, 2 is down, 3 is left
    {
        switch (direction)
        {
            case 0:
                return compatibleUp(tileA, tileB);
            case 1:
                return compatibleRight(tileA, tileB);
            case 2:
                return compatibleDown(tileA, tileB);
            case 3:
                return compatibleLeft(tileA, tileB);
        }

        return true;
    }
    private bool compatibleUp(Tile tileA, Tile tileB)
    {
        for(uint x = 0; x < tileSize; x++)
        {
            for(uint y = 0; y < tileSize - 1; y++)
            {
                if(tileA.blocks[x,y].blockPerm != tileB.blocks[x, y + 1].blockPerm)
                {
                    return false;
                }
            }
        }
        return true;
    }
    private bool compatibleRight(Tile tileA, Tile tileB)
    {
        for (uint x = 1; x < tileSize; x++)
        {
            for (uint y = 0; y < tileSize; y++)
            {
                if (tileA.blocks[x, y].blockPerm != tileB.blocks[x - 1, y].blockPerm)
                {
                    return false;
                }
            }
        }
        return true;
    }
    private bool compatibleDown(Tile tileA, Tile tileB)
    {
        for (uint x = 0; x < tileSize; x++)
        {
            for (uint y = 1; y < tileSize; y++)
            {
                if (tileA.blocks[x, y].blockPerm != tileB.blocks[x, y - 1].blockPerm)
                {
                    return false;
                }
            }
        }
        return true;
    }
    private bool compatibleLeft(Tile tileA, Tile tileB)
    {
        for (uint x = 0; x < tileSize - 1; x++)
        {
            for (uint y = 0; y < tileSize; y++)
            {
                if (tileA.blocks[x, y].blockPerm != tileB.blocks[x + 1, y].blockPerm)
                {
                    return false;
                }
            }
        }
        return true;
    }
    public void deserialize(PaletteData data)
    {
        this.paletteData = data;
        this.xSize = (uint)data.xSize;
        this.ySize = (uint)data.ySize;
        sampleBlockArray = new BlockData[xSize, ySize];
        int index = 0;
        for(int y = 0; y < ySize; y++)
        {
            for(int x = 0; x < xSize; x++)
            {
                sampleBlockArray[x, y] = data.sampleArray[index];
                index++;
            }
        }
    }
    public void debugPrint()
    {
        Debug.Log(xSize);
        Debug.Log(ySize);
        Debug.Log(numberOfTiles);
        /*for (int ii = 0; ii < numberOfTiles; ii++)
        {
            Debug.Log("tile #" + ii);
            for (int x = 0; x < tileSize; x++)
            {
                for (int y = 0; y < tileSize; y++)
                {
                    Debug.Log(tiles[ii][0].blocks[x,y].blockPerm);
                }
            }
        }*/
        for(int ii = 0; ii < numberOfTiles; ii++)
        {
            Debug.Log("tile# " + ii);
            Debug.Log(tiles[ii][0].getString());
            Debug.Log("count:" + frequencies[ii]);
        }
    }
}
public class Tile
{
    public BlockData[,] blocks;
    public Tile(uint size)
    {
        this.blocks = new BlockData[size, size];
    }
    public void setBlock(int x, int y, BlockData block)
    {
        blocks[x, y] = block;
    }
    public void setBlocks(BlockData[,] blocks)
    {
        this.blocks = blocks;
    }
    public string getString()
    {
        StringWriter writer = new StringWriter();
        for(int y = 0; y < 3; y++)
        {
            for(int x = 0; x < 3; x++)
            {
                writer.Write(blocks[x, y].blockPerm + "\t");
            }
            writer.Write("\n");
        }
        return writer.ToString();
    }
}
[System.Serializable]
public class PaletteData
{
    public int xSize;
    public int ySize;
    public BlockData[] sampleArray;
}
public class WFCGenerator
{
    public WFCGeneratorCell[][] grid;
    public int xSize;
    public int ySize;
    private int uncollapsedCells;
    private Palette palette;

    private Heap heap;
    private Stack<RemovalUpdate> removalStack;

    public WFCGenerator(Palette palette, int xSize, int ySize)
    {
        uncollapsedCells = xSize * ySize;
        this.palette = palette;
        this.xSize = xSize;
        this.ySize = ySize;
        grid = new WFCGeneratorCell[xSize][];
        ((int, int), float)[] input = new ((int, int), float)[xSize * ySize];
        int index = 0;
        for (int x = 0; x < xSize; x++)
        {
            grid[x] = new WFCGeneratorCell[ySize];
            for(int y = 0; y < ySize; y++)
            {
                grid[x][y] = new WFCGeneratorCell((int)palette.numberOfTiles, Random.value / 100f, palette.frequencies);
                input[index++] = ((x, y), grid[x][y].getEntropy());
            }
        }
        heap = new Heap(input, xSize * ySize * 10);
        removalStack = new Stack<RemovalUpdate>();
        initializeEnablerCounts();
    }
    private void initializeEnablerCounts()
    {
        for(int x = 0; x < xSize; x++)
        {
            for(int y = 0; y < ySize; y++)
            {
                for(int tileA = 0; tileA < palette.numberOfTiles; tileA++)
                {
                    for(int tileARot = 0; tileARot < 4; tileARot++)
                    {
                        int[] tempCount = { 0,0,0,0};
                        for(uint dir = 0; dir < 4; dir++) //for each cell next to the current cell...
                        {
                            for(int tileB = 0; tileB > palette.numberOfTiles; tileB++)
                            {
                                for(int tileBRot = 0; tileBRot < 4; tileBRot++)
                                {
                                    if (palette.compatible(palette.tiles[tileA][tileARot], palette.tiles[tileB][tileBRot], dir))
                                    {
                                        tempCount[dir]++;
                                    }
                                }
                            }
                        }
                        grid[x][y].enablerCount[tileA] = tempCount;
                    }
                }
            }
        }
    }
    private (int,int)? chooseNextCell()
    {
        while (!heap.isEmpty())
        {
            (int, int) coord = heap.extractMin().Item1;
            if (!grid[coord.Item1][coord.Item2].isCollapsed)
            {
                return coord;
            }
        }
        return null;
    }
    private void collapseCellAt((int, int) coord)
    {
        (int,int) tileIndex = grid[coord.Item1][coord.Item2].chooseTileIndex(palette.frequencies);
        grid[coord.Item1][coord.Item2].isCollapsed = true;
        for(int ii = 0; ii < grid[coord.Item1][coord.Item2].possibilities.Length; ii++)
        {
            if (ii != tileIndex.Item1)
            {
                for(int rot = 0; rot < 4; rot++)
                {
                    grid[coord.Item1][coord.Item2].possibilities[ii][rot] = false;
                    removalStack.Push(new RemovalUpdate(ii, rot, coord));
                }
            }
            else
            {
                for (int rot = 0; rot < 4; rot++)
                {
                    if(rot != tileIndex.Item2)
                    {
                        grid[coord.Item1][coord.Item2].possibilities[ii][rot] = false;
                        removalStack.Push(new RemovalUpdate(ii, rot, coord));
                    }
                }
            }
        }
    }
    private void propagate()
    {
        while(removalStack.Count > 0)
        {
            RemovalUpdate update = removalStack.Pop();
            for(uint dir = 0; dir < 4; dir++)
            {
                (int, int) neighbor = getNeighbor(update.coordinate, (int)dir);
                for (int tileB = 0; tileB > palette.numberOfTiles; tileB++)
                {
                    for (int tileBRot = 0; tileBRot < 4; tileBRot++)
                    {
                        if (palette.compatible(palette.tiles[update.index][update.rotation], palette.tiles[tileB][tileBRot], dir)) //for each compatible tile in the specified direction from the removal update tile...
                        {
                            if(grid[neighbor.Item1][neighbor.Item2].enablerCount[update.index][update.rotation] == 1) //if we are about to set a count to zero...
                            {
                                bool hasZero = false;
                                foreach(int[] tileCount in grid[neighbor.Item1][neighbor.Item2].enablerCount)
                                {
                                    for(int tileRot = 0; tileRot < 4; tileRot++)
                                    {
                                        if (tileCount[tileRot] == 0) //if theres already a zero, then the potential has already been removed, and we dont want to re-remove it
                                        {
                                            hasZero = true;
                                        }
                                    }
                                }
                                if (!hasZero) //if is hasnt already been removed, remove it 
                                {
                                    grid[neighbor.Item1][neighbor.Item2].removeTile((uint)tileB, (uint)tileBRot, palette.frequencies);
                                    if(grid[neighbor.Item1][neighbor.Item2].totalPossibilities(palette.frequencies) == 0)
                                    {
                                        //hit a contradiction, need to restart algo
                                        Debug.Log("hit a contradiction, need to restart algorithm");
                                    }
                                    heap.insert((neighbor, grid[neighbor.Item1][neighbor.Item2].getEntropy()));
                                    removalStack.Push(new RemovalUpdate(tileB, tileBRot, neighbor));
                                }
                            }
                            grid[neighbor.Item1][neighbor.Item2].enablerCount[update.index][update.rotation]--;
                        }
                    }
                }
            }
        }
    }
    public (int,int)[] run()
    {
        while(uncollapsedCells > 0)
        {
            (int, int) nextCell = ((int,int))chooseNextCell();
            collapseCellAt(nextCell);
            propagate();
            uncollapsedCells--;
        }
        return decodeGrid();
    }
    private (int,int)[] decodeGrid()
    {
        (int, int)[] decodedGrid = new (int, int)[xSize * ySize];
        for(int y = 0; y < ySize; y++)
        {
            for(int x = 0; x < xSize; x++)
            {
                decodedGrid[x + y * xSize] = grid[x][y].getFinalTile();
            }
        }
        return decodedGrid;
    }
    private (int,int) getNeighbor((int,int) start, int direction)
    {
        switch (direction)
        {
            case 0: //up
                return (start.Item1, start.Item2 - 1);
            case 1: //right
                return (start.Item1 + 1, start.Item2);
            case 2:
                return (start.Item1, start.Item2 + 1);
            case 3:
                return (start.Item1 - 1, start.Item2);
        }
        return start;
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
    private class Heap //yeah, c# really doesnt have a heap in their standard lib. im thrilled
    {
        ((int, int), float)[] heapContents; //the int pair is the index of the gridspace, the float is the entropy
        int heapSize;

        public Heap(((int, int), float)[] input, int maxSize)
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
            if(left < heapSize && heapContents[left].Item2 < heapContents[index].Item2)
            {
                smallest = left;
            }
            if(right < heapSize && heapContents[right].Item2 < heapContents[index].Item2)
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
            for(int ii = 0; ii < input.Length; ii++)
            {
                heapContents[ii] = input[ii];
            }
            for(int ii = heapSize / 2; ii >= 0; ii--)
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
                while(tempIndex > 0 && heapContents[getParent(tempIndex)].Item2 < heapContents[tempIndex].Item2)
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
        public ((int,int), float) extractMin()
        {
            ((int, int), float) min = heapContents[0];
            heapContents[0] = heapContents[--heapSize];
            heapify(0);
            return min;
        }
        public void insert(((int, int), float) item)
        {
            int index = heapSize++;
            heapContents[index] = item;
            heapContents[index].Item2 = float.MaxValue;
            decreaseKey(index, item.Item2);
        }
    }
    private class RemovalUpdate
    {
        public int index;
        public int rotation;
        public (int,int) coordinate;

        public RemovalUpdate(int index, int rotation, (int,int) coordinate)
        {
            this.index = index;
            this.rotation = rotation;
            this.coordinate = coordinate;
        }
    }

}
public class WFCGeneratorCell
{
    public bool isCollapsed;
    public bool[][] possibilities;
    uint sumOfWeights;
    float sumOfWeightLogWeights;
    public float entropyNoise;
    public int[][] enablerCount;

    public WFCGeneratorCell(int numberOfTiles, float random, uint[][] freqs)
    {
        isCollapsed = false;
        possibilities = new bool[numberOfTiles][];
        for(int ii = 0; ii < numberOfTiles; ii++)
        {
            possibilities[ii] = new bool[4];
            for (int jj = 0; jj < 4; jj++) //set rotations too, dummy
            {
                possibilities[ii][jj] = true;
                sumOfWeights += freqs[ii][jj];
            }
        }
        entropyNoise = random;
        enablerCount = new int[numberOfTiles][]; //each tile has 4 enabler counts, 1 for each neighbor
        for(int ii = 0; ii < numberOfTiles; ii++)
        {
            enablerCount[ii] = new int[4];
        }
    }
    public void removeTile(uint tileIndex, uint rotation, uint[][] freqs)
    {
        possibilities[tileIndex][rotation] = false;
        sumOfWeights -= freqs[tileIndex][rotation];
        sumOfWeightLogWeights -= (freqs[tileIndex][rotation] * Mathf.Log(freqs[tileIndex][rotation]));
    }
    public uint totalPossibilities(uint[][] freqs)
    {
        uint total = 0;
        for(int ii = 0; ii < freqs.Length; ii++)
        {
            for(int rots = 0; rots < 4; rots++)
            {
                if (possibilities[ii][rots])
                {
                    total += freqs[ii][rots];
                }
            }
        }
        return total;
    }
    public float getEntropy()
    {
        return Mathf.Log(sumOfWeights) - (sumOfWeightLogWeights / sumOfWeights);
    }
    public (int,int) chooseTileIndex(uint[][] freqs)
    {
        int index = (int)(Random.value * sumOfWeights);
        for(int ii = 0; ii < possibilities.Length; ii++)
        {
            uint sum = 0;
            foreach(uint freq in freqs[ii])
            {
                sum += freq;
            }
            if (index >= sum)
            {
                index -= (int)sum;
            }
            else
            {
                for(int rot = 0; rot < 4; rot++)
                {

                    if (possibilities[ii][rot])
                    {
                        return (ii, rot);
                    }
                }
            }
        }
        Debug.Log("chooseTileIndex...how did you end up here?!?");
        return (0,0);
    }
    public (int,int) getFinalTile()
    {
        for(int index = 0; index < possibilities.Length; index++)
        {
            for(int rot = 0; rot < 4; rot++)
            {
                if (possibilities[index][rot])
                {
                    return (index, rot);
                }
            }
        }
        Debug.Log("couldnt find a final tile for whatever reason :/");
        return (0, 0);
    }
}
