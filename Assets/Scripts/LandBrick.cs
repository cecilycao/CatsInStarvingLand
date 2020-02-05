using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LandBrick : MonoBehaviour
{
    public GameObject m_player;
    public int maxDigDistance = 6;
    float m_Size;
    // Start is called before the first frame update
    void Start()
    {
        m_Size = GetComponent<BoxCollider2D>().bounds.size.x;
        //alterLand.onClick.AddListener(AlterLand);
        m_player = GameObject.FindWithTag("Player");
    }

    void OnMouseDown()
    {
        if (checkDistance())
        {
            if (true/*has pickaxe or nothing in hand */)
            {
                print("clicked a land brick");
                /*new a land fragment for pick up*/
                Destroy(gameObject);
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
}
