using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameResources;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LandBrick : PickedUpItems, IPointerClickHandler
{
    public PickedUpItemName myType;

    //public PlayerComponent m_player;
    BoxCollider2D m_collider;

    public float diggingTime = 0.5f;
    public float scaleChange = 0.5f;
    public bool isCracked = false;
    public WorldManager m_worldManager;

    public GameManagerForNetwork m_gameManager;

    public Vector2Int index;
    private bool isPickedUp = false;

    int maxDigDistance = 2;
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

    public void OnPointerClick(PointerEventData eventData)
    {
        PlayerComponent m_player = m_gameManager.LocalPlayer;
        m_worldManager = FindObjectOfType<WorldManager>();
        Debug.Log("index: " + index.x + ", " + index.y);
        if (m_gameManager.LocalPlayer.currentHolded is Pickaxe)
        {
            Pickaxe m_pickaxe = (Pickaxe)m_gameManager.LocalPlayer.currentHolded;
            maxDigDistance = m_pickaxe.pickRange;
            diggingTime = m_pickaxe.diggingTime;
        }

        if (!isCracked && checkDistance() && m_State == ItemState.DEFAULT)
        {
            if (m_player.m_status != PlayerComponent.PlayerStatus.DIGGING)
            {
                StartCoroutine("DigTile");
                
            }
        }
    }

    private IEnumerator DigTile()
    {
        PlayerComponent m_player = m_gameManager.LocalPlayer;
        m_player.StartDig();

        print("start cracked a land brick");
        yield return new WaitForSeconds(diggingTime);
        /*new a land fragment for pick up*/

        //crackALandTile();
        //isCracked = true;
        m_worldManager.UpdateTileMap(index, 0);
        m_player.EndDig();
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
        //if(m_gameManager.LocalPlayer.currentHolded is Pickaxe)
        //{
        //    Pickaxe m_pickaxe = (Pickaxe)m_gameManager.LocalPlayer.currentHolded;
        //    maxDigDistance = m_pickaxe.pickRange;
        //} 
        float distance = Vector2.Distance(m_gameManager.LocalPlayer.transform.position, transform.position);
        Debug.Log("max:" + maxDigDistance + "; distance: " + distance + "; allowedDistance: " + maxDigDistance * m_Size);
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
        if(collision.gameObject.tag == "Player" && m_State == ItemState.DEFAULT && !isPickedUp)
        {
            PlayerComponent m_player = collision.gameObject.GetComponent<PlayerComponent>();
            if (m_player.PickedUp(this))
            {
                isPickedUp = true;
                Debug.Log("Some player pick me!");
                Rigidbody2D rb = GetComponent<Rigidbody2D>();
                Destroy(rb);
            }
        }
    }

}
