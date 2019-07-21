using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelScript : MonoBehaviour
{
    public GameObject block;
    public int LEVEL_WIDTH = 10;
    public int LEVEL_HEIGHT = 4;
    public int LEVEL_LENGTH = 10;
    public GridSpace[][,] grid;
    public bool[] levelVis;
    private int blockSize = 4;
    public Material mat1;
    public LevelData levelData;

    public Entity player;

    public Stack<Move> moveStack = new Stack<Move>();

    // Start is called before the first frame update
    void Start()
    {
        deserialize();
        buildBlocks();
        buildBlockPerms();
        buildMaterialOverrides();
        player = new Entity(new Coordinate(0, 0, 0), GameObject.Find("Player"));
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
        TextAsset jsonString = Resources.Load<TextAsset>("FloorJSON/DebugFloor2");
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
