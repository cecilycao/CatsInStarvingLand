using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StonePickaxe : Pickaxe
{

    public override GameResources.PickedUpItemName getItemName()
    {
        return GameResources.PickedUpItemName.STONE_PICKAXE;
    }

    public override GameObject getGameObj()
    {
        return gameObject;
    }
}
