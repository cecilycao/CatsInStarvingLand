using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyTile : PickedUpItems
{
    public Vector2Int index;
    public WorldManager m_worldManager;
    public PlayerComponent m_player;

    void Start()
    {
        //m_worldManager = GameObject.FindWithTag("WorldManager").GetComponent<WorldManager>();
        GameObject playerGameObject = GameObject.FindWithTag("Player");
        m_player = playerGameObject.GetComponent<PlayerComponent>();
    }

    void OnMouseDown()
    {
        m_worldManager = FindObjectOfType<WorldManager>();
        Debug.Log("index: " + index.x + ", " + index.y);
        if(m_player.currentHolded == null)
        {
            return;
        }
        if(m_player.currentHolded.GetType() == typeof(LandBrick))
        {
            int landTileId = (int)m_player.currentHolded.getItemName();
            if(m_worldManager.UpdateTileMap(index, landTileId))
            {
                m_player.useItemInHand();
                Destroy(gameObject);
            }
        }
        
    }
}
