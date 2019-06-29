using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelScript : MonoBehaviour
{
    public GameObject block;
    public int LEVEL_WIDTH = 20;
    public int LEVEL_HEIGHT = 1;
    public int LEVEL_LENGTH = 20;
    public GameObject[][,] level_blocks;
    public bool[] level_vis;
    private int block_size = 4;
    public Material mat1;
    public LevelData level_data;
    // Start is called before the first frame update
    void Start()
    {
        deserialize();
        build_blocks();
        build_block_perms();
        build_material_overrides();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void build_blocks()
    {
        level_blocks = new GameObject[LEVEL_HEIGHT][,];
        level_vis = new bool[LEVEL_HEIGHT];
        for (int y = 0; y < LEVEL_HEIGHT; y++)
        {
            level_blocks[y] = new GameObject[LEVEL_WIDTH, LEVEL_LENGTH];
            level_vis[y] = true;
            for (int x = 0; x < LEVEL_WIDTH; x++)
            {
                for (int z = 0; z < LEVEL_LENGTH; z++)
                {
                    level_blocks[y][x,z] = (GameObject)Instantiate(block, new Vector3(x * block_size, y * block_size, z * block_size), transform.rotation, transform);
                    level_blocks[y][x,z].name = "block" + (x + z * LEVEL_WIDTH + y * LEVEL_WIDTH * LEVEL_LENGTH);
                    level_blocks[y][x,z].GetComponent<BlockScript>().instantiate_voxels();
                    print(x + z * LEVEL_WIDTH + y * LEVEL_WIDTH * LEVEL_LENGTH);
                    level_blocks[y][x,z].GetComponent<BlockScript>().deserialize(level_data.block_data[x + z * LEVEL_WIDTH + y * LEVEL_WIDTH * LEVEL_LENGTH]);
                }
            }
        }
    }
    void build_block_perms()
    {
        for (int y = 0; y < LEVEL_HEIGHT; y++)
        {
            for (int x = 0; x < LEVEL_WIDTH; x++)
            {
                for (int z = 0; z < LEVEL_LENGTH; z++)
                {
                    level_blocks[y][x, z].GetComponent<BlockScript>().build_block_perm();
                }
            }
        }
    }
    void deserialize()
    {
        TextAsset json_string = Resources.Load<TextAsset>("FloorJSON/DebugFloor1");
        level_data = JsonUtility.FromJson<LevelData>(json_string.ToString());
    }

    void build_material_overrides()
    {
        for (int y = 0; y < LEVEL_HEIGHT; y++)
        {
            for (int x = 0; x < LEVEL_WIDTH; x++)
            {
                for (int z = 0; z < LEVEL_LENGTH; z++)
                {
                    level_blocks[y][x, z].GetComponent<BlockScript>().build_material_overrides();
                }
            }
        }
    }
    public void set_floor_visible(int floor, bool visible)
    {
        for (int x = 0; x < LEVEL_WIDTH; x++)
        {
            for (int z = 0; z < LEVEL_LENGTH; z++)
            {
                level_blocks[floor][x, z].GetComponent<BlockScript>().set_visible(visible);
            }
        }
        level_vis[floor] = visible;
    }
}
public class LevelData
{
    public BlockData[] block_data;
}
