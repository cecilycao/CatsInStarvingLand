using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinterCloth : Cloth
{
    public override GameResources.PickedUpItemName getItemName()
    {
        return GameResources.PickedUpItemName.WINTER_CLOTH;
    }

    public override GameObject getGameObj()
    {
        return gameObject;
    }
}
