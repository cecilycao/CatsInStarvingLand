using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameResources;

public class PlayerComponent : MonoBehaviourPun, IPunObservable
{

    public static PlayerComponent instance;
    
    public int m_health;//current health
    public int m_hunger;
    public double m_temperature;
    public int m_tiredness;

    private bool isInvincible; //是否无敌

    public PickedUpItems currentHolded;

    private int maxHealth = 100;
    public int myMaxHealth
    {
        get { return maxHealth; }
    }
    public int myCurrentHealth
    {
        get { return m_health; }
    }
    private float invincibleTime = 2f; //无敌时间
    private float invincibleTimer;

    public int maxHunger = 100;

    public int myCurrentHunger
    {
        get { return m_hunger; }
    }

    

    public GameObject bulletObj;

    private Vector2 lookDeriction = new Vector2(1, 0);


    public int surroundingTemperature;

    public enum m_status
    {
        DEFAULT,
        ATTACK,
        SLEEP,
        DEAD
    };

    

    public Transform HoldedPosition;

    public Backpack myBackpack;

    public UIManager myUIManager;

    private float lastTempCheck;

    private APCharacterController APcontroller;
    Animator m_anim;

    private void OnEnable()
    {
        //if(instance == null)
        //{
        //    instance = this;
        //} else
        //{
        //    if(PlayerComponent.instance != this)
        //    {
        //        Destroy(PlayerComponent.instance.gameObject);
        //        PlayerComponent.instance = this;
        //    }
        //}
        //DontDestroyOnLoad(this.gameObject);
    }

    private void Awake()
    {
        APcontroller = GetComponent<APCharacterController>();
        m_anim = GetComponent<Animator>();

        if (!photonView.IsMine && GetComponent<APCharacterController>() != null)
        {
            Destroy(GetComponent<APCharacterController>());
            //Destroy Motor????
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        APcontroller = GetComponent<APCharacterController>();


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

        lastTempCheck = Time.time;    
    }

    // Update is called once per frame
    void Update()
    {
        //health -1 / 3s

        //tiredness -10 / 27s


        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer < 0)
            {
                isInvincible = false;
            }
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            attack();
        }
        BodyTempCheckBasedOnTemp();
        
    }

    void BodyTempCheckBasedOnTemp() {
        if (Time.time - lastTempCheck >= 1)
        {
            if (this.surroundingTemperature > 28)
            {
                m_temperature += 0.1;
            }
            else if (this.surroundingTemperature < 10)
            {
                m_temperature -= 0.1;
            }
            lastTempCheck = Time.time;

            Debug.Log("当前环境温度:" + surroundingTemperature + "   当前体温: " + m_temperature);
            myUIManager.UpdateTemperature(m_temperature);
        }

    }
    public void changeHunger(int amount)
    {
       
            //Debug.Log("玩家当前饥饿值：" + m_hunger + "/" + maxHunger);
            m_hunger = Mathf.Clamp(m_hunger + amount, 0, maxHunger);
            Debug.Log("玩家当前饥饿值：" + m_hunger + "/" + maxHunger);
            //myUIManager.UpdateHunger(m_hunger);
    }

    void attack()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        GameObject bullet = Instantiate(bulletObj, APcontroller.GetRigidBody().position, Quaternion.identity);
        Bullet Bc = bullet.GetComponent<Bullet>();
        if (Bc != null)
        {
            Bc.BulletMove(lookDeriction, 300);
        }
    }

    void OnMouseDown()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        Debug.Log("Click character");
        //ChangeValue();
        useItemInHand();

    }

    public void PlayerInitialize()
    {
        StartCoroutine("Digest");
        StartCoroutine("Working");
    }

    IEnumerator Digest()
    {
        if (m_hunger > 0)
        {
            m_hunger--;

            yield return new WaitForSeconds(3);
        }
        else
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

    //Only local player can pick up things
    public bool PickedUp(PickedUpItems item)
    {
        if (!photonView.IsMine)
        {
            return false;
        }
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
        if (item.m_State == PickedUpItems.ItemState.IN_BAG)
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

    public void ChangeHealth(int amount)
    {
        if (amount < 0)
        {
            if (isInvincible == true)
            {
                return;
            }
            isInvincible = true;
            invincibleTimer = invincibleTime;
        }

        Debug.Log("player" + m_health + "/" + maxHealth);
        m_health = Mathf.Clamp(m_health + amount, 0, maxHealth);
        Debug.Log("player" + m_health + "/" + maxHealth);
    }

    public void useItemInHand()
    {
        //if Food:
        //if(currentHolded.getItemName() == PickedUpItemName.FRUIT)
        //{
        //    //ChangeHealth();
        //if (currentHolded.getItemName() == PickedUpItemName.DRIED_FISH)
        //{
        //    changeHunger(10);
        //}
        Destroy(currentHolded.gameObject);
        //}

    }

    //public void ChangeValue()
    //{
    //    if (currentHolded.getItemName() == PickedUpItemName.DRIED_FISH)
    //    {
    //        changeHunger(10);
    //    }
    //}

    //Change player's temperature based on surrounding temperature.
    //if surrounding temperature > 28, m_temperature +0.1/s
    //if surrounding temperature < 10, m_temperature -0.1/s
    public void changeZone(int surroundingTemperature)
    {
        this.surroundingTemperature = surroundingTemperature;
    }

    

    public static void RefreshInstance(ref PlayerComponent player, PlayerComponent Prefab)
    {
        var position = Vector3.zero;
        var rotation = Quaternion.identity;

        if (player != null)
        {
            position = player.transform.position;
            rotation = player.transform.rotation;
            PhotonNetwork.Destroy(player.gameObject);
        }
        player = PhotonNetwork.Instantiate(Prefab.gameObject.name, position, rotation).GetComponent<PlayerComponent>();

    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        throw new NotImplementedException();
    }
}
