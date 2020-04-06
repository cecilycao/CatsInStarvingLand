using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IronPickaxe : Pickaxe
{

    public override GameResources.PickedUpItemName getItemName()
    {
        return GameResources.PickedUpItemName.IRON_PICKAXE;
    }

    public override GameObject getGameObj()
    {
        return gameObject;
    }
}
