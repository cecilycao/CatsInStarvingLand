﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public override Resources.PickedUpItemName getItemName()
    {
        return Resources.PickedUpItemName.POOPOO;
    }
}