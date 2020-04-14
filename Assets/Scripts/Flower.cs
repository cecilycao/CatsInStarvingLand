using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameResources;

public class Flower : PlaceableItem
{


    public override PickedUpItemName getItemName()
    {
        return PickedUpItemName.FLOWER;
    }
}
