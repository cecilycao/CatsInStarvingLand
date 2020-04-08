using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LittleSun : PlaceableItem
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override GameResources.PickedUpItemName getItemName()
    {
        return GameResources.PickedUpItemName.LITTLE_SUN;
    }

    public override GameObject getGameObj()
    {
        return gameObject;
    }
}
