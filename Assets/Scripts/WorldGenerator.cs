using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameResources;

public class WorldGenerator : MonoBehaviourPun
{
    public GameObject LandTile_DIRT;
    public GameObject LandTile_STONE;
    public GameObject LandTile_SAND;
    public GameObject LandTile_IRON;
    public GameObject LandTile_SPARE;
    public GameObject LandTile_Empty;

    public GameObject FruitPlant;
    public GameObject LuminousPlant;
    public GameObject FlowerPlant;
    public GameObject GrassAnimal;
    public GameObject SandAnimal;
    public GameObject RuinAnimal;

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
    public int fruitPlantsFillPercent;
    public int luminousPlantFillPercent;
    public int animalFillPercent;

    MapGenerator m_mapGenerator;
    [HideInInspector]
    public int[,] m_map; // widthCount * width, widthCount * height
    public int[,] m_landTypeMap; // widthCount, widthCount
    public PickedUpItems[,] TileMap;

    // Start is called before the first frame update
    public void GenerateWorld(string seed)
    {
        this.seed = seed;
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
            TileMap = new PickedUpItems[m_map.GetLength(0), m_map.GetLength(1)];
            for (int x = 0; x < width * finalMapWidthCount; x++)
            {
                //height* finalMapWidthCount
                for (int y = 0; y < height * finalMapWidthCount; y++)
                {
                    //Fill with a tile, ground, stone .etc or emptyTile
                    FillWithTileType(m_map[x, y], x, y);

                    LandType type = getLandType(x, y);
                    //if the index is surface, try to fill with creature
                    if (m_map[x, y] == 0 && PhotonNetwork.IsMasterClient)
                    {
                        if(PlantGenerationConditions(x, y))
                        {
                            print("Generate plant..");
                            int randIntPlant = pseudoRandom.Next(0, 100);
                            int totalPlantPercent = fruitPlantsFillPercent + luminousPlantFillPercent;
                            int randIntOneKindPlant = pseudoRandom.Next(0, totalPlantPercent);
                            if (randIntPlant < (fruitPlantsFillPercent + luminousPlantFillPercent))
                            {
                                //can generate plant here
                                if(randIntOneKindPlant < fruitPlantsFillPercent)
                                {
                                    if(type == LandType.SANDLAND)
                                        GeneratePlant((int)PickedUpItemName.FLOWER_PLANT, x, y, true);

                                    if (type == LandType.GREENLAND || type == LandType.RUINLAND)
                                        GeneratePlant((int)PickedUpItemName.FRUIT_PLANT, x, y, true);
                                } else if(randIntOneKindPlant < (fruitPlantsFillPercent + luminousPlantFillPercent))
                                {
                                    GeneratePlant((int)PickedUpItemName.LIGHT_PLANT, x, y, true);
                                }
                            }
                        } 
                        
                        if(AnimalGenerationConditions(x, y))
                        {
                            print("Generate animal..");
                            int randIntAnimal = pseudoRandom.Next(0, 100);
                            //Debug.Log("RandIntAnimal: " + randIntAnimal);
                            if (randIntAnimal < animalFillPercent)
                            {
                                //generate animal
                                //if in greenland
                                if(type == LandType.GREENLAND)
                                    GenerateCreature(GrassAnimal, x, y);

                                //if in sandland
                                if(type == LandType.SANDLAND)
                                    GenerateCreature(SandAnimal, x, y);

                                //if in ruinland
                                if (type == LandType.RUINLAND)
                                    GenerateCreature(RuinAnimal, x, y);

                            }
                        }
                       
                    }
                    

                }
            }
        }
    }

    //Creatures are networked items
    public GameObject GenerateCreature(GameObject obj, int x, int y)
    {
        Vector3 pos = new Vector3((-totalWidth / 2.0f) + (tileSize / 2.0f) + x * tileSize, (totalHeight / 2.0f) - (tileSize / 2.0f) - tileSize * y, 1);
        //Debug.Log(pos);
        //GameObject newCreature = Instantiate(obj);
        GameObject newCreature = PhotonNetwork.InstantiateSceneObject(obj.name, Vector3.zero, Quaternion.identity);
        newCreature.transform.SetParent(transform);
        newCreature.transform.position = pos;
        return newCreature;
    }

    //Items are not networked
    public GameObject GenerateItem(GameObject obj, int x, int y)
    {
        Vector3 pos = new Vector3((-totalWidth / 2.0f) + (tileSize / 2.0f) + x * tileSize, (totalHeight / 2.0f) - (tileSize / 2.0f) - tileSize * y, 1);
        //Debug.Log(pos);
        //GameObject newCreature = Instantiate(obj);
        GameObject newItem = Instantiate(obj);
        newItem.transform.SetParent(transform);
        newItem.transform.position = pos;
        PlaceableItem placeable = newItem.GetComponent<PlaceableItem>();
        placeable.index = new Vector2Int(x, y);
        return newItem;
    }

    public void GeneratePlant(int obj, int x, int y, bool hasFruit)
    {
        GameObject newPlant;
        if(obj == (int)PickedUpItemName.FRUIT_PLANT)
        {
            newPlant = GenerateCreature(FruitPlant, x, y);
            Plant m_plant = newPlant.GetComponent<Plant>();
            m_plant.setFruitStatus(hasFruit);
            m_plant.setIndex(x, y);
            
        } else if(obj == (int)PickedUpItemName.LIGHT_PLANT)
        {
            newPlant = GenerateCreature(LuminousPlant, x, y);
            LuminousPlant m_luPlant = newPlant.GetComponent<LuminousPlant>();
            m_luPlant.setFruitStatus(hasFruit);
            m_luPlant.setIndex(x, y);
        }
        else if (obj == (int)PickedUpItemName.FLOWER_PLANT)
        {
            newPlant = GenerateCreature(FlowerPlant, x, y);
            FlowerPlant m_fPlant = newPlant.GetComponent<FlowerPlant>();
            m_fPlant.setFruitStatus(hasFruit);
            m_fPlant.setIndex(x, y);
        }


    }

    public bool PlantGenerationConditions(int x, int y)
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
        if(m_map[x, y + 1] == (int)TileType.DIRT || m_map[x, y + 1] == (int)TileType.SAND)
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

    public bool PlaceItemCondition(int x, int y, int obj)
    {
        if(obj == (int) PickedUpItemName.LAMP || obj == (int)PickedUpItemName.LITTLE_SUN)
        {
            return true;
        }
        //!!!!!!!!!!!not complete here
        return false;
    }




    public void FillWithTileType(int TileTypeVal, int x, int y)
    {
        if (TileTypeVal == (int)TileType.DIRT)
        {
            TileMap[x, y] = FillWith(LandTile_DIRT, x, y);
        }
        else if (TileTypeVal == (int)TileType.STONE)
        {
            TileMap[x, y] = FillWith(LandTile_STONE, x, y);
        }
        else if (TileTypeVal == (int)TileType.SAND)
        {
            TileMap[x, y] = FillWith(LandTile_SAND, x, y);
        }
        else if (TileTypeVal == (int)TileType.IRON)
        {
            TileMap[x, y] = FillWith(LandTile_IRON, x, y);
        }
        else if (TileTypeVal == (int)TileType.SPARE)
        {
            TileMap[x, y] = FillWith(LandTile_SPARE, x, y);
        }
        else if (TileTypeVal == 0)
        {
            TileMap[x, y] = FillWith(LandTile_Empty, x, y);
        }
    }

    PickedUpItems FillWith(GameObject obj, int x, int y)
    {
        //print("Fill with landtile at " + x + ", " + y);
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
            return tile;
        }
        else
        {
            EmptyTile.index = new Vector2Int(x, y);
            //Debug.Log("index: " + x + ", " + y);
            return EmptyTile;
        }

    }

    public LandType getLandType(int x, int y)
    {
        if (m_landTypeMap[x / width, y / height] == 0)
        {
            return LandType.GREENLAND;
        } else if (m_landTypeMap[x / width, y / height] == 1)
        {
            return LandType.SANDLAND;
        } else
        {
            return LandType.RUINLAND;
        }
    }

    public int[,] getInitialMap() {
        return m_map;
    }

    public PickedUpItems getTileComponent(int x, int y)
    {
        return TileMap[x, y];
    }


}
