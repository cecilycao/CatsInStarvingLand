using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameResources;

public class Poopoo : PickedUpItems
{
    //PlayerComponent m_player;
    // Start is called before the first frame update
    void Start()
    {
        //m_player = GameObject.FindWithTag("Player").GetComponent<PlayerComponent>();
    }

    // Update is called once per frame
    void Update()
    {
        
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
