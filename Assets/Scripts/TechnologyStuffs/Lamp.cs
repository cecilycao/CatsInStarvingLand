using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Lamp : PlaceableItem
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
        return GameResources.PickedUpItemName.LAMP;
    }

    public override GameObject getGameObj()
    {
        return gameObject;
    }
}
