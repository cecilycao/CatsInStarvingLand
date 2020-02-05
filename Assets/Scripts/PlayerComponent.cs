﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerComponent : MonoBehaviour
{

    public int m_health;
    public int m_hunger;
    public int m_temperature;
    public int m_tiredness;

    public PickedUpItems currentHolded;


    public Transform HoldedPosition;


    // Start is called before the first frame update
    void Start()
    {
        m_health = 100;
        m_hunger = 100;
        m_temperature = 38;
        m_tiredness = 100;

        //temp
        PlayerInitialize();

        //temporary for testing use
        if (currentHolded != null)
        {
            currentHolded.transform.SetParent(transform);
            currentHolded.transform.position = HoldedPosition.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //health -1 / 3s

        //tiredness -10 / 27s


    }

    public void PlayerInitialize()
    {
        StartCoroutine("Digest");
        StartCoroutine("Working");
    }

    IEnumerator Digest()
    {
        if(m_hunger > 0)
        {
            m_hunger--;
            
            yield return new WaitForSeconds(3);
        } else
        {
            //decrease health
        }
    }

    IEnumerator Working()
    {
        if (m_tiredness > 0)
        {
            m_tiredness--;
            yield return new WaitForSeconds(2.7f);
        }
        else
        {
            //decrease health
        }
    }

    public void PickedUp(PickedUpItems item)
    {
        //put in bag
        //destroy item
    }

    public void HoldItemInHand(PickedUpItems item)
    {
        if(item.m_State == PickedUpItems.ItemState.IN_BAG)
        {
            //hold in hand(change UI?)
            item.m_State = PickedUpItems.ItemState.IN_HAND;
            currentHolded = item;
            item.transform.SetParent(transform);
            item.transform.position = HoldedPosition.position;
        }
    }

    //Use this function to get whats in hand
    public PickedUpItems GetWhatsInHand()
    {
        return currentHolded;
    }
}
