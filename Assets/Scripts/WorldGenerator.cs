﻿using System;
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

    public GameObject GreenLandZone;
    public GameObject SandLandZone;
    public GameObject RuinLandZone;
    public GameObject LandZoneType;


    //a tile is a square: x=y
    public float tileSize;
    //number of tiles 
    public int width;
    public int height;
    public int finalMapWidthCount;

    float totalWidth;
    float totalHeight;

    //for landtile
    public string seed;
    public bool useRandomSeed;
    [Range(1,8)]
    public int smoothRatio = 4;
    [Range(0, 100)]
    public int randomFillPercent1;
    [Range(0, 100)]
    public int randomFillPercent2;

    //for plants and animals.etc
    public int plantsFillPercent;
    public int animalFillPercent;

    MapGenerator m_mapGenerator;
    [HideInInspector]
    public int[,] m_map;
    public int[,] m_landTypeMap;

    // Start is called before the first frame update
    public void GenerateWorld()
    {
        BoxCollider2D LandTileCollider = LandTile_DIRT.GetComponent<BoxCollider2D>();
        tileSize = LandTileCollider.size.x;
        totalWidth = tileSize * (width * finalMapWidthCount);
        totalHeight = tileSize * (height * finalMapWidthCount);
       
        initializeMap();
        FillWithMesh();
        FillWithLandTypeZone();
    }

    //generate a matrix of map, with val indicates what kind of ground tile it is
    private void initializeMap()
    {
        m_mapGenerator = new MapGenerator(tileSize, width, height, seed, useRandomSeed, smoothRatio, randomFillPercent1, randomFillPercent2);
        m_map = m_mapGenerator.GenerateCombinedMaps(finalMapWidthCount, finalMapWidthCount / 2 + 1);
        //should get it after generate combine maps, null otw
        m_landTypeMap = m_mapGenerator.getLandTypeMap();
    }

    private void FillWithLandTypeZone()
    {
        for (int x = 0; x < m_landTypeMap.GetLength(0); x++)
        {
            for (int y = 0; y < m_landTypeMap.GetLength(1); y++)
            {
                Vector3 pos = new Vector3((-totalWidth / 2.0f) + (width * tileSize / 2.0f) + x * width * tileSize, (totalHeight / 2.0f) - (width * tileSize / 2.0f) - width * tileSize * y, 5);

                if (m_landTypeMap[x, y] == (int)LandType.GREENLAND)
                {
                    //Fill with GreenLandZone
                    //Debug.Log(pos);
                    GameObject newZone = Instantiate(GreenLandZone);
                    newZone.transform.SetParent(LandZoneType.transform);
                    newZone.transform.position = pos;
                }
                else if (m_landTypeMap[x, y] == (int)LandType.SANDLAND)
                {
                    //Fill with SandLandZone
                    //Debug.Log(pos);
                    GameObject newZone = Instantiate(SandLandZone);
                    newZone.transform.SetParent(LandZoneType.transform);
                    newZone.transform.position = pos;
                }
                else if (m_landTypeMap[x, y] == (int)LandType.RUINLAND)
                {
                    //Fill with RuinLandZone
                    //Debug.Log(pos);
                    GameObject newZone = Instantiate(RuinLandZone);
                    newZone.transform.SetParent(LandZoneType.transform);
                    newZone.transform.position = pos;
                }
            }
        }
    }


    //Fill the map with meshes: tiles, plants, animals.
    //plants and animals only generates at empty tiles that (x, y-1) is a ground tile.
    public void FillWithMesh()
    {
        System.Random pseudoRandom = new System.Random(seed.GetHashCode());

        if (m_map != null)
        {
            for (int x = 0; x < width * finalMapWidthCount; x++)
            {
                //height* finalMapWidthCount
                for (int y = 0; y < height * finalMapWidthCount; y++)
                {
                    //Fill with a tile, ground, stone .etc or emptyTile
                    FillWithTileType(m_map[x, y], x, y);

                    //if the index id surface, try to fill with creature
                    if (m_map[x, y] == 0)
                    {
                        if(PlantGenerationConditions(x, y))
                        {
                            int randIntPlant = pseudoRandom.Next(0, 100);
                           //Debug.Log("RandIntPlant: " + randIntPlant);
                            if (randIntPlant < plantsFillPercent)
                            {
                                //genertae planet
                                GenerateCreature(FruitPlant, x, y);
                            }
                        } 
                        
                        if(AnimalGenerationConditions(x, y))
                        {
                            int randIntAnimal = pseudoRandom.Next(0, 100);
                            //Debug.Log("RandIntAnimal: " + randIntAnimal);
                            if (randIntAnimal < animalFillPercent)
                            {
                                //generate animal
                                GenerateCreature(GrassAnimal, x, y);
                            }
                        }
                       
                    }
                    

                }
            }
        }
    }

    public void GenerateCreature(GameObject obj, int x, int y)
    {
        Vector3 pos = new Vector3((-totalWidth / 2.0f) + (tileSize / 2.0f) + x * tileSize, (totalHeight / 2.0f) - (tileSize / 2.0f) - tileSize * y, 1);
        //Debug.Log(pos);
        GameObject newCreature = Instantiate(obj);
        newCreature.transform.SetParent(transform);
        newCreature.transform.position = pos;
    }

    private bool PlantGenerationConditions(int x, int y)
    {
        if(y >= height * finalMapWidthCount)
        {
            return false;
        }
        ////is not surface
        //if(m_map[x, y - 1] == 0)
        //{
        //    return false;
        //}
        if(m_map[x, y + 1] == (int)TileType.DIRT)
        {
            return true;
        }
        return false;
    }

    public bool AnimalGenerationConditions(int x, int y)
    {
        if (y >= height * finalMapWidthCount)
        {
            return false;
        }
        if (m_map[x, y + 1] != 0)
        {
            return true;
        }
        return false;
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
       
        Vector3 pos = new Vector3((-totalWidth/2.0f) + (tileSize/2.0f) + x * tileSize, (totalHeight / 2.0f) - (tileSize / 2.0f) - tileSize * y, 3);
        //Debug.Log(pos);
        GameObject newTile = Instantiate(obj);
        newTile.transform.SetParent(transform);
        newTile.transform.position = pos;

        LandBrick tile = newTile.GetComponent<LandBrick>();
        EmptyTile EmptyTile = newTile.GetComponent<EmptyTile>();

        if (tile != null)
        {
            tile.index = new Vector2Int(x, y);
            //Debug.Log("index: " + x + ", " + y);
        }
        else
        {
            EmptyTile.index = new Vector2Int(x, y);
            //Debug.Log("index: " + x + ", " + y);
        }

    }

    public int[,] getInitialMap() {
        return m_map;
    }


}
