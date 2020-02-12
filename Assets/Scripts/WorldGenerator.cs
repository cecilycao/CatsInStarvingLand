using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public GameObject LandTile_DIRT;
    public GameObject LandTile_STONE;
    public GameObject LandTile_SAND;
    public GameObject LandTile_IRON;
    public GameObject LandTile_SPARE;


    //a tile is a square: x=y
    public float tileSize;
    //number of tiles 
    public int width;
    public int height;
    public int finalMapWidthCount;

    public string seed;
    public bool useRandomSeed;
    [Range(1,8)]
    public int smoothRatio = 4;
    [Range(0, 100)]
    public int randomFillPercent1;
    [Range(0, 100)]
    public int randomFillPercent2;

    MapGenerator m_mapGenerator;
    int[,] m_map;

    // Start is called before the first frame update
    void Start()
    {
        BoxCollider2D LandTileCollider = LandTile_DIRT.GetComponent<BoxCollider2D>();
        tileSize = LandTileCollider.size.x;
        initializeMap();
    }

    private void initializeMap()
    {
        m_mapGenerator = new MapGenerator(tileSize, width, height, seed, useRandomSeed, smoothRatio, randomFillPercent1, randomFillPercent2);
        //m_mapGenerator.GenerateMap(true);
        m_map = m_mapGenerator.GenerateCombinedMaps(finalMapWidthCount, finalMapWidthCount / 2 + 1);


        FillWithMesh();
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetMouseButtonDown(0))
        //{
        //    foreach(Transform child in transform)
        //    {
        //        Destroy(child.gameObject);
        //    }
        //    initializeMap();
        //}
    }

    public void FillWithMesh()
    {
        if (m_map != null)
        {
            for (int x = 0; x < width * finalMapWidthCount; x++)
            {
                for (int y = 0; y < height * finalMapWidthCount; y++)
                {
                    //Gizmos.color = (map[x, y] == 1) ? Color.black : Color.white;

                    if (m_map[x, y] == (int)Resources.TileType.DIRT)
                    {
                        FillWith(LandTile_DIRT, x, y);
                    }
                    else if(m_map[x, y] == (int)Resources.TileType.STONE)
                    {
                        FillWith(LandTile_STONE, x, y);
                    }
                    else if (m_map[x, y] == (int)Resources.TileType.SAND)
                    {
                        FillWith(LandTile_SAND, x, y);
                    }
                    else if (m_map[x, y] == (int)Resources.TileType.IRON)
                    {
                        FillWith(LandTile_IRON, x, y);
                    }
                    else if (m_map[x, y] == (int)Resources.TileType.SPARE)
                    {
                        FillWith(LandTile_SPARE, x, y);
                    }

                }
            }
        }
    }

    void FillWith(GameObject obj, int x, int y)
    {
        Vector2 pos = new Vector3(-width / 2 + x + tileSize, -height / 2 + y + tileSize);
        GameObject newTile = Instantiate(obj);
        newTile.transform.SetParent(transform);
        newTile.transform.position = pos;
        // Gizmos.DrawCube(pos, Vector3.one);
    }
}
