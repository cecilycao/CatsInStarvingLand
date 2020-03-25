using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static GameResources;

public class PlayerComponent : MonoBehaviourPun, IPunObservable
{

    public static PlayerComponent instance;
    
    public int m_health;//current health
    public int m_hunger;
    public double m_temperature;
    public int m_tiredness;
    public int m_ID;
    public PlayerStatus m_status;
    public bool bagFull = false;

    private bool isInvincible; //是否无敌

    public PickedUpItems currentHolded;
    //public int HoldedItemID;

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

    public SpriteRenderer HoldedItemSprite;

    public Transform MeleePosition;

    public Inventory myBackpack;

    public UIManager myUIManager;

    private float lastTempCheck;

    private APCharacterController APcontroller;
    Animator m_anim;

    public enum PlayerStatus
    {
        DEFAULT,
        ATTACK,
        SLEEP,
        DEAD
    };

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
        m_ID = GetComponent<PhotonView>().ViewID;

        this.myBackpack = FindObjectOfType<Inventory>();

        myUIManager = FindObjectOfType<UIManager>();

        //initialize player status
        invincibleTimer = 0;
        m_health = 100;
        m_hunger = 100;
        m_temperature = 38;
        m_tiredness = 100;

        m_status = PlayerStatus.DEFAULT;

        if (photonView.IsMine)
        {
            myUIManager.UpdateHealth(m_health);
            myUIManager.UpdateHunger(m_hunger);
            myUIManager.UpdateTemperature(m_temperature);
            myUIManager.UpdateTiredness(m_tiredness);

            InvokeRepeating("Digest", 1f, 1f);
            InvokeRepeating("Working", 1f, 1f);
        }
        

        

        lastTempCheck = Time.time;   
        
        if (HoldedItemSprite == null)
        {
            Debug.Log("Remember to attach holded item sprite");
        }
    }

    // Update is called once per frame
    void Update()
    {
        //tiredness -10 / 27s

        if (m_status != PlayerStatus.DEAD && photonView.IsMine)
        {
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
                Attack();
            }
            BodyTempCheckBasedOnTemp();

            if (m_health <= 0)
            {
                OnDeath();
            }
        }
        
    }

    void Digest()
    {
        if(m_status == PlayerStatus.DEAD)
        {
            return;
        }
        if (m_hunger > 0)
        {
            m_hunger--;
            if(photonView.IsMine)
                myUIManager.UpdateHunger(m_hunger);
        }
        else
        {
            m_health -= 3;
            if (photonView.IsMine)
            {
                myUIManager.UpdateHunger(m_hunger);
                myUIManager.UpdateHealth(m_health);
            }
            
            //decrease health
        }
    }

    void Working()
    {
        if (m_status == PlayerStatus.DEAD)
        {
            return;
        }
        if (m_tiredness > 0)
        {
            m_tiredness--;
            if(photonView.IsMine)
                myUIManager.UpdateTiredness(m_tiredness);
        }
        else
        {
            //decrease health
            m_health -= 3;
            if (photonView.IsMine)
            {
                myUIManager.UpdateTiredness(m_tiredness);
                myUIManager.UpdateHealth(m_health);
            }
        }
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

            //Debug.Log("当前环境温度:" + surroundingTemperature + "   当前体温: " + m_temperature);
            if(photonView.IsMine)
                myUIManager.UpdateTemperature(m_temperature);
        }

    }

    public void changeHunger(int amount)
    {
        if (m_status == PlayerStatus.DEAD)
        {
            return;
        }
        //Debug.Log("玩家当前饥饿值：" + m_hunger + "/" + maxHunger);
        m_hunger = Mathf.Clamp(m_hunger + amount, 0, maxHunger);
            Debug.Log("玩家当前饥饿值：" + m_hunger + "/" + maxHunger);
        if(photonView.IsMine)
            myUIManager.UpdateHunger(m_hunger);
    }

    public void ChangeHealth(int amount)
    {
        if (m_status == PlayerStatus.DEAD)
        {
            return;
        }
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
        if(photonView.IsMine)
            myUIManager.UpdateHealth(m_health);
    }

    void Attack()
    {
        if (m_status == PlayerStatus.DEAD)
        {
            return;
        }
        if (!photonView.IsMine)
        {
            return;
        }
        AudioManager.instance.PlaySound("punch");
        photonView.RPC("RpcAttack", RpcTarget.AllBuffered, MeleePosition.position);

    }

    [PunRPC]
    void RpcAttack(Vector3 position)
    {

        GameObject bullet = Instantiate(bulletObj, position, Quaternion.identity);
        Bullet Bc = bullet.GetComponent<Bullet>();
        if (Bc != null)
        {
            Bc.BulletMove(lookDeriction, 300);
        }
    }

    private void OnDeath()
    {
        print("You are Dead!!!!");
        
        //disable movement
        APcontroller.enabled = false;
        //disable input

        photonView.RPC("RpcOnDeath", RpcTarget.AllBuffered);
        WorldManager.Instance.OnPlayerNumberChange();
        //SceneManager.LoadScene("Menu");
    }

    [PunRPC]
    private void RpcOnDeath()
    {
        m_status = PlayerStatus.DEAD;
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
   

    public bool PickedUp(PickedUpItems item)
    {
        GameResources.PickedUpItemName name = item.getItemName();
        Debug.Log("Pick up " + name.ToString());
        Debug.Log("Bag full: " + bagFull);
        if (currentHolded != null && bagFull)
        {
            Debug.Log("Oops, your bug is full! Can not pick up " + name.ToString());
            return false;
        }


        if (photonView.IsMine)
        {
            if(currentHolded == null)
            {
                HoldItemInHand(item);
            } 
            //put in bag
            if (myBackpack.AddNewItem(item))
            {
                bagFull = myBackpack.ItemSpaceLeft() <= 0;
                Debug.Log("Sucessfully put in bag: now space left " + myBackpack.ItemSpaceLeft());
            } else
            {
                Debug.LogError("BAG FULL???????");
                return false;
            }
        } else
        {
            //not local player, only check hold in hand
            if (currentHolded == null)
            {
                HoldItemInHand(item);
            }
        }
        item.m_State = PickedUpItems.ItemState.IN_BAG;
        item.gameObject.SetActive(false);
        return true;
    }

    public void HoldItemInHand(PickedUpItems item)
    {
        //if (item.m_State == PickedUpItems.ItemState.IN_BAG)
        //{
            //hold in hand(change UI?)
            item.m_State = PickedUpItems.ItemState.IN_HAND;
            currentHolded = item;
            
            
            Debug.Log("Hold Item: "+ item.getItemName());


            string HoldedItemID = item.getItemName().ToString();
            Debug.Log("Hold Item: Item exist.");
            photonView.RPC("RpcChangeHoldItemSprite", RpcTarget.AllBuffered, HoldedItemID, m_ID);


            //item.transform.SetParent(transform);
            //item.transform.position = HoldedPosition.position;
        //}
    }
    
    [PunRPC]
    public void RpcChangeHoldItemSprite(string ItemName, int PlayerID)
    {
        PlayerComponent[] playerList = GetComponentsInParent<PlayerComponent>();
        foreach(PlayerComponent player in playerList)
        {
            if(player.m_ID == PlayerID)
            {
                if(ItemName == "")
                {
                    HoldedItemSprite.sprite = null;
                }
                else
                {
                    string spriteName = "InventoryUIs/" + ItemName.ToString();
                    HoldedItemSprite.sprite = Resources.Load<Sprite>(spriteName);
                }
                
            }
        }
       
    }

    //Use this function to get whats in hand
    public PickedUpItems GetWhatsInHand()
    {
        return currentHolded;
    }



    public void useItemInHand()
    {
        //if Food:
        //if(currentHolded.getItemName() == PickedUpItemName.FRUIT)
        //{
        //    //ChangeHealth();
        if(currentHolded == null)
        {
            return;
        }

        currentHolded.m_State = PickedUpItems.ItemState.IN_BAG;

        if (currentHolded.getItemName() == PickedUpItemName.DRIED_FISH)
        {
            changeHunger(10);
        }
        else if (currentHolded.getItemName() == PickedUpItemName.FRUIT)
        {
            changeHunger(10);
        }
        else if (currentHolded.getItemName() == PickedUpItemName.POOPOO)
        {
            changeHunger(10);
        }

        // TODO

        GameResources.PickedUpItemName name = currentHolded.getItemName();
        myBackpack.PopItem(currentHolded);
        Destroy(currentHolded.gameObject);

        myBackpack.LetItemInHandByName(name);

        if (!myBackpack.DoIHave(name)) {
            photonView.RPC("RpcChangeHoldItemSprite", RpcTarget.AllBuffered, "", m_ID);
        }

    }

    //Change player's temperature based on surrounding temperature.
    //if surrounding temperature > 28, m_temperature +0.1/s
    //if surrounding temperature < 10, m_temperature -0.1/s
    public void changeZone(int surroundingTemperature, LandType land)
    {
        this.surroundingTemperature = surroundingTemperature;
        if (photonView.IsMine)
        {
            CameraFollow.Instance.changeBg(land);
        }
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
        if (stream.IsWriting)
        {
            stream.SendNext(bagFull);
            stream.SendNext(m_health);
        }
        else
        {
            bagFull = (bool)stream.ReceiveNext();
            m_health = (int)stream.ReceiveNext();
        }
    }
}
