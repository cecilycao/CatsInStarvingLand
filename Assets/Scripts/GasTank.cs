using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameResources;

public class GasTank : PickedUpItems
{
    public override PickedUpItemName getItemName()
    {
        return PickedUpItemName.GAS_TANK;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player" && m_State == ItemState.DEFAULT)
        {
            Debug.Log("Pick me!");
            PlayerComponent current_player = other.GetComponent<PlayerComponent>();
            current_player.PickedUp(this);
        }
    }

    public override GameObject getGameObj()
    {
        return gameObject;
    }
}
