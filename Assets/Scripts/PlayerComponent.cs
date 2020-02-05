using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerComponent : MonoBehaviour
{

    public int m_health;
    public int m_hunger;
    public int m_temperature;
    public int m_tiredness;

    // Start is called before the first frame update
    void Start()
    {
        m_health = 100;
        m_hunger = 100;
        m_temperature = 38;
        m_tiredness = 100;

        //temp
        PlayerInitialize();
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


}
