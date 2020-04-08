using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuinLandKey : Key
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public override GameResources.PickedUpItemName getItemName()
    {
        return GameResources.PickedUpItemName.RUINLAND_KEY;
    }

    public override GameObject getGameObj()
    {
        return gameObject;
    }
}
