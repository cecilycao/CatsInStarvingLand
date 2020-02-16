using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameResources;

public class Poopoo : PickedUpItems
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override PickedUpItemName getItemName()
    {
        return PickedUpItemName.POOPOO;
    }
}
