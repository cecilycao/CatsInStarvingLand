using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameResources;

public class Poopoo : PickedUpItems
{
    //PlayerComponent m_player;
    // Start is called before the first frame update

    private int happenDay;


    public GameObject slime;
    //public GameResources.PickedUpItemName tplant;

    

    public bool isSlime;

    void Start()
    {
        //m_player = GameObject.FindWithTag("Player").GetComponent<PlayerComponent>();
       
        isSlime = false;
        happenDay = (int)WorldManager.Instance.getCurrentSecond();
    }

    // Update is called once per frame
    void Update()
    {
        //if (!isSlime)
        //{

        //    if (WorldManager.Instance.getCurrentSecond() - happenDay >= 30)
        //    {
        //        //Destroy(this.gameObject);
        //        //GameObject xiaoslime = Instantiate(slime, transform.position, transform.rotation);
        //        Debug.Log("biansheng!!!!!!!!!!!!!!!!!!");
        //        happenDay = (int)WorldManager.Instance.getCurrentSecond() + 1;
        //    }
        //    isSlime = true;
        //}
        if (WorldManager.Instance.getCurrentSecond() - happenDay >= 30)
        {
            if (m_State == ItemState.DEFAULT)
            {
                Destroy(this.gameObject);
                GameObject xiaoslime = Instantiate(slime, transform.position, transform.rotation);
                //Debug.Log("biansheng!!!!!!!!!!!!!!!!!!");
                //happenDay = (int)WorldManager.Instance.getCurrentSecond() +30;
            }
           
        }
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
}
