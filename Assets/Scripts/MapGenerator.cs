using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Resources;
using Random = UnityEngine.Random;

public class MapGenerator : MonoBehaviour
{
    public static MapGenerator m_Instance;
    //int[,] map;

    //a tile is a square: x=y
    float tileSize;

    //number of tiles 
    int width;
    int height;

    string seed;
    bool useRandomSeed;
    int smoothRatio;

    //randomFillPercent1 + randomFillPercent2 < 100
    int randomFillPercent1;
    int randomFillPercent2;

    int centerEmptyWidth = 10;
    int centerEmptyHeight = 5;

    int resource1;
    int resource2;


    public static MapGenerator Instance
    {
        get
        {
            if (m_Instance == null)
                m_Instance = FindObjectOfType<MapGenerator>();
            return m_Instance;
        }
    } 

    public MapGenerator(float tileSize, int width, int height, string seed, bool useRandomSeed, int smoothRatio, int randomFillPercent1, int randomFillPercent2)
    {
        this.tileSize = tileSize;
        this.width = width;
        this.height = height;
        this.seed = seed;
        this.useRandomSeed = useRandomSeed;
        this.smoothRatio = smoothRatio;

        if(randomFillPercent1 + randomFillPercent2 > 100)
        {
            Debug.Log("wrong random fill percent entered.");
        }
        this.randomFillPercent1 = randomFillPercent1;
        this.randomFillPercent2 = randomFillPercent2;

        this.resource1 = (int)resource1;
        this.resource2 = (int)resource2;
        //FillWithMesh();
    }

    public int[,] GenerateCombinedMaps(int widthCount, int centerIndex)
    {
        //Generate 9 maps , with 1 is Center
        int[,] finalMap = new int[widthCount * this.width, widthCount * this.height];

        for(int i = 0; i < widthCount; i++)
        {
            for(int j = 0; j < widthCount; j++)
            {
                int[,] newMap;
                if (i == j && i == centerIndex - 1)
                {
                    newMap = GenerateMap(true);
                } else
                {
                    newMap = GenerateMap(false);
                }
                for(int x = 0; x < width; x++)
                {
                    for(int y = 0; y < height; y++)
                    {
                        finalMap[i * this.width + x, j * this.height + y] = newMap[x, y];
                    }
                    
                }
                
            }
            
        }

        return finalMap;
    }



    public int[,] GenerateMap(bool isCenter)
    {
        int randInt = Random.Range(1, 4);
        if (isCenter)
        {
            return GenerateMapHelper(true, (int)TileType.DIRT, (int)TileType.STONE);
        }
        else
        {
            //1,2,3
            
            if(randInt == 1)
            {
                return GenerateMapHelper(true, (int)TileType.SAND, (int)TileType.IRON);
            } else if(randInt == 2)
            {
                return GenerateMapHelper(true, (int)TileType.STONE, (int)TileType.SPARE);
            } else
            {
                return GenerateMapHelper(true, (int)TileType.DIRT, (int)TileType.STONE);
            }

        }
    }

    private int[,] GenerateMapHelper(bool isCenter, int resource1, int resource2)
    {
        int[,] map = new int[width, height];
        //random fill map with three ints
        RandomFillMap(map, resource1, resource2);

        //if this map piece is the center piece, make a cave in the center
        if (isCenter)
        {
            for (int j = 0; j < centerEmptyHeight; j++)
            {
                for (int i = 0; i < centerEmptyWidth; i++)
                {
                    map[(width - centerEmptyWidth) / 2 + i, (height - centerEmptyHeight) / 2 + j] = 0;
                }
            }
        }

        for (int i = 0; i < 5; i++)
        {
            SmoothMap(map);
        }
        return map;
    }

    //random Fill map with 0, resource1, resource2
    private void RandomFillMap(int[,] map, int resource1, int resource2)
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
                    //resource1 || resource2
                    //map[x, y] = 1;
                    map[x, y] = (pseudoRandom.Next(0, 100) < randomFillPercent1*100/(randomFillPercent1+randomFillPercent2)) ? resource1 : resource2;
                }
                else
                {
                    int randomInt = pseudoRandom.Next(0, 100);
                    if ( randomInt > (randomFillPercent1 + randomFillPercent2))
                    {
                        map[x, y] = 0;
                    } else if (randomInt < randomFillPercent1)
                    {
                        map[x, y] = resource1;
                    } else
                    {
                        map[x, y] = resource2;
                    }
                    //map[x, y] = (pseudoRandom.Next(0, 100) < randomFillPercent) ? 1 : 0;
                }
            }
        }
    }

    private void SmoothMap(int[,] map)
    {
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                int neighbourWallTilesR1 = GetSurroundingWallCount(map, x, y, resource1);
                int neighbourWallTilesR2 = GetSurroundingWallCount(map, x, y, resource2);
                int neighbourWallTilesEmpty = GetSurroundingWallCount(map, x, y, 0);
                if (neighbourWallTilesR1 + neighbourWallTilesR2 + neighbourWallTilesEmpty != 8)
                {
                    Debug.Log("Neighbour Wall count is wrong!");
                }
                if(neighbourWallTilesEmpty > smoothRatio)
                    map[x, y] = 0;
               
                if (neighbourWallTilesR1 > smoothRatio)
                    map[x, y] = resource1;
                if (neighbourWallTilesR2 > smoothRatio)
                    map[x, y] = resource2;
                //if (neighbourWallTilesR1 + neighbourWallTilesR2 < 2*smoothRatio)
                //    map[x, y] = 0;
            }
        }
    }

    private int GetSurroundingWallCount(int[,] map, int gridX, int gridY, int resource)
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
                        //wallCount += map[neighbourX, neighbourY];
                        if(map[neighbourX, neighbourY] == resource)
                        {
                            wallCount++;
                        }
                    }
                }
                else
                {
                    //edges
                    //wallCount++;
                }

            }
        }
        return wallCount;
    }

    //public int[,] GetMap()
    //{
    //    return map;
    //}

    
    //private void OnDrawGizmos()
    //{
    //    if(map != null)
    //    {
    //        for(int x = 0; x < width; x++)
    //        {
    //            for(int y = 0; y < height; y++)
    //            {
    //                Gizmos.color = (map[x, y] == 1) ? Color.black : Color.white;
    //                Vector3 pos = new Vector3(-width / 2 + x + .5f, -height / 2 + y + 0.5f, 0);
    //                //Instantiate(LandTile);
    //                //LandTile.transform.position = pos;
    //                Gizmos.DrawCube(pos,Vector3.one);
    //            }
    //        }
    //    }
    //}

    //private void printMap()
    //{
    //    if (map != null)
    //    {
    //        for (int y = 0; y < height; y++)
    //        {
    //            for (int x = 0; x < width; x++)
    //            {
    //                Debug.Log(map[x, y] + " ");
    //            }
    //            Debug.Log("\n");
    //        }
    //    }
    //}

    
}
