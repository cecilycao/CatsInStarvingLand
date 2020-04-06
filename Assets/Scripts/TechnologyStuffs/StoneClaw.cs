using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameResources;

public class StoneClaw : Claw
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
        return PickedUpItemName.STONE_CLAW;
    }

    public override GameObject getGameObj()
    {
        return gameObject;
    }
}
