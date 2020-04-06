using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Excavator : Pickaxe
{

    public override GameResources.PickedUpItemName getItemName()
    {
        return GameResources.PickedUpItemName.EXCAVATOR;
    }

    public override GameObject getGameObj()
    {
        return gameObject;
    }
}
