using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameResources;

public class WorldGenerator : MonoBehaviour
{
    public GameObject LandTile_DIRT;
    public GameObject LandTile_STONE;
    public GameObject LandTile_SAND;
    public GameObject LandTile_IRON;
    public GameObject LandTile_SPARE;
    public GameObject LandTile_Empty;

    public GameObject FruitPlant;
    public GameObject GrassAnimal;


    //a tile is a square: x=y
    public float tileSize;
    //number of tiles 
    public int width;
    public int height;
    public int finalMapWidthCount;

    float totalWidth;
    float totalHeight;

    public string seed;
    public bool useRandomSeed;
    [Range(1,8)]
    public int smoothRatio = 4;
    [Range(0, 100)]
    public int randomFillPercent1;
    [Range(0, 100)]
    public int randomFillPercent2;

    MapGenerator m_mapGenerator;
    [HideInInspector]
    public int[,] m_map;

    // Start is called before the first frame update
    public void GenerateLands()
    {
        BoxCollider2D LandTileCollider = LandTile_DIRT.GetComponent<BoxCollider2D>();
        tileSize = LandTileCollider.size.x;
        totalWidth = tileSize * (width * finalMapWidthCount);
        totalHeight = tileSize * (height * finalMapWidthCount);
        initializeMap();
    }

    public void GenerateCreatures()
    {
        if (m_map != null)
        {
            for (int x = 0; x < width * finalMapWidthCount; x++)
            {
                //height* finalMapWidthCount
                for (int y = 0; y < height * finalMapWidthCount; y++)
                {
                    //Gizmos.color = (map[x, y] == 1) ? Color.black : Color.white;

                    FillWithTileType(m_map[x, y], x, y);

                }
            }
        }
    }

    private void initializeMap()
    {
        m_mapGenerator = new MapGenerator(tileSize, width, height, seed, useRandomSeed, smoothRatio, randomFillPercent1, randomFillPercent2);
        //m_mapGenerator.GenerateMap(true);
        m_map = m_mapGenerator.GenerateCombinedMaps(finalMapWidthCount, finalMapWidthCount / 2 + 1);


        FillWithMesh();
    }


    public void FillWithMesh()
    {
        if (m_map != null)
        {
            for (int x = 0; x < width * finalMapWidthCount; x++)
            {
                //height* finalMapWidthCount
                for (int y = 0; y < height * finalMapWidthCount; y++)
                {
                    //Gizmos.color = (map[x, y] == 1) ? Color.black : Color.white;

                    FillWithTileType(m_map[x, y], x, y);

                }
            }
        }
    }

    public void FillWithTileType(int TileTypeVal, int x, int y)
    {
        if (TileTypeVal == (int)TileType.DIRT)
        {
            FillWith(LandTile_DIRT, x, y);
        }
        else if (TileTypeVal == (int)TileType.STONE)
        {
            FillWith(LandTile_STONE, x, y);
        }
        else if (TileTypeVal == (int)TileType.SAND)
        {
            FillWith(LandTile_SAND, x, y);
        }
        else if (TileTypeVal == (int)TileType.IRON)
        {
            FillWith(LandTile_IRON, x, y);
        }
        else if (TileTypeVal == (int)TileType.SPARE)
        {
            FillWith(LandTile_SPARE, x, y);
        }
        else if (TileTypeVal == 0)
        {
            FillWith(LandTile_Empty, x, y);
        }
    }

    void FillWith(GameObject obj, int x, int y)
    {
        
        //bug here, wrong index at the top level: (0, 0) - (0, ...)
        Vector2 pos = new Vector3((-totalWidth/2.0f) + (tileSize/2.0f) + x * tileSize, (totalHeight / 2.0f) - (tileSize / 2.0f) - tileSize * y);
        Debug.Log(pos);
        GameObject newTile = Instantiate(obj);
        newTile.transform.SetParent(transform);
        newTile.transform.position = pos;

        LandBrick tile = newTile.GetComponent<LandBrick>();
        EmptyTile EmptyTile = newTile.GetComponent<EmptyTile>();

        if (tile != null)
        {
            tile.index = new Vector2Int(x, y);
            Debug.Log("index: " + x + ", " + y);
        }
        else
        {
            EmptyTile.index = new Vector2Int(x, y);
            Debug.Log("index: " + x + ", " + y);
        }

    }

    public int[,] getInitialMap() {
        return m_map;
    }
}
