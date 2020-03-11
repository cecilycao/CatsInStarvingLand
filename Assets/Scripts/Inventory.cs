using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    private GameObject[] objList;
    private InventorySlot[] slotList;
    // Start is called before the first frame update

    private int maxCapacity = 10000;
    private int maxItemSpace = 12;
    private int currentWeight = 0;


    private Dictionary<GameResources.PickedUpItemName, int> backpack = new Dictionary<GameResources.PickedUpItemName, int>();

    private Dictionary<GameResources.PickedUpItemName, int> itemToSlotIndex = new Dictionary<GameResources.PickedUpItemName, int>();
    private Dictionary<int, GameResources.PickedUpItemName> slotIndexToItem = new Dictionary<int, GameResources.PickedUpItemName>();



    private Dictionary<int, int> hashID2IndexID = new Dictionary<int, int>();

    void Start()
    {
        backpack.Clear();
        slotList = GetComponentsInChildren<InventorySlot>();
        if (slotList.Length != 12)
        {
            Debug.Log("Everything are fucked up");
        }

        for (int i = 0; i < 12; i++)
        {
            hashID2IndexID[slotList[i].GetHashCode()] = i;
        }
    }

    public int slotGetIndexID(int hashCode)
    {
        return hashID2IndexID[hashCode];
    }

    public GameResources.PickedUpItemName WhatItemAtThisIndex(int index)
    {
        GameResources.PickedUpItemName tmp = GameResources.PickedUpItemName.DEFAULT;
        slotIndexToItem.TryGetValue(index, out tmp);
        return tmp;
    }

    public bool IsEmpty()
    {
        return backpack.Count == 0;
    }

    public int CapacityLeft()
    {
        return maxCapacity - currentWeight;
    }

    public int ItemSpaceLeft()
    {
        return maxItemSpace - backpack.Count;
    }

    public int CurrentCapacityOccupied()
    {
        return currentWeight;
    }

    public int CurrentItemSpaceOccupied()
    {
        return backpack.Count;
    }

    public bool UpdateCapacity(int to)
    {
        if (CurrentCapacityOccupied() > to)
        {
            return false;
        }
        maxCapacity = to;
        return true;
    }

    public bool AddNewItem(GameResources.PickedUpItemName newItem)
    {
        int weight = 1;

        if (weight > this.CapacityLeft())
        {
            return false;
        }

        int tmpCount;
        if (backpack.TryGetValue(newItem, out tmpCount))
        {
            backpack[newItem] = tmpCount + 1;
            slotList[itemToSlotIndex[newItem]].updateItem(newItem, tmpCount + 1);
        }
        else
        {
            if (ItemSpaceLeft() == 0)
            {
                return false;
            }


            for (int i = 0; i < 12; i++)
            {
                if (!slotIndexToItem.ContainsKey(i))
                {

                    slotIndexToItem[i] = newItem;
                    itemToSlotIndex[newItem] = i;
                    
                    slotList[i].updateItem(newItem, 1);
                    
                    break;
                }
            }

            backpack[newItem] = 1;
        }

        currentWeight += weight;
        return true;
    }

    public bool PopItem(GameResources.PickedUpItemName item)
    {
        int weight = 1;
        int tmpCount;
        if (backpack.TryGetValue(item, out tmpCount))
        {
            if (tmpCount - 1 > 0)
            {
                backpack[item] = tmpCount - 1;
            }
            else
            {
                backpack.Remove(item);

                int index = itemToSlotIndex[item];
                slotIndexToItem.Remove(index);
                itemToSlotIndex.Remove(item);
            }
        }
        else
        {
            return false;
        }
        currentWeight -= weight;
        return true;
    }

    public bool DoIHave(GameResources.PickedUpItemName item)
    {
        return backpack.ContainsKey(item);
    }

    public Dictionary<GameResources.PickedUpItemName, int> WhatsInBackpack()
    {
        return backpack;
    }

}
