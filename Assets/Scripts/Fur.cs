using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fur : PickedUpItems
{
    void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.gameObject.tag == "Player" && m_State == ItemState.DEFAULT)
        {
            Debug.Log("Pick Fur!");
            PlayerComponent m_player = other.GetComponent<PlayerComponent>();
            m_player.PickedUp(this);

        }

    }

    public override GameResources.PickedUpItemName getItemName()
    {
        return GameResources.PickedUpItemName.WOOL;
    }

    public override GameObject getGameObj()
    {
        return gameObject;
    }
}
