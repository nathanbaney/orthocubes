using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockScript : MonoBehaviour
{
    public static int blockSize = 4;
    public GameObject voxel;
    public ulong blockPerm;
    public BlockData blockData;
    public bool[] walkable; //0 is NE, 1 SE, 2 SW, 3 NW, 4 up, 5 down

    public ulong wallPerm = 0L; //System.Convert.ToUInt64("1000000000F00000",16);
    // Start is called before the first frame update
    void Start()
    {
        wallPerm = 0x000F000F000F000F;
        print(wallPerm);
        print(getRotation(wallPerm, 1));
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void instantiateVoxels()
    {
        for (int y = 0; y < blockSize; y++)
        {
            for (int z = 0; z < blockSize; z++)
            {
                for (int x = 0; x < blockSize; x++)
                {
                    int id = x + 4 * z + 16 * y;
                    Vector3 position = new Vector3(x, y, z);
                    GameObject instance = Instantiate(voxel, position + transform.position, transform.rotation, transform);
                    instance.name = "voxel" + id;
                    instance.SetActive(false);
                }
            }
        }
    }
    public void deserialize(BlockData block_data)
    {
        this.blockData = block_data;
        blockPerm = System.Convert.ToUInt64(block_data.blockPerm, 16);
        walkable = block_data.walkableData;
        //print(block_data.walkableData[0]);
    }
    public void buildBlockPerm()
    {
        ulong blockPermTemp = blockPerm;
        int voxelIndex = 0;
        int lowestBit = 0x0;
        while (blockPermTemp != 0x0)
        {
            lowestBit = (int)blockPermTemp & 0x1;
            if (lowestBit == 0x1) //if the voxel should be drawn...
            {
                transform.Find("voxel" + voxelIndex).gameObject.SetActive(true);
            }
            blockPermTemp = blockPermTemp >> 1;
            voxelIndex++;
        }
    }
    public void buildMaterialOverrides()
    {
        foreach (MaterialOverride mo in blockData.materialOverrides)
        {
            //print(mo.voxelIndex + " " + mo.material);
            Material[] mats = transform.Find("voxel" + mo.voxelIndex).GetComponent<MeshRenderer>().materials;
            mats[0] = Resources.Load<Material>(mo.material);
            transform.Find("voxel" + mo.voxelIndex).GetComponent<MeshRenderer>().materials = mats;
        }
    }
    public void setVisible(bool visible)
    {
        foreach(MeshRenderer voxel in GetComponentsInChildren<MeshRenderer>())
        {
            voxel.enabled = visible;
        }
    }
    //basically, when doing a 90 degree rotation counter clockwise around the y axis, each voxel at the specified index will go to the 
    //next specified index. Indexing for voxels is as follows, incrementing first on the x axis:
    //     F
    //    E B   
    //   D A 7 
    //  C 9 6 3
    //   8 5 2
    //    4 1
    //     0
    // where the next y level with start with 16 where the 0 place is, incrementing the same way (x first, then z, then y)
    private byte[][] rotationArray = new byte[][] 
    {
        new byte[]{0,3,15,12},
        new byte[]{1,7,14,8},
        new byte[]{2,11,13,4},
        new byte[]{5,6,10,9}
    };
    private int getRotationIndex(byte index, byte rotation) //1 is a 90 degree rotation ccw, 2 is 180, NO NEGATIVE ROTATIONS
    {
        int rotatedIndex = 64;
        byte rotationArrayLength = 4;
        byte rotationLevelLength = 16;
        foreach(byte[] array in rotationArray)
        {
            for(byte ii = 0; ii < array.Length; ii++)
            {
                if(array[ii] == index % rotationLevelLength)
                {
                    rotatedIndex = array[(ii + rotation) % rotationArrayLength] + rotationLevelLength * (index / rotationLevelLength);
                }
            }
        }
        return rotatedIndex;
    }

    public ulong getRotation(ulong blockPerm, byte rotation)
    {
        ulong rotatedPerm = 0;
        ulong bit = 1;
        for (byte index = 0; index < 64; index++)
        {
            ulong startingBitValue = blockPerm & (bit << index);
            ulong endingBit = startingBitValue << getRotationIndex(index, rotation) - index;
            rotatedPerm = rotatedPerm | endingBit;
        }
        return rotatedPerm;
    }
}
[System.Serializable]
public class BlockData
{
    public string blockPerm;
    public bool[] walkableData;
    public List<MaterialOverride> materialOverrides;
}
[System.Serializable]
public class MaterialOverride
{
    public int voxelIndex;
    public string material;
}
