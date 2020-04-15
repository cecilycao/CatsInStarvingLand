using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameResources;

public class Poopoo : PickedUpItems
{
    //PlayerComponent m_player;
    // Start is called before the first frame update

    private int happenSecond;
    public int slimeCreateDuration = 10;


    public GameObject slime;
    //public GameResources.PickedUpItemName tplant;

    

    //public bool createSlime;

    void Start()
    {
        //m_player = GameObject.FindWithTag("Player").GetComponent<PlayerComponent>();

        //createSlime = false;
        happenSecond = (int)WorldManager.Instance.getCurrentSecond();
    }

    // Update is called once per frame
    void Update()
    {
        //if (!isSlime)
        //{

        //    if (WorldManager.Instance.getCurrentSecond() - happenSecond >= 30)
        //    {
        //        //Destroy(this.gameObject);
        //        //GameObject xiaoslime = Instantiate(slime, transform.position, transform.rotation);
        //        Debug.Log("biansheng!!!!!!!!!!!!!!!!!!");
        //        happenSecond = (int)WorldManager.Instance.getCurrentSecond() + 1;
        //    }
        //    isSlime = true;
        //}
        if (WorldManager.Instance.getCurrentSecond() - happenSecond >= slimeCreateDuration && m_State == ItemState.DEFAULT)
        {
            Destroy(gameObject);
            if (PhotonNetwork.IsMasterClient)
            {
                CreateSlime();
            }
            
        }
           
        
    }


    public void CreateSlime()
    {
        //GameObject newSlime = Instantiate(slime);
        //newSlime.transform.position = transform.position;
        PhotonNetwork.InstantiateSceneObject(slime.name, transform.position, transform.rotation);

            //PhotonNetwork.Destroy(this.gameObject);
    }

    public override PickedUpItemName getItemName()
    {
        return PickedUpItemName.POOPOO;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player" && m_State == ItemState.DEFAULT)
        {
            Debug.Log("Pick me!");
            PlayerComponent current_player = other.GetComponent<PlayerComponent>();
            current_player.PickedUp(this);
        }
    }
    public override GameObject getGameObj()
    {
        return gameObject;
    }
}
