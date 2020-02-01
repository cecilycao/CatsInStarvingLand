using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LandBrick : MonoBehaviour
{
    
    // Start is called before the first frame update
    void Start()
    {

        //alterLand.onClick.AddListener(AlterLand);
    }

    void OnMouseDown()
    {
        if(true/*has pickaxe or nothing in hand */)
        {
            print("clicked a land brick");
            /*new a land fragment for pick up*/
            Destroy(gameObject);
        }
    }


}
