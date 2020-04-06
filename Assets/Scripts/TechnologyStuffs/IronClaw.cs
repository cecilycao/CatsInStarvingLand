using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameResources;

public class IronClaw : Claw
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public override PickedUpItemName getItemName()
    {
        return PickedUpItemName.IRON_CLAW;
    }

    public override GameObject getGameObj()
    {
        return gameObject;
    }
}
