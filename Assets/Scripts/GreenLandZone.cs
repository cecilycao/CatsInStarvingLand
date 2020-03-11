using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameResources;

public class GreenLandZone : MonoBehaviour
{
    public int temperature = 20;
    BoxCollider2D m_collider;
    public LandType m_landType = LandType.GREENLAND;

    // Start is called before the first frame update
    void Start()
    {
        m_collider = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            Debug.Log("Hi!!! You are in greenland!!!");
            collision.gameObject.GetComponent<PlayerComponent>().changeZone(temperature, m_landType);
        }
    }
}
