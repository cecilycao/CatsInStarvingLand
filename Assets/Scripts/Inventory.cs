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

    private PlayerComponent myPlayer;

    List<PickedUpItems> instanceList = new List<PickedUpItems>();

    private Dictionary<GameResources.PickedUpItemName, int> backpack = new Dictionary<GameResources.PickedUpItemName, int>();
    private Dictionary<GameResources.PickedUpItemName, int> itemToSlotIndex = new Dictionary<GameResources.PickedUpItemName, int>();
    private Dictionary<int, GameResources.PickedUpItemName> slotIndexToItem = new Dictionary<int, GameResources.PickedUpItemName>();



    private Dictionary<int, int> hashID2IndexID = new Dictionary<int, int>();

    void Start()
    {
        backpack.Clear();

        myPlayer = GameManagerForNetwork.Instance.LocalPlayer;

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

    //Show Item at index in the hand.
    public void WhatItemAtThisIndex(int index)
    {
        myPlayer = GameManagerForNetwork.Instance.LocalPlayer;

        GameResources.PickedUpItemName tmp = GameResources.PickedUpItemName.DEFAULT;
        slotIndexToItem.TryGetValue(index, out tmp);

        if (tmp != GameResources.PickedUpItemName.DEFAULT)
        {
            foreach (var el in instanceList)
            {
                el.m_State = PickedUpItems.ItemState.IN_BAG;
                if (el.getItemName() == tmp)
                {
                    //should excute on all clients
                    myPlayer.HoldItemInHand(el);

                    Debug.Log(el.getItemName());
                    Debug.Log("CLICKED");
                }
            }
        } else {
            myPlayer.HandItemBack();
        }
    }

    //Pre: nothing holded in hand
    //post: hold item with given name in hand
    public void LetItemInHandByName(GameResources.PickedUpItemName itemName)
    {
        if (itemToSlotIndex.ContainsKey(itemName)) {
            int slotIndex = itemToSlotIndex[itemName];
            WhatItemAtThisIndex(slotIndex);
        }
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

    // public bool AddNewItem(GameResources.PickedUpItemName newItem)
    // {
    //     int weight = 1;

    //     if (weight > this.CapacityLeft())
    //     {
    //         return false;
    //     }

    //     int tmpCount;
    //     if (backpack.TryGetValue(newItem, out tmpCount))
    //     {
    //         backpack[newItem] = tmpCount + 1;
    //         slotList[itemToSlotIndex[newItem]].updateItem(newItem, tmpCount + 1);
    //     }
    //     else
    //     {
    //         if (ItemSpaceLeft() == 0)
    //         {
    //             return false;
    //         }


    //         for (int i = 0; i < 12; i++)
    //         {
    //             if (!slotIndexToItem.ContainsKey(i))
    //             {

    //                 slotIndexToItem[i] = newItem;
    //                 itemToSlotIndex[newItem] = i;

    //                 slotList[i].updateItem(newItem, 1);

    //                 break;
    //             }
    //         }

    //         backpack[newItem] = 1;
    //     }

    //     currentWeight += weight;
    //     return true;
    // }


    public bool AddNewItem(PickedUpItems newItem)
    {

        GameResources.PickedUpItemName itemName = newItem.getItemName();


        int weight = 1;

        if (weight > this.CapacityLeft())
        {
            return false;
        }

        int tmpCount;
        if (backpack.TryGetValue(itemName, out tmpCount))
        {
            backpack[itemName] = tmpCount + 1;
            slotList[itemToSlotIndex[itemName]].updateItem(itemName, tmpCount + 1);
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
                    slotIndexToItem[i] = itemName;
                    itemToSlotIndex[itemName] = i;

                    slotList[i].updateItem(itemName, 1);

                    break;
                }
            }

            backpack[itemName] = 1;
        }

        instanceList.Add(newItem);

        currentWeight += weight;
        return true;
    }

    public bool PopItem(GameResources.PickedUpItemName ItemName)
    {
        PickedUpItems thisItem = null;
        foreach(PickedUpItems item in instanceList)
        {
            if (item.getItemName() == ItemName)
            {
                thisItem = item;
            }
        }
        if(thisItem == null)
        {
            Debug.LogError("Didn't find item in instance list");
            return false;
        } else
        {
            return PopItem(thisItem);
        }
    }

    public bool PopItem(PickedUpItems newItem)
    {
        GameResources.PickedUpItemName itemName = newItem.getItemName();
        int weight = 1;

        Debug.Log("POP " + itemName);

        int tmpCount;

        instanceList.Remove(newItem);

        if (backpack.TryGetValue(itemName, out tmpCount))
        {
            int index = itemToSlotIndex[itemName];

            if (tmpCount - 1 > 0)
            {
                backpack[itemName] = tmpCount - 1;

                slotList[index].updateItem(itemName, tmpCount - 1);
            }
            else
            {
                slotList[index].updateItem(GameResources.PickedUpItemName.DEFAULT, 0);

                backpack.Remove(itemName);
                slotIndexToItem.Remove(index);
                itemToSlotIndex.Remove(itemName);
                myPlayer = GameManagerForNetwork.Instance.LocalPlayer;
                if (myPlayer.currentHolded == newItem)
                {
                    myPlayer.DestroyHoldedItem();
                }
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

    public int HowManyDoIHave(GameResources.PickedUpItemName item)
    {
        if (backpack.ContainsKey(item))
        {
            return backpack[item];
        } else
        {
            return 0;
        }
    }

    public Dictionary<GameResources.PickedUpItemName, int> WhatsInBackpack()
    {
        return backpack;
    }

}
