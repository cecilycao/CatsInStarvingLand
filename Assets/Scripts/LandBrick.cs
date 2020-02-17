using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameResources;
using UnityEngine.UI;

public class LandBrick : PickedUpItems
{
    public PickedUpItemName myType;

    public PlayerComponent m_player;
    BoxCollider2D m_collider;

    public float scaleChange = 0.5f;
    public bool isCracked = false;
    public int maxDigDistance = 6;
    float m_Size;
    // Start is called before the first frame update
    void Start()
    {
        
        //alterLand.onClick.AddListener(AlterLand);
        m_player = GameObject.FindWithTag("Player").GetComponent<PlayerComponent>();
        m_collider = GetComponent<BoxCollider2D>();
        m_Size = m_collider.bounds.size.x;
    }

    void OnMouseDown()
    {
        if (!isCracked && checkDistance())
        {
            if (true/*has pickaxe or nothing in hand */)
            {
                print("cracked a land brick");
                /*new a land fragment for pick up*/

                Vector3 newScale = gameObject.transform.localScale;
                newScale *= scaleChange;
                gameObject.transform.localScale = newScale;

                m_collider.isTrigger = true;
                isCracked = true;
            }
        }

    }

    private bool checkDistance()
    {
        float distance = Vector2.Distance(m_player.transform.position, transform.position);
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.tag == "Player")
        {
            Debug.Log("Pick me!");
            if (m_player.PickedUp(this))
            {
                Destroy(gameObject);
            }
            
        }
    }
}
