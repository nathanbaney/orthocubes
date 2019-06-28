using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockScript : MonoBehaviour
{
    public int block_size = 4;
    public GameObject voxel;
    public ulong block_perm;
    public BlockData block_data;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void instantiate_voxels()
    {
        for (int y = 0; y < block_size; y++)
        {
            for (int z = 0; z < block_size; z++)
            {
                for (int x = 0; x < block_size; x++)
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
        this.block_data = block_data;
        block_perm = System.Convert.ToUInt64(block_data.block_perm, 16);
    }
    public void build_block_perm()
    {
        ulong block_perm_temp = block_perm;
        int voxel_index = 0;
        int lowest_bit = 0x0;
        while (block_perm_temp != 0x0)
        {
            lowest_bit = (int)block_perm_temp & 0x1;
            if (lowest_bit == 0x1) //if the voxel should be drawn...
            {
                transform.Find("voxel" + voxel_index).gameObject.SetActive(true);
            }
            block_perm_temp = block_perm_temp >> 1;
            voxel_index++;
        }
    }
    public void build_material_overrides()
    {
        foreach (MaterialOverride mo in block_data.material_overrides)
        {
            print(mo.voxel_index + " " + mo.material);
            Material[] mats = transform.Find("voxel" + mo.voxel_index).GetComponent<MeshRenderer>().materials;
            mats[0] = Resources.Load<Material>(mo.material);
            transform.Find("voxel" + mo.voxel_index).GetComponent<MeshRenderer>().materials = mats;
        }
    }
}
[System.Serializable]
public class BlockData
{
    public string block_perm;
    public List<MaterialOverride> material_overrides;
}
[System.Serializable]
public class MaterialOverride
{
    public int voxel_index;
    public string material;
}
