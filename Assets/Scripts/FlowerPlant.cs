using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameResources;



public class FlowerPlant : Plant
{

    public override PickedUpItemName getItemName()
    {
        return PickedUpItemName.FLOWER_PLANT;
    }

    public override GameObject getGameObj()
    {
        return gameObject;
    }

}