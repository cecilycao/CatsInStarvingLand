using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    private GameObject[] objList;
    private InventorySlot[] slotList;
    // Start is called before the first frame update
    void Start()
    {
        slotList = new InventorySlot[12];

        Debug.Log("Inventory Starts!");
        objList = GameObject.FindGameObjectsWithTag("InventorySlot");
        if (objList.Length != 12)
        {
            Debug.Log("Everything are fucked up");
        }
        for (int i = 0; i < objList.Length; i++)
        {
            slotList[i] = objList[i].GetComponent<InventorySlot>();
        }
    }

    public bool UpdateOne(int slotID, Resources.PickedUpItemName item, int count)
    {
        if (slotID >= 12)
        {
            Debug.Log("FUCK you");
        }
        return slotList[slotID].updateItem(item, count);
    }
}
