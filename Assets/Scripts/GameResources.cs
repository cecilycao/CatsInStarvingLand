using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameResources : MonoBehaviour
{


    public enum TileType{
        EMPTY,
        DIRT,
        STONE,
        SAND,
        IRON,
        SPARE
    }

    public enum PickedUpItemName
    {
        DEFAULT,
        //Mines
        DIRT,
        STONE,
        IRON,
        SAND,
        //RESOURCES
        WOOD,
        POOPOO,
        FLOWER,
        BUTTERFLY,
        WOOL,
        SPARE,
        GAS_TANK,
        //EATABLE
        FRUIT,
        DRIED_FISH,
        //---TECH STUFF--
        //WEALPON
        STONE_CLAW,
        IRON_CLAW,
        TECH_CLAW,
        //STUFF
        STONE_PICKAXE,
        IRON_PICKAXE,
        EXCAVATOR,
        //LIGHTING
        LIGHT_BULB,
        LAMP,
        LITTLE_SUN,
        //CLOTH
        SUMMER_CLOTH,
        WINTER_CLOTH,
        //FUNITURE
        CATERRY,
        WOOD_FENCE,
        CHEST
    }
}
