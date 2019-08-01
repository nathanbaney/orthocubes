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
        if (Resources.Load<TextAsset>("FloorJSON/floorData") != null)
        {
            deserialize();
        }
        else
        {
            buildBlankFloor();
        }
        buildBlocks();
        buildBlockPerms();
        buildMaterialOverrides();
        player = new Entity(new Coordinate(0, 0, 0), GameObject.Find("Player"));
        debugPalette = deserializePalette("PaletteJSON/debugPalette");
        debugPalette.processTiles();
        //debugPalette.debugPrint();
        
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
        return new Vector3(x*BlockScript.blockSize+2, y * BlockScript.blockSize + 2, z * BlockScript.blockSize + 2);
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
    BlockData[,] sampleBlockArray; //the original sample "image", made 2d array
    uint numberOfTiles;

    uint tileSize; //ALWAYS 3, IM TIRED OF MATRIX STUFF, NO EXCEPTIONS
    Tile[][] tiles; //the set of tiles that make up the sample image. tileindex, rotation, the actual tile itself.
    uint[] frequencies; //the amount that each tile occurs in the sample
    bool[,,,,] adjacencyRules; //whether each tile and their rotations can exist 1 unit cardinal direction away from another tile without contradicting overlap

    public Palette()
    {

    }
    //sets tiles, frequencies, adjacency rules
    public void processTiles()
    {
        tileSize = 3;
        numberOfTiles = xSize * ySize;
        getTiles();
        for (int ii = 0; ii < numberOfTiles; ii++)
        {
            Debug.Log(tiles[ii][0].blocks[0, 0].blockPerm);
        }
        //frequencies = getFrequencies();
        //adjacencyRules = getAdjacencyRules();
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
                //Debug.Log(sampleBlockArray[(x + ii) % xSize, (y + jj) % ySize].blockPerm);
                //Debug.Log(sampleArray[(x + ii) % xSize + ((y + jj) % ySize)*xSize].blockPerm);
                tile.setBlock(ii,jj,sampleBlockArray[(x + ii)%xSize, (y + jj)%ySize]);
            }
        }
        return tile;
    } 
    private uint[] getFrequencies()
    {
        uint[] freqs = new uint[numberOfTiles];
        for(int ii = 0; ii < numberOfTiles; ii++)
        {
            freqs[ii] += 1;
            for(int jj = ii+1; jj < numberOfTiles; jj++)
            {
                int kk = 0;
                while(kk < 4)
                {
                    if(tileEquals(tiles[ii][kk], tiles[jj][0])){
                        freqs[ii]++;
                        freqs[jj]++;
                        kk += 4;
                    }
                    kk++;
                }
            }
        }
        return freqs;
    }
    private bool tileEquals(Tile tileA, Tile tileB)
    {
        for(int ii = 0; ii < tileSize; ii++)
        {
            for(int jj = 0; jj < tileSize; jj++)
            {
                if (!tileA.blocks[ii, jj].blockPerm.Equals(tileB.blocks[ii, jj]))
                {
                    return false;
                }
            }
        }
        return true;
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
                Debug.Log("pos:" + x + " " + y + " perm: " + tile.blocks[x, y].blockPerm);
                (int, int) rot = tileRotationArray[x, y];
                rotatedTile.setBlock(rot.Item1, rot.Item2, tile.blocks[x, y]);
                ulong perm = System.Convert.ToUInt64(rotatedTile.blocks[rot.Item1, rot.Item2].blockPerm, 16);
                string permString = BlockScript.getRotation(perm, 1).ToString("X");
                rotatedTile.blocks[rot.Item1, rot.Item2].blockPerm = permString;
                Debug.Log("pos:" + x + " " + y + " perm: " + tile.blocks[x,y].blockPerm);
            }
        }
        return rotatedTile;
    }
    //wow, quintuple nested for loop. i feel so dirty
    public bool[,,,,] getAdjacencyRules()
    {
        bool[,,,,] rules = new bool[numberOfTiles, 4, numberOfTiles, 4, 4]; //tileindex, tilerotation, tileindex, tilerotation, direction
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
                            rules[tileA, tileARots, tileB, tileBRots, direction] = compatible(tiles[tileA][tileARots], tiles[tileB][tileBRots], direction);
                        }
                    }
                }
            }
        }
        return rules;
    }
    private bool compatible(Tile tileA, Tile tileB, uint direction) //0 is up, 1 is right, 2 is down, 3 is left
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
        for (int ii = 0; ii < numberOfTiles; ii++)
        {
            Debug.Log("tile #" + ii);
            for (int x = 0; x < tileSize; x++)
            {
                for (int y = 0; y < tileSize; y++)
                {
                    Debug.Log(tiles[ii][0].blocks[x,y].blockPerm);
                }
            }
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
}
[System.Serializable]
public class PaletteData
{
    public int xSize;
    public int ySize;
    public BlockData[] sampleArray;
}
