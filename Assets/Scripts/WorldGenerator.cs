using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public GameObject LandTile;
    //a tile is a square: x=y
    public float tileSize;
    //number of tiles 
    public int width;
    public int height;
    public string seed;
    public bool useRandomSeed;
    [Range(0, 100)]
    public int randomFillPercent;

    MapGenerator m_mapGenerator;
    int[,] m_map;

    // Start is called before the first frame update
    void Start()
    {
        BoxCollider2D LandTileCollider = LandTile.GetComponent<BoxCollider2D>();
        tileSize = LandTileCollider.size.x;
        initializeMap();
    }

    private void initializeMap()
    {
        m_mapGenerator = new MapGenerator(tileSize, width, height, seed, useRandomSeed, randomFillPercent);
        m_mapGenerator.GenerateMap(true);
        m_map = m_mapGenerator.GetMap();
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
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    //Gizmos.color = (map[x, y] == 1) ? Color.black : Color.white;
                    if (m_map[x, y] == 1)
                    {
                        Vector2 pos = new Vector3(-width / 2 + x + tileSize, -height / 2 + y + tileSize);
                        GameObject newTile = Instantiate(LandTile);
                        newTile.transform.SetParent(transform);
                        newTile.transform.position = pos;
                        // Gizmos.DrawCube(pos, Vector3.one);
                    }

                }
            }
        }
    }
}
