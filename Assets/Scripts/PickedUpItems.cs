using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Items That can be picked up 
*/
public class PickedUpItems : MonoBehaviour
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

    public virtual Resources.PickedUpItemName getItemName()
    {
        return Resources.PickedUpItemName.DEFAULT;
    }

}
