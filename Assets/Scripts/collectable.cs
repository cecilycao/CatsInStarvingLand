using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class collectable : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerEnter2D(Collider2D other){
         PlayerComponent pc = other.GetComponent< PlayerComponent>();
         
         
        if(pc!=null){
            // if(pc.myCurrentHealth<pc.myMaxHealth){
            //     // pc.ChangeHealth(1);
            //     Destroy(this.gameObject);
            // }
            Destroy(this.gameObject);
             Debug.Log("shiqu");
            //  pc.PickedUp(other);

           
        }
    }
}
