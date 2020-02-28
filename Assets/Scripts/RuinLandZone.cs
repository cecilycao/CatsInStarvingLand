﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuinLandZone : MonoBehaviour
{
    public int temperature = 0;
    BoxCollider2D m_collider;

    // Start is called before the first frame update
    void Start()
    {
        m_collider = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            Debug.Log("Hi!!! You are in ruinLand!!!");
            collision.gameObject.GetComponent<PlayerComponent>().changeZone(temperature);
        }
    }
}
