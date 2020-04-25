using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameResources;

public abstract class Claw : PickedUpItems
{
    public int damage;
    public bool touch = false;
    public Collider2D target;


    public void OnTriggerEnter2D(Collider2D collision)
    {
        touch = true;
        target = collision;
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        touch = false;
        target = null;
    }

    public void Attack()
    {
        if (touch)
        {
            if (target.gameObject.tag == "animal")
            {
                print("Hit animal");
                EnemyControler Ec = target.gameObject.GetComponent<EnemyControler>();
                if (Ec != null)
                {
                    Ec.animalChangeHealth(damage);
                } else
                {
                    Debug.LogError("Animal doesn't have script");
                }

            }
            else if (target.gameObject.tag == "slime")
            {
                slime sl = target.gameObject.GetComponent<slime>();
                sl.slimeChangeHealth(damage);
                Debug.Log("Hit Slime");
            }
            Debug.Log("Damage: " + damage);
            
        }
    }
}
