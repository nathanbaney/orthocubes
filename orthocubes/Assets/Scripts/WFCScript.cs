using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class WFCScript
{
    public int xSize, ySize;
    private int uncollapsedCells;
    private int forcedCells;
    private Palette palette;
    private WFCCell[][] grid;
    private Heap entropyHeap;
    private Stack<RemovalUpdate> removalStack;

    public WFCScript(int xSize, int ySize, string pathToPaletteJson)
    {
        this.xSize = xSize;
        this.ySize = ySize;
        uncollapsedCells = xSize * ySize;
        forcedCells = 0;
        palette = deserializePalette(pathToPaletteJson);
        grid = new WFCCell[xSize][];
        for (int x = 0; x < xSize; x++)
        {
            grid[x] = new WFCCell[ySize];
            for(int y = 0; y < ySize; y++)
            {
                grid[x][y] = new WFCCell(palette);
            }
        }
        ((int, int), float)[] input = { ((0, 0), grid[0][0].getEntropy()) };
        entropyHeap = new Heap(input, (int)(xSize * ySize * 1000));
        removalStack = new Stack<RemovalUpdate>();
    }
    private Palette deserializePalette(string pathToPaletteJson)
    {
        string jsonString = File.ReadAllText(pathToPaletteJson);
        PaletteData data = JsonUtility.FromJson<PaletteData>(jsonString);
        Palette palette = new Palette();
        palette.deserialize(data);
        palette.processTiles();
        return palette;
    }
    private (int,int) chooseNextCell()
    {
        while (!entropyHeap.isEmpty())
        {
            ((int, int),float) cell = entropyHeap.extractMin();
            if (!grid[cell.Item1.Item1][cell.Item1.Item2].isCollapsed)
            {
                return cell.Item1;
            }
        }
        //Debug.Log("chooseNextTile ran out of entries somehow");
        return (0, 0);
    }
    private void collapseCell((int,int) cell)
    {
        grid[cell.Item1][cell.Item2].isCollapsed = true;
        int tileIndex = grid[cell.Item1][cell.Item2].chooseTile();
        //Debug.Log("collapsed cell " + cell + " to tile " + tileIndex);
        for (int ii = 0; ii < palette.numberOfTiles; ii++)
        {
            if(ii != tileIndex)
            {
                grid[cell.Item1][cell.Item2].removeTile(ii);
                removalStack.Push(new RemovalUpdate(ii, cell));
            }
        }
        uncollapsedCells--;
    }
    private void forceCollapseCell((int,int) cell, int tileIndex)
    {
        grid[cell.Item1][cell.Item2].isCollapsed = true;
        grid[cell.Item1][cell.Item2].chosenTile = tileIndex;
        uncollapsedCells--;
        forcedCells++;
        //Debug.Log("force collapsed cell " + cell + " to tile " + tileIndex + " totalcount: " + forcedCells);
    }
    private bool propagate()
    {
        bool reset = false;
        while(removalStack.Count > 0)
        {
            RemovalUpdate update = removalStack.Pop();
            for(int dir = 0; dir < 4; dir++)
            {
                (int, int)? potentialNeighbor = getNeighbor(update.coordinate, dir, false);
                if(potentialNeighbor != null)
                {
                    (int, int) neighbor = ((int, int))potentialNeighbor;
                    for (int tileB = 0; tileB < palette.numberOfTiles; tileB++)
                    {
                        if (palette.adjacencyRules[update.index,tileB,dir]) //if tileB was enabled by the removed tile
                        {
                            grid[neighbor.Item1][neighbor.Item2].enablerCount[tileB][oppositeDirection(dir)]--; //decrement enabler count
                            if (grid[neighbor.Item1][neighbor.Item2].enablerCount[tileB][oppositeDirection(dir)] == 0) //if youre about to remove the tile
                            {
                                bool wasRemoved = false;
                                for(int enablerDirections = 0; enablerDirections < 4; enablerDirections++)
                                {
                                    if(enablerDirections != oppositeDirection(dir) && grid[neighbor.Item1][neighbor.Item2].enablerCount[tileB][enablerDirections] == 0) //if its already been removed before
                                    {
                                        wasRemoved = true;
                                    }
                                }
                                if (!wasRemoved) //if it wasnt removed
                                {
                                    grid[neighbor.Item1][neighbor.Item2].removeTile(tileB); //remove it 
                                    entropyHeap.insert((neighbor, grid[neighbor.Item1][neighbor.Item2].getEntropy()));
                                    if (grid[neighbor.Item1][neighbor.Item2].hasContradiction())
                                    {
                                        //Debug.Log("contradiction detected at " + neighbor);
                                        forceCollapseCell(neighbor, 0);
                                        //reset = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        return reset;
    }
    private void addNeighborsToHeap((int,int) point, int depth)
    {
        if(depth > 0)
        {
            for (int dir = 0; dir < 4; dir++)
            {
                (int, int)? potentialNeighbor = getNeighbor(point, dir, false);
                if (potentialNeighbor != null)
                {
                    (int, int) neighbor = ((int, int))potentialNeighbor;
                    if (!grid[neighbor.Item1][neighbor.Item2].isCollapsed)
                    {
                        entropyHeap.insert((neighbor, grid[neighbor.Item1][neighbor.Item2].getEntropy()));
                        addNeighborsToHeap(neighbor, depth - 1);
                    }
                }
            }
        }
    }
    public void generateWithResets(int maxAttempts)
    {
        int attempt = 1;
        while (generate() && attempt < maxAttempts)
        {
            //Debug.Log("attempt " + attempt);
            reset();
            attempt++;
        }
        if(attempt == maxAttempts)
        {
            //Debug.Log("max attempts reached, showing last attempt");
            int[] decodedGrid = decodeGrid();
            LevelData levelData = new LevelData((int)(xSize * ySize));
            for (int ii = 0; ii < ySize * xSize; ii++)
            {
                levelData.blockData[ii] = palette.tiles[decodedGrid[ii]].blocks[0, 0];
            }
            StreamWriter writer = new StreamWriter("D:/dev/orthocubes/gamedata/floorData.json");
            writer.Write(JsonUtility.ToJson(levelData));
            writer.Close();
        }
    }
    public bool generate()
    {
        //debugPrintEnables();
        while (uncollapsedCells > 0)
        {
            (int, int) cell = chooseNextCell();
            collapseCell(cell);
            if (propagate())
            {
                return true;
            }
            if(forcedCells > 20)
            {
                return true;
            }
            //addNeighborsToHeap(cell, 1);
        }
        int[] decodedGrid = decodeGrid();
        LevelData levelData = new LevelData((xSize * ySize));
        for(int ii = 0; ii < ySize * xSize; ii++)
        {
            levelData.blockData[ii] = palette.tiles[decodedGrid[ii]].blocks[0,0];
        }
        StreamWriter writer = new StreamWriter("D:/dev/orthocubes/gamedata/floorData.json");
        writer.Write(JsonUtility.ToJson(levelData));
        writer.Close();
        return false;
    }
    private void reset()
    {
        uncollapsedCells = xSize * ySize;
        forcedCells = 0;
        grid = new WFCCell[xSize][];
        ((int, int), float)[] input = new ((int, int), float)[xSize * ySize];
        for (int x = 0; x < xSize; x++)
        {
            grid[x] = new WFCCell[ySize];
            for (int y = 0; y < ySize; y++)
            {
                grid[x][y] = new WFCCell(palette);
                input[x + y * xSize] = ((x, y), grid[x][y].getEntropy());
            }
        }
        entropyHeap = new Heap(input, (int)(xSize * ySize * 1000));
        removalStack = new Stack<RemovalUpdate>();
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
                    if (y >= ySize)
                    {
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
    private void debugPrintEnables()
    {
        string message = "";
        for(int x = 0; x < xSize; x++)
        {
            for(int y = 0; y < ySize; y++)
            {
                message += "grid " + x + " " + y + "\n";
                message += grid[x][y].debugPrintEnablers();
            }
        }
        Debug.Log(message);
    }
    //private classes
    private class WFCCell
    {
        public bool isCollapsed;
        private bool[] possibilities;
        private int sumOfWeights;
        private float sumOfWeightLogWeights;
        private float entropyNoise;
        public int[][] enablerCount;
        public int chosenTile;

        Palette palette;

        public WFCCell(Palette palette)
        {
            chosenTile = 1;
            isCollapsed = false;
            possibilities = new bool[palette.numberOfTiles];
            this.palette = palette;
            enablerCount = new int[palette.numberOfTiles][];
            for(int ii = 0; ii < palette.numberOfTiles; ii++)
            {
                enablerCount[ii] = new int[4];
                for(int dir = 0; dir < 4; dir++)
                {
                    enablerCount[ii][dir] = palette.initialEnablerCount[ii][dir];
                }
            }
            sumOfWeights = 0;
            sumOfWeightLogWeights = 0;
            entropyNoise = Random.value / 100000;
            for(int ii = 0; ii < palette.numberOfTiles; ii++)
            {
                possibilities[ii] = true;
                sumOfWeights += palette.frequencies[ii];
                sumOfWeightLogWeights += Mathf.Log(palette.frequencies[ii]) * palette.frequencies[ii];
            }
        }
        public float getEntropy()
        {
            return Mathf.Log(sumOfWeights) - (sumOfWeightLogWeights / sumOfWeights) + entropyNoise;
        }
        public void removeTile(int tileIndex)
        {
            possibilities[tileIndex] = false;
            sumOfWeights -= palette.frequencies[tileIndex];
            sumOfWeightLogWeights -= Mathf.Log(palette.frequencies[tileIndex]) * palette.frequencies[tileIndex];
        }
        public int chooseTile()
        {
            int rng = (int)(Random.value * sumOfWeights);
            for (int ii = 0; ii < palette.numberOfTiles; ii++)
            {
                if (possibilities[ii])
                {
                    if(rng < palette.frequencies[ii])
                    {
                        chosenTile = (int)ii;
                        return chosenTile;
                    }
                    else
                    {
                        rng -= palette.frequencies[ii];
                    }
                }
            }
            //Debug.Log("chooseTile had some weight issues");
            return 0;
        }
        public bool hasContradiction()
        {
            if(sumOfWeights <= 0 && !isCollapsed)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public string debugPrintEnablers()
        {
            string message = "";
            for(int ii = 0; ii < palette.numberOfTiles; ii++)
            {
                for(int dir = 0; dir < 4; dir++)
                {
                    message += "\t" + ii + " dir: " + dir + " " + enablerCount[ii][dir] + " still possible: " + possibilities[ii] + "\n";
                }
            }
            return message;
        }
    }
    private class Tile
    {
        public BlockData[,] blocks;
        public Tile(int size)
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
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    writer.Write(blocks[x, y].blockPerm + "\t");
                }
                writer.Write("\n");
            }
            return writer.ToString();
        }
    }
    private class Palette
    {
        public PaletteData paletteData;
        public int xSize; //horizontal size of sample image
        public int ySize; //vertical size of sample image
        public bool rotations;
        public BlockData[,] sampleBlockArray; //the original sample "image", made 2d array

        public int numberOfTiles;
        public int tileSize; //ALWAYS 3, IM TIRED OF MATRIX STUFF, NO EXCEPTIONS
        public Tile[] tiles; //the set of tiles that make up the sample image
        public int[] frequencies; //the amount that each tile occurs in the sample
        public bool[,,] adjacencyRules; //whether each tile can exist 1 unit cardinal direction away from another tile without contradicting overlap
        public int[][] initialEnablerCount;

        private int ROTATION_MODIFIER = 4;

        //sets tiles, frequencies, adjacency rules
        public void processTiles()
        {
            tileSize = 3;
            numberOfTiles = rotations?(xSize * ySize * ROTATION_MODIFIER): (xSize * ySize);
            initialEnablerCount = new int[numberOfTiles][];
            for(int ii = 0; ii < numberOfTiles; ii++)
            {
                initialEnablerCount[ii] = new int[4];
                int[] temp = { 0, 0, 0, 0 };
                initialEnablerCount[ii] = temp;
            }
            getTiles();
            getFrequencies();
            getAdjacencyRules();
        }
        private void getTiles()
        {
            tiles = new Tile[numberOfTiles];
            int index = 0;
            for (int y = 0; y < ySize; y++)
            {
                for (int x = 0; x < xSize; x++)
                {
                    tiles[index] = getTileAtPosition(x, y);
                    index++;
                    if (rotations)
                    {
                        for (int rot = 1; rot < 4; rot++)
                        {
                            tiles[index] = getRotatedTile(tiles[index - rot], rot);
                            index++;
                        }
                    }
                }
            }
        }
        private Tile getTileAtPosition(int x, int y)
        {
            Tile tile = new Tile(tileSize);
            for (int ii = 0; ii < tileSize; ii++)
            {
                for (int jj = 0; jj < tileSize; jj++)
                {
                    tile.setBlock(ii, jj, sampleBlockArray[(x + ii) % xSize, (y + jj) % ySize]);
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
            frequencies = new int[numberOfTiles];
            for (int tileA = 0; tileA < numberOfTiles; tileA++)
            {
                for (int tileB = 0; tileB < numberOfTiles; tileB++)
                {
                    if (tileEquals(tiles[tileA], tiles[tileB]))
                    {
                        frequencies[tileA]++; //end result is 1 + the amount of duplicate tiles for each tile
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
        public Tile getRotatedTile(Tile tile, int rotations)
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
            for (int x = 0; x < tileSize; x++)
            {
                for (int y = 0; y < tileSize; y++)
                {
                    (int, int) rot = tileRotationArray[x, y];
                    rotatedTile.setBlock(rot.Item1, rot.Item2, tile.blocks[x, y]);
                    rotatedTile.blocks[rot.Item1, rot.Item2].blockPerm = BlockScript.getRotation(rotatedTile.blocks[rot.Item1, rot.Item2].blockPerm, 1);
                    rotatedTile.blocks[rot.Item1, rot.Item2].walkableData = BlockScript.getWalkableRotation(rotatedTile.blocks[rot.Item1, rot.Item2].walkableData, 1);
                }
            }
            return rotatedTile;
        }
        public void getAdjacencyRules()
        {
            adjacencyRules = new bool[numberOfTiles, numberOfTiles, 4]; //tileindex,  tileindex, direction
            for (int tileA = 0; tileA < numberOfTiles; tileA++)
            {
                for (int tileB = 0; tileB < numberOfTiles; tileB++)
                {
                    for (int direction = 0; direction < 4; direction++)
                    {
                        adjacencyRules[tileA,  tileB,  direction] = compatible(tiles[tileA], tiles[tileB], direction);
                        if(adjacencyRules[tileA, tileB, direction])
                        {
                            initialEnablerCount[tileA][direction]++;
                        }
                    }
                }
            }
        }
        public bool compatible(Tile tileA, Tile tileB, int direction) //0 is up, 1 is right, 2 is down, 3 is left
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
            for (int x = 0; x < tileSize; x++)
            {
                for (int y = 0; y < tileSize - 1; y++)
                {
                    if (tileA.blocks[x, y].blockPerm != tileB.blocks[x, y + 1].blockPerm)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        private bool compatibleRight(Tile tileA, Tile tileB)
        {
            for (int x = 1; x < tileSize; x++)
            {
                for (int y = 0; y < tileSize; y++)
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
            for (int x = 0; x < tileSize; x++)
            {
                for (int y = 1; y < tileSize; y++)
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
            for (int x = 0; x < tileSize - 1; x++)
            {
                for (int y = 0; y < tileSize; y++)
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
            this.xSize = (int)data.xSize;
            this.ySize = (int)data.ySize;
            sampleBlockArray = new BlockData[xSize, ySize];
            int index = 0;
            for (int y = 0; y < ySize; y++)
            {
                for (int x = 0; x < xSize; x++)
                {
                    sampleBlockArray[x, y] = data.sampleArray[index];
                    index++;
                }
            }
        }
        public void debugPrintRules()
        {
            for(int tileA = 0; tileA < numberOfTiles; tileA++)
            {
                for(int tileB = 0; tileB < numberOfTiles; tileB++)
                {
                    for(int dir = 0; dir < 4; dir++)
                    {
                        if (adjacencyRules[tileA,tileB,dir])
                        {
                            Debug.Log(tileA + " " + tileB + " " + dir);
                        }
                    }
                }
            }
        }
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
            if (left < heapSize && heapContents[left].Item2 < heapContents[index].Item2)
            {
                smallest = left;
            }
            if (right < heapSize && heapContents[right].Item2 < heapContents[smallest].Item2)
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
                heapify((int)ii);
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
                while (tempIndex > 0 && heapContents[getParent(tempIndex)].Item2 > heapContents[tempIndex].Item2)
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
            decreaseKey((int)index, item.Item2);
        }
        public void debugPrintContents()
        {
            Debug.Log("heap contents");
            for(int ii = 0; ii < heapSize; ii++)
            {
                Debug.Log(heapContents[ii]);
            }
        }
    }
    private class RemovalUpdate
    {
        public int index;
        public (int, int) coordinate;

        public RemovalUpdate(int index, (int, int) coordinate)
        {
            this.index = index;
            this.coordinate = coordinate;
        }
        public string toString()
        {
            return "index: " + index + " coord: " + coordinate;
        }
    }
    [System.Serializable]
    public class PaletteData
    {
        public int xSize;
        public int ySize;
        public bool rotations;
        public BlockData[] sampleArray;
    }
}
