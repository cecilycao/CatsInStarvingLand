using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameResources;

public class Lightbulb : PlaceableItem
{


    public override PickedUpItemName getItemName()
    {
        return PickedUpItemName.LIGHT_BULB;
    }
}
