using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PaletteMakerScript
{
    public void generateJSON(int[] values, int xSize, int ySize, bool rotations, string name)
    {
        WFCScript.PaletteData paletteData = new WFCScript.PaletteData();
        paletteData.xSize = xSize;
        paletteData.ySize = ySize;
        paletteData.rotations = rotations;
        paletteData.sampleArray = deserializeInputValues(values);
        StreamWriter writer = new StreamWriter("D:/dev/orthocubes/gamedata/" + name + ".json");
        writer.Write(JsonUtility.ToJson(paletteData));
        writer.Close();
    }
    /*private BlockData deserializeInputValue(int value)
    {
        BlockData block = new BlockData();
        ulong blockPerm = 0;
        switch (value)
        {
            case 0:
                blockPerm = 0x000000000000FFFF;
                block.blockPerm = blockPerm.ToString("X16");
                break;
            case 1:
                blockPerm = 0xF000F000F000FFFF;
                block.blockPerm = blockPerm.ToString("X16");
                break;
            case 2:
                blockPerm = 0x888888888888FFFF;
                block.blockPerm = blockPerm.ToString("X16");
                break;
            case 3:
                blockPerm = 0x000F000F000FFFFF;
                block.blockPerm = blockPerm.ToString("X16");
                break;
            case 4:
                blockPerm = 0x111111111111FFFF;
                block.blockPerm = blockPerm.ToString("X16");
                break;
            case 5:
                blockPerm = 0xF111F111F111FFFF;
                block.blockPerm = blockPerm.ToString("X16");
                break;
            case 6:
                blockPerm = 0xF888F888F888FFFF;
                block.blockPerm = blockPerm.ToString("X16");
                break;
            case 7:
                blockPerm = 0x888F888F888FFFFF;
                block.blockPerm = blockPerm.ToString("X16");
                break;
            case 8:
                blockPerm = 0x111F111F111FFFFF;
                block.blockPerm = blockPerm.ToString("X16");
                break;
            case 9:
                blockPerm = 0xF99FF99FF99FFFFF;
                block.blockPerm = blockPerm.ToString("X16");
                break;
            case 10:
                blockPerm = 0x999999999999FFFF;
                block.blockPerm = blockPerm.ToString("X16");
                break;
            case 11:
                blockPerm = 0xF00FF00FF00FFFFF;
                block.blockPerm = blockPerm.ToString("X16");
                break;
            case 12:
                blockPerm = 0xF999F999F999FFFF;
                block.blockPerm = blockPerm.ToString("X16");
                break;
            case 13:
                blockPerm = 0xF88FF88FF88FFFFF;
                block.blockPerm = blockPerm.ToString("X16");
                break;
            case 14:
                blockPerm = 0x999F999F999FFFFF;
                block.blockPerm = blockPerm.ToString("X16");
                break;
            case 15:
                blockPerm = 0xF11FF11FF11FFFFF;
                block.blockPerm = blockPerm.ToString("X16");
                break;
            case 16:
                blockPerm = 0x900990099009FFFF;
                block.blockPerm = blockPerm.ToString("X16");
                break;
            case 17:
                blockPerm = 0x900090009000FFFF;
                block.blockPerm = blockPerm.ToString("X16");
                break;
            case 18:
                blockPerm = 0x800880088008FFFF;
                block.blockPerm = blockPerm.ToString("X16");
                break;
            case 19:
                blockPerm = 0x000900090009FFFF;
                block.blockPerm = blockPerm.ToString("X16");
                break;
            case 20:
                blockPerm = 0x100110011001FFFF;
                block.blockPerm = blockPerm.ToString("X16");
                break;
            case 21:
                blockPerm = 0x900890089008FFFF;
                block.blockPerm = blockPerm.ToString("X16");
                break;
            case 22:
                blockPerm = 0x900890089008FFFF;
                block.blockPerm = blockPerm.ToString("X16");
                break;
            case 23:
                blockPerm = 0x800980098009FFFF;
                block.blockPerm = blockPerm.ToString("X16");
                break;
            case 24:
                blockPerm = 0x100910091009FFFF;
                block.blockPerm = blockPerm.ToString("X16");
                break;
            default:
                Debug.Log("invalid input value: " + value);
                break;
        }
        return block;
    }*/
    private BlockData deserializeInputValue(int value)
    {
        BlockData block = new BlockData();
        ulong blockPerm = 0;
        switch (value)
        {
            case 0:
                blockPerm = 0x000000000000FFFF;
                block.blockPerm = blockPerm.ToString("X16");
                block.walkableData = new bool[] {true,true,true,true,false,false};
                break;
            case 1:
                blockPerm = 0x0FF00FF00FF0FFFF;
                block.blockPerm = blockPerm.ToString("X16");
                block.walkableData = new bool[] { false, false, false, false, false, false };
                break;
            case 2:
                blockPerm = 0x666666666666FFFF;
                block.blockPerm = blockPerm.ToString("X16");
                block.walkableData = new bool[] { false, false, false, false, false, false };
                break;
            case 3:
                blockPerm = 0x0EE60EE60EE6FFFF;
                block.blockPerm = blockPerm.ToString("X16");
                block.walkableData = new bool[] { false, false, false, false, false, false };
                break;
            case 4:
                blockPerm = 0x077607760776FFFF;
                block.blockPerm = blockPerm.ToString("X16");
                block.walkableData = new bool[] { false, false, false, false, false, false };
                break;
            case 5:
                blockPerm = 0x677067706770FFFF;
                block.blockPerm = blockPerm.ToString("X16");
                block.walkableData = new bool[] { false, false, false, false, false, false };
                break;
            case 6:
                blockPerm = 0x6EE06EE06EE0FFFF;
                block.blockPerm = blockPerm.ToString("X16");
                block.walkableData = new bool[] { false, false, false, false, false, false };
                break;
            case 7:
                blockPerm = 0x0FF60FF60FF6FFFF;
                block.blockPerm = blockPerm.ToString("X16");
                block.walkableData = new bool[] { false, false, false, false, false, false };
                break;
            case 8:
                blockPerm = 0x6FF06FF06FF0FFFF;
                block.blockPerm = blockPerm.ToString("X16");
                block.walkableData = new bool[] { false, false, false, false, false, false };
                break;
            case 9:
                blockPerm = 0x6EE66EE66EE6FFFF;
                block.blockPerm = blockPerm.ToString("X16");
                block.walkableData = new bool[] { false, false, false, false, false, false };
                break;
            case 10:
                blockPerm = 0x677667766776FFFF;
                block.blockPerm = blockPerm.ToString("X16");
                block.walkableData = new bool[] { false, false, false, false, false, false };
                break;
            case 11:
                blockPerm = 0x6FF66FF66FF6FFFF;
                block.blockPerm = blockPerm.ToString("X16");
                block.walkableData = new bool[] { false, false, false, false, false, false };
                break;
            case 12:
                blockPerm = 0x099009900990FFFF;
                block.blockPerm = blockPerm.ToString("X16");
                block.walkableData = new bool[] { false, false, false, false, false, false };
                break;
            case 13:
                blockPerm = 0x600660066006FFFF;
                block.blockPerm = blockPerm.ToString("X16");
                block.walkableData = new bool[] { false, false, false, false, false, false };
                break;
            default:
                Debug.Log("invalid input value: " + value);
                break;
        }
        return block;
    }
    private BlockData[] deserializeInputValues(int[] values)
    {
        BlockData[] blocks = new BlockData[values.Length];
        for(int ii = 0; ii < values.Length; ii++)
        {
            blocks[ii] = deserializeInputValue(values[ii]);
        }
        return blocks;
    }
}
