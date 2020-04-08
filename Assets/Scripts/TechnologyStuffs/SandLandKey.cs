using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandLandKey : Key
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public override GameResources.PickedUpItemName getItemName()
    {
        return GameResources.PickedUpItemName.SANDLAND_KEY;
    }

    public override GameObject getGameObj()
    {
        return gameObject;
    }
}
