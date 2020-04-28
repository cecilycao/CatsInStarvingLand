﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummerCloth : Cloth
{
    public override GameResources.PickedUpItemName getItemName()
    {
        return GameResources.PickedUpItemName.SUMMER_CLOTH;
    }

    public override GameObject getGameObj()
    {
        return gameObject;
    }
}