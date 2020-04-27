using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameResources;
using Random = UnityEngine.Random;

public class MapGenerator : MonoBehaviour
{
    public static MapGenerator m_Instance;
    int[,] m_landTypeMap;
    //int[,] map;

    //a tile is a square: x=y
    float tileSize;

    //number of tiles 
    int width;
    int height;

    //final map size
    int finalWidth;
    int finalHeight;

    string seed;
    bool useRandomSeed;
    int smoothRatio;

    //randomFillPercent1 + randomFillPercent2 < 100
    int randomFillPercent1;
    int randomFillPercent2;

    //10, 5
    int centerEmptyWidth = 10;
    int centerEmptyHeight = 5;

    int EdgeWidth = 5;


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

    }

    public int[,] GenerateCombinedMaps(int widthCount, int centerIndex)
    {
        if (useRandomSeed)
        {
            float seed = Time.time;
        }
        finalWidth = widthCount * this.width;
        finalHeight = widthCount * this.height;
        //Generate 9 maps , with 1 is Center
        int[,] finalMap = new int[finalWidth, finalHeight];

        m_landTypeMap = CreateLandTypeMap(widthCount, centerIndex);

        for(int i = 0; i < widthCount; i++)
        {
            for(int j = 0; j < widthCount; j++)
            {
                int[,] newMap;
                //if (i == j && i == centerIndex - 1)
                //{
                //    seed = seed + i * 10 + j;
                //    newMap = GenerateMap(true, seed.ToString());
                //} else
                //{
                //    seed = seed + i * 10 + j;
                //    newMap = GenerateMap(false, seed.ToString());
                //}
                seed = seed + i * 10 + j;
                if (i == j && i == centerIndex - 1)
                {
                    newMap = GenerateMap(true, m_landTypeMap[i, j], seed.ToString());
                } else
                {
                    newMap = GenerateMap(false, m_landTypeMap[i, j], seed.ToString());
                }
                
                
                for (int x = 0; x < width; x++)
                {
                    for(int y = 0; y < height; y++)
                    {
                        finalMap[i * this.width + x, j * this.height + y] = newMap[x, y];
                    }
                    
                }
                
            }
            
        }

        for (int i = 0; i < finalWidth; i++)
        {
            for (int j = 0; j < finalHeight; j++)
            {
                if(i < EdgeWidth || i > finalWidth - EdgeWidth || j < EdgeWidth || j > finalHeight - EdgeWidth)
                {
                    finalMap[i, j] = (int)TileType.UnCrackable;
                }
            }
        }


        for (int i = 0; i < 5; i++)
        {
            SmoothMap(finalMap);
        }
        return finalMap;
    }

    public int[,] CreateLandTypeMap(int widthCount, int centerIndex)
    {
        int[,] landTypeMap = new int[widthCount, widthCount];

        int glandCount = 0;
        int slandCount = 0;
        int rlandCount = 0;

        int randInt = 0;
        while (slandCount == 0 || rlandCount == 0)
        {
            glandCount = 0;
            slandCount = 0;
            rlandCount = 0;
            for (int i = 0; i < widthCount; i++)
            {
                for (int j = 0; j < widthCount; j++)
                {
                    if(i == j && i == centerIndex - 1)
                    {
                        landTypeMap[i, j] = (int)LandType.GREENLAND;
                        glandCount++;
                    } else
                    {
                        randInt = Random.Range(1, 4);
                        if(randInt == 1)
                        {
                            landTypeMap[i, j] = (int)LandType.GREENLAND;
                            glandCount++;
                        } else if(randInt == 2)
                        {
                            landTypeMap[i, j] = (int)LandType.SANDLAND;
                            slandCount++;
                        } else
                        {
                            landTypeMap[i, j] = (int)LandType.RUINLAND;
                            rlandCount++;
                        }
                    }

                }
            }
        }
        

        return landTypeMap;
    }


    public int[,] getLandTypeMap()
    {
        return m_landTypeMap;
    }

    public int[,] GenerateMap(bool isCenter, int landType, string seed)
    {
        if (isCenter)
        {
            return GenerateMapHelper(true, (int)TileType.DIRT, (int)TileType.STONE, seed);
        }
        else
        {
            if (landType == (int)LandType.SANDLAND)
            {
                return GenerateMapHelper(false, (int)TileType.SAND, (int)TileType.IRON, seed);
            }
            else if (landType == (int)LandType.RUINLAND)
            {
                return GenerateMapHelper(false, (int)TileType.STONE, (int)TileType.SPARE, seed);
            }
            else if (landType == (int)LandType.GREENLAND)
            {
                return GenerateMapHelper(false, (int)TileType.DIRT, (int)TileType.STONE, seed);
            }
        }
        Debug.Log("Wrong Land Type!!!!!!!");
        return null;
    }

    private int[,] GenerateMapHelper(bool isCenter, int resource1, int resource2, string seed)
    {
        int[,] map = new int[width, height];
        //random fill map with three ints
        RandomFillMap(map, resource1, resource2, seed);

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

        //for (int i = 0; i < 5; i++)
        //{
        //    SmoothMap(map, resource1, resource2);
        //}
        return map;
    }

    //random Fill map with 0, resource1, resource2
    private void RandomFillMap(int[,] map, int resource1, int resource2, string seed)
    {
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

    //private void SmoothMap(int[,] map, int resource1, int resource2)
    //{
    //    for(int x = 0; x < width; x++)
    //    {
    //        for(int y = 0; y < height; y++)
    //        {
    //            //int neighbourWallTilesR1 = GetSurroundingWallCount(map, x, y, resource1);
    //            //int neighbourWallTilesR2 = GetSurroundingWallCount(map, x, y, resource2);
    //            int neighbourWallTilesEmpty = GetSurroundingWallCount(map, x, y, 0);
    //            //int totalWallCount = neighbourWallTilesR1 + neighbourWallTilesR2 + neighbourWallTilesEmpty;

    //           // Debug.Log("Total: " + totalWallCount + "; Empty: " + neighbourWallTilesEmpty + "; R1: " + neighbourWallTilesR1 + "; R2: " + neighbourWallTilesR2);
    //            Debug.Log("Empty:" + neighbourWallTilesEmpty);

    //            if(neighbourWallTilesEmpty <= 4)
    //            {
    //                map[x, y] = resource1;
    //            }
    //            else if(neighbourWallTilesEmpty >= 5)
    //            {
    //                map[x, y] = 0;
    //            }

    //            //if (neighbourWallTilesR1 + neighbourWallTilesR2 >= smoothRatio / 8)
    //            //{
    //            //    map[x, y] = resource1;
    //            //    //map[x, y] = resource2;
    //            //}
    //            //else if(neighbourWallTilesR1 + neighbourWallTilesR2 < smoothRatio / 8)
    //            //{
    //            //    map[x, y] = 0;
    //            //}

    //            //if (neighbourWallTilesR1 + neighbourWallTilesR2 < 2*smoothRatio)
    //            //    map[x, y] = 0;
    //        }
    //    }
    //}
    void SmoothMap(int[,] map)
    {
        SurroundingInfo[,] infoMap = GetSurroundingWallCount(map, (int)TileType.DIRT, (int)TileType.STONE, (int)TileType.SAND, (int)TileType.IRON, (int)TileType.SPARE);

        // if(i < EdgeWidth || i > finalWidth - EdgeWidth || j < EdgeWidth || j > finalHeight - EdgeWidth)
        for (int x = EdgeWidth; x <= finalWidth - EdgeWidth; x++)
        {
            for (int y = EdgeWidth; y <= finalHeight - EdgeWidth; y++)
            {
                int neighbourWallTiles = infoMap[x,y].totalWallCount;
                int[] list = { infoMap[x,y].R1Count, infoMap[x, y].R2Count, infoMap[x, y].R3Count, infoMap[x, y].R4Count, infoMap[x, y].R5Count };
                
                if (neighbourWallTiles > 4)
                {
                    //if (list[4] > 0 && list[4] >= list[3])
                    //{
                    //    map[x, y] = list[4];
                    //}
                    //else
                    //{
                    //    map[x, y] = list[3];
                    //}
                    int maxIndex = findMaxIndex(list);
                    if (map[x,y] == 0 || list[maxIndex] - list[map[x,y]-1] >= 1)
                        map[x, y] = maxIndex + 1;
                }
                else if (neighbourWallTiles < 4)
                {
                    map[x, y] = 0;
                }
                   

            }
        }
    }

    int findMaxIndex(int[] list)
    {
        int maxIndex = 0;
        int maxVal = 0;
        for(int i = 0; i < list.Length; i++)
        {
            if(list[i] > maxVal)
            {
                maxVal = list[i];
                maxIndex = i;
            }
        }
        return maxIndex;
    }

    SurroundingInfo[,] GetSurroundingWallCount(int[,] map, int resource1, int resource2, int resource3, int resource4, int resource5)
    {

        int mapWidth = map.GetLength(0) - EdgeWidth;
        int mapHeight = map.GetLength(1) - EdgeWidth;


        SurroundingInfo[,] info = new SurroundingInfo[map.GetLength(0), map.GetLength(1)];

        for (int gridX = EdgeWidth; gridX <= mapWidth; gridX++)
        {
            for (int gridY = EdgeWidth; gridY <= mapHeight; gridY++)
            {
                for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
                {
                    for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
                    {
                        if (neighbourX >= EdgeWidth && neighbourX <= mapWidth && neighbourY >= EdgeWidth && neighbourY <= mapHeight)
                        {
                            if (neighbourX != gridX || neighbourY != gridY)
                            {
                                if (map[neighbourX, neighbourY] != 0 && map[neighbourX, neighbourY] != (int)TileType.UnCrackable)
                                {
                                    info[gridX, gridY].totalWallCount++;
                                    if (map[neighbourX, neighbourY] == resource1)
                                    {
                                        info[gridX, gridY].R1Count++;
                                    }
                                    else if (map[neighbourX, neighbourY] == resource2)
                                    {
                                        info[gridX, gridY].R2Count++;
                                    }
                                    else if (map[neighbourX, neighbourY] == resource3)
                                    {
                                        info[gridX, gridY].R3Count++;
                                    }
                                    else if (map[neighbourX, neighbourY] == resource4)
                                    {
                                        info[gridX, gridY].R4Count++;
                                    }
                                    else if (map[neighbourX, neighbourY] == resource5)
                                    {
                                        info[gridX, gridY].R5Count++;
                                    }
                                }
                            }
                        }
                        else
                        {
                            info[gridX, gridY].totalWallCount++;
                        }
                    }
                }
            }
        }

        return info;
    }
    //int GetSurroundingWallCount(int[,] map, int gridX, int gridY)
    //{
    //    int wallCount = 0;
    //    for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
    //    {
    //        for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
    //        {
    //            if (neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height)
    //            {
    //                if (neighbourX != gridX || neighbourY != gridY)
    //                {
    //                    if(map[neighbourX, neighbourY] != 0)
    //                    {
    //                        wallCount++;
    //                    }
    //                }
    //            }
    //            else
    //            {
    //                wallCount++;
    //            }
    //        }
    //    }

    //    return wallCount;
    //}

    struct SurroundingInfo
    {
        public int R1Count;
        public int R2Count;
        public int R3Count;
        public int R4Count;
        public int R5Count;
        public int totalWallCount; 
    }

    //private int GetSurroundingWallCount(int[,] map, int gridX, int gridY, int resource)
    //{
    //    int wallCount = 0;
    //    for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
    //    {
    //        for(int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
    //        {
    //            if(neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height)
    //            {
    //                if (neighbourX != gridX || neighbourY != gridY)
    //                {
    //                    //wallCount += map[neighbourX, neighbourY];
    //                    if(map[neighbourX, neighbourY] == resource)
    //                    {
    //                        wallCount++;
    //                    }
    //                }
    //            }
    //            //else
    //            //{
    //            //    //edges
    //            //    //wallCount++;
    //            //}

    //        }
    //    }
    //    return wallCount;
    //}

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
