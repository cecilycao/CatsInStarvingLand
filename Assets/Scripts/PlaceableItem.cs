using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;

public class PlaceableItem : PickedUpItems, IPointerClickHandler
{
    public Vector2Int index;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("click a placeable item");
        //find local player
        PlayerComponent m_player = GameManagerForNetwork.Instance.LocalPlayer.GetComponent<PlayerComponent>();

        PickedUpItems holdedItem = m_player.GetWhatsInHand();

        //pick up a placeable
        if (true)
        {
            //photonView.RPC("RpcPickUpItem", RpcTarget.AllBuffered, m_player.m_ID);
            PickUpItem(m_player);
        }
    }

    [PunRPC]
    public void RpcPickUpItem(int playerID)
    {
        PlayerComponent m_player = WorldManager.Instance.getPlayer(playerID);
        m_player.PickedUp(this);
        WorldManager.Instance.clearObjectAt(index.x, index.y);
    }

    public void PickUpItem(PlayerComponent m_player)
    {
        //PlayerComponent m_player = WorldManager.Instance.getPlayer(playerID);
        m_player.PickedUp(this);
        WorldManager.Instance.clearObjectAt(index.x, index.y);
    }
}
