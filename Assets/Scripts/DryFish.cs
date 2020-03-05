using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DryFish : PickedUpItems
{
    //public PlayerComponent m_player;
    // Start is called before the first frame update
    void Start()
    {
        //m_player = GameObject.FindWithTag("Player").GetComponent<PlayerComponent>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnTriggerEnter2D(Collider2D other)
    {
        

        if (other.gameObject.tag == "Player" && m_State == ItemState.DEFAULT)
        { 
            Debug.Log("Pick DryFish!");
            PlayerComponent m_player = other.GetComponent<PlayerComponent>();
            m_player.PickedUp(this);
            
        }

    }

    public static implicit operator GameObject(DryFish v)
    {
        throw new NotImplementedException();
    }

    public override GameResources.PickedUpItemName getItemName()
    {
        return GameResources.PickedUpItemName.DRIED_FISH;
    }
}
