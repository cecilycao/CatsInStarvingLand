using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameResources;

public class Fruit : PickedUpItems
{
    // Start is called before the first frame update

    //饱腹 + 10； 生命 +5
    //腐烂：一段时间后destroy

    //public GameObject fruit;
    //public PlayerComponent m_player;
    //public WorldManager wm;

    public int start_time;

    void Start()
    {
        start_time = WorldManager.Instance.getCurrentDay();
        //m_player = GameObject.FindWithTag("Player").GetComponent<PlayerComponent>();
    }

    // Update is called once per frame
    void Update()
    {
        //m_player.ChangeHealth(5);
        //m_player.changeHunger(10);

        if (WorldManager.Instance.getCurrentDay() - start_time >= 5)
        {
            Destroy(gameObject);
        }
    }



    public override PickedUpItemName getItemName()
    {
        return PickedUpItemName.FRUIT;
    }
}
