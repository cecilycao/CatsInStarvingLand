using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameResources;
using UnityEngine.UI;

public class LandBrick : PickedUpItems
{
    public PickedUpItemName myType;

    //public PlayerComponent m_player;
    BoxCollider2D m_collider;

    public float scaleChange = 0.5f;
    public bool isCracked = false;
    public int maxDigDistance = 6;
    public WorldManager m_worldManager;

    public GameManagerForNetwork m_gameManager;

    public Vector2Int index;

    float m_Size;
    // Start is called before the first frame update
    void Start()
    {
        
        //alterLand.onClick.AddListener(AlterLand);
        //m_player = GameObject.FindWithTag("Player").GetComponent<PlayerComponent>();
        m_collider = GetComponent<BoxCollider2D>();
        m_Size = m_collider.bounds.size.x;
        m_gameManager = GameObject.FindWithTag("WorldManager").GetComponent<GameManagerForNetwork>();


    }

    void OnMouseDown()
    {
        m_worldManager = FindObjectOfType<WorldManager>();
        Debug.Log("index: " + index.x + ", " + index.y);
        if (!isCracked && checkDistance() && m_State == ItemState.DEFAULT)
        {
            if (true/*has pickaxe or nothing in hand */)
            {
                print("cracked a land brick");
                /*new a land fragment for pick up*/

                //crackALandTile();
                //isCracked = true;
                m_worldManager.UpdateTileMap(index, 0);
                AudioManager.instance.PlaySound("dig");
            }
        }

    }

    public void crackALandTile()
    {
        Vector3 newScale = gameObject.transform.localScale;
        newScale *= scaleChange;
        gameObject.transform.localScale = newScale;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.mass = 1;
        //m_collider.isTrigger = true;
    }

    private bool checkDistance()
    {
        float distance = Vector2.Distance(m_gameManager.LocalPlayer.transform.position, transform.position);
        if (distance > maxDigDistance * m_Size)
        {
            return false;
        }
        return true;
    }

    public override PickedUpItemName getItemName()
    {
        return myType;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Player" && m_State == ItemState.DEFAULT)
        {
            Debug.Log("Pick me!");
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            Destroy(rb);
            PlayerComponent m_player = collision.gameObject.GetComponent<PlayerComponent>();
            m_player.PickedUp(this);
            
        }
    }

}
