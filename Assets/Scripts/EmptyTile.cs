using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EmptyTile : PickedUpItems, IPointerClickHandler
{
    public Vector2Int index;
    public WorldManager m_worldManager;
    public PlayerComponent m_player;
    public bool occupied;

    void Start()
    {
        //m_worldManager = GameObject.FindWithTag("WorldManager").GetComponent<WorldManager>();
        //GameObject playerGameObject = GameObject.FindWithTag("Player");
        //m_player = playerGameObject.GetComponent<PlayerComponent>();
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Click.." + transform.position.z);
        if (occupied)
        {
            return;
        }
        m_player = GameManagerForNetwork.Instance.LocalPlayer;
        m_worldManager = FindObjectOfType<WorldManager>();
        Debug.Log("index: " + index.x + ", " + index.y);
        if (m_player.currentHolded == null)
        {
            Debug.Log("Nothing holded in hands");
            return;
        }
        if (m_player.currentHolded.GetType() == typeof(LandBrick))
        {
            Debug.Log("holded a landBrick in hands");
            int landTileId = (int)m_player.currentHolded.getItemName();
            if (m_worldManager.UpdateTileMap(index, landTileId))
            {
                AudioManager.instance.PlaySound("dig");
                m_player.useItemInHand();
                Destroy(gameObject);
            }
        }
        else if (m_player.currentHolded.GetType() == typeof(Plant) || m_player.currentHolded.GetType().IsSubclassOf(typeof(Plant)))
        {
            Debug.Log("Plant something...");
            if (m_worldManager.Plant(index, (int)m_player.currentHolded.getItemName()))
            {
                AudioManager.instance.PlaySound("newPlant");
                m_player.useItemInHand();
                occupied = true;
            }
        } else if (m_player.currentHolded is PlaceableItem) //Lamp, LittleSun, CatsHome
        {
            Debug.Log("Trying to Place sth....");
            //for all clients, generate the item at correct position
            if (m_worldManager.Place(index, (int)m_player.currentHolded.getItemName()))
            {
                AudioManager.instance.PlaySound("newPlant");
                m_player.useItemInHand();
                occupied = true;
            }
        }
    }
}
