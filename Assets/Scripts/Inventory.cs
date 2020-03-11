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
    void Start()
    {
        backpack.Clear();
        slotList = GetComponentsInChildren<InventorySlot>();
        if (slotList.Length != 12)
        {
            Debug.Log("Everything are fucked up");
        }
    }

    private bool UpdateOne(int slotID, GameResources.PickedUpItemName item, int count)
    {
        if (slotID >= 12)
        {
            Debug.Log("FUCK you");
        }
        return slotList[slotID].updateItem(item, count);
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
        Debug.Log("准备放入背包");
        int weight = 1;
        if (weight > this.CapacityLeft())
        {
            Debug.Log("放入背包结果1");
            return false;
        }

        int tmpCount;
        if (backpack.TryGetValue(newItem, out tmpCount))
        {
            backpack[newItem] = tmpCount + 1;
        }
        else
        {
            if (ItemSpaceLeft() == 0)
            {
                Debug.Log("放入背包结果2");
                return false;
            }
            backpack[newItem] = 1;
        }
        updateAllSlot();
        currentWeight += weight;
        Debug.Log("放入背包结果3");
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
            }
        }
        else
        {
            return false;
        }
        currentWeight -= weight;
        updateAllSlot();
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

    void updateAllSlot()
    {
        int count = 0;
        foreach (var item in backpack)
        {
            Debug.Log("背包更新: " + item.Key + "=" + item.Value);
            this.UpdateOne(count, item.Key, item.Value);
            count++;
        }
    }
}
