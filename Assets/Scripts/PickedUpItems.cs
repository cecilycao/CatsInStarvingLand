using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameResources;

/*
Items That can be picked up 
*/
public class PickedUpItems : MonoBehaviourPun
{
    public ItemState m_State;
    //add public ItemName
    
    public enum ItemState
    {
        DEFAULT,
        IN_HAND,
        IN_BAG
    }

    public PickedUpItems()
    {
        m_State = ItemState.DEFAULT;
    }

    public virtual PickedUpItemName getItemName()
    {
        return PickedUpItemName.DEFAULT;
    }

}
