using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public static MapGenerator m_Instance;
    int[,] map;

    //a tile is a square: x=y
    float tileSize;

    //number of tiles 
    int width;
    int height;

    string seed;
    bool useRandomSeed;

    [Range(0, 100)]
    int randomFillPercent;

    int centerEmptyWidth = 10;
    int centerEmptyHeight = 5;

    public static MapGenerator Instance
    {
        get
        {
            if (m_Instance == null)
                m_Instance = FindObjectOfType<MapGenerator>();
            return m_Instance;
        }
    }

    public MapGenerator(float tileSize, int width, int height, string seed, bool useRandomSeed, int randomFillPercent)
    {
        this.tileSize = tileSize;
        this.width = width;
        this.height = height;
        this.seed = seed;
        this.useRandomSeed = useRandomSeed;
        this.randomFillPercent = randomFillPercent;
        //FillWithMesh();
    }


    public void GenerateMap(bool isCenter)
    {
        map = new int[width, height];
        RandomFillMap();
        if (isCenter)
        {
            for (int j = 0; j < centerEmptyHeight; j++) {
                for (int i = 0; i < centerEmptyWidth; i++)
                {
                    map[(width - centerEmptyWidth) / 2 + i, (height - centerEmptyHeight)/2 + j] = 0;
                }
            }
        }

        for (int i = 0; i < 5; i++)
        {
            SmoothMap();
        }
    }

    private void RandomFillMap()
    {
        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }

        System.Random pseudoRandom = new System.Random(seed.GetHashCode());

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    map[x, y] = 1;
                }
                else
                {
                    map[x, y] = (pseudoRandom.Next(0, 100) < randomFillPercent) ? 1 : 0;
                }
            }
        }
    }

    private void SmoothMap()
    {
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                int neighbourWallTiles = GetSurroundingWallCount(x, y);

                if (neighbourWallTiles > 4)
                    map[x, y] = 1;
                else if (neighbourWallTiles < 4)
                    map[x, y] = 0;
            }
        }
    }

    private int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for(int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if(neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height)
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        wallCount += map[neighbourX, neighbourY];
                    }
                }
                else
                {
                    wallCount++;
                }

            }
        }
        return wallCount;
    }

    public int[,] GetMap()
    {
        return map;
    }

    
    private void OnDrawGizmos()
    {
        if(map != null)
        {
            for(int x = 0; x < width; x++)
            {
                for(int y = 0; y < height; y++)
                {
                    Gizmos.color = (map[x, y] == 1) ? Color.black : Color.white;
                    Vector3 pos = new Vector3(-width / 2 + x + .5f, -height / 2 + y + 0.5f, 0);
                    //Instantiate(LandTile);
                    //LandTile.transform.position = pos;
                    Gizmos.DrawCube(pos,Vector3.one);
                }
            }
        }
    }

    
}
