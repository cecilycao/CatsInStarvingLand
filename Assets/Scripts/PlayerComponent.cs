using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameResources;

public class PlayerComponent : MonoBehaviour
{
    //health component
    public int m_health;//current health
    private int maxHealth =100;
    public int myMaxHealth{
        get{return maxHealth;}
    }
    public int myCurrentHealth{
        get{return m_health;}
    }
     private float invincibleTime = 2f; //无敌时间
    private float invincibleTimer;  
    private bool isInvincible; //是否无敌

    public int m_hunger;
    public int m_temperature;
    public int m_tiredness;

    public int surroundingTemperature;

    public enum m_status {
        DEFAULT,
        ATTACK,
        SLEEP
    };

    public PickedUpItems currentHolded;

    public Transform HoldedPosition;

    public Backpack myBackpack;

    public UIManager myUIManager;

    // Start is called before the first frame update
    void Start()
    {
        Inventory iv = FindObjectOfType<Inventory>();
        myBackpack = new Backpack(iv);


        m_health = 100;
        invincibleTimer = 0;

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
        

     if(isInvincible){
            invincibleTimer -= Time.deltaTime;
            if(invincibleTimer<0){
                isInvincible = false;
            }
        }

    }



    void OnMouseDown()
    {
        useItemInHand();
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

    public bool PickedUp(PickedUpItems item)
    {
        GameResources.PickedUpItemName name = item.getItemName();
        if (currentHolded == null)
        {
            item.m_State = PickedUpItems.ItemState.IN_BAG;
            HoldItemInHand(item);
            return true;
        }
        //put in bag
        if (myBackpack.AddNewItem(name))
        {
            item.m_State = PickedUpItems.ItemState.IN_BAG;
            Destroy(item.gameObject);
            return true;
        }
        return false;
    }

    public void HoldItemInHand(PickedUpItems item)
    {
        if(item.m_State == PickedUpItems.ItemState.IN_BAG)
        {
            //hold in hand(change UI?)
            item.m_State = PickedUpItems.ItemState.IN_HAND;
            currentHolded = item;

            Debug.Log("Hold Item: Item exist.");
            item.transform.SetParent(transform);
            item.transform.position = HoldedPosition.position;

            
        }
    }

    //Use this function to get whats in hand
    public PickedUpItems GetWhatsInHand()
    {
        return currentHolded;
    }

    public void ChangeHealth(int amount){
        if(amount <0){
            if(isInvincible==true){
                return;
            }
            isInvincible = true;
            invincibleTimer = invincibleTime;
        }

        Debug.Log(m_health+"/"+maxHealth);
        m_health =Mathf.Clamp(m_health+amount,0,maxHealth);
        Debug.Log(m_health+"/"+maxHealth);
    }

    public void useItemInHand()
    {
        //if Food:
        //if(currentHolded.getItemName() == PickedUpItemName.FRUIT)
        //{
        //    //ChangeHealth();
            Destroy(currentHolded.gameObject);
        //}


    }

    //Change player's temperature based on surrounding temperature.
    //if surrounding temperature > 28, m_temperature +0.1/s
    //if surrounding temperature < 10, m_temperature -0.1/s
    public void changeZone(int surroundingTemperature)
    {
        this.surroundingTemperature = surroundingTemperature;
    }

}
