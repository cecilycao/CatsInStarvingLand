using System.Collections.Generic;
using UnityEngine;


public class Backpack
{
    private int maxCapacity = 10000;
    private int maxItemSpace = 12;
    private int currentWeight = 0;

    private Inventory inventoryUI;

    private Dictionary<GameResources.PickedUpItemName, int> inventory = new Dictionary<GameResources.PickedUpItemName, int>();

    //temp use for demo
    private List<GameResources.PickedUpItemName> itemIndex = new List<GameResources.PickedUpItemName>();
    public Backpack(Inventory ivControllor)
    {
        inventoryUI = ivControllor;
        inventory.Clear();
    }

    public bool IsEmpty()
    {
        return inventory.Count == 0;
    }

    public int CapacityLeft()
    {
        return maxCapacity - currentWeight;
    }

    public int ItemSpaceLeft()
    {
        return maxItemSpace - inventory.Count;
    }

    public int CurrentCapacityOccupied()
    {
        return currentWeight;
    }

    public int CurrentItemSpaceOccupied()
    {
        return inventory.Count;
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
        if (inventory.TryGetValue(newItem, out tmpCount))
        {
            inventory[newItem] = tmpCount + 1;
        }
        else
        {
            if (ItemSpaceLeft() == 0)
            {
                return false;
            }
            inventory[newItem] = 1;
            itemIndex.Add(newItem);
        }
        updateAllSlot();
        currentWeight += weight;
        return true;
    }

    public bool PopItem(GameResources.PickedUpItemName item)
    {
        int weight = 1;
        int tmpCount;
        if (inventory.TryGetValue(item, out tmpCount))
        {
            if (tmpCount - 1 > 0)
            {
                inventory[item] = tmpCount - 1;
            }
            else
            {
                inventory.Remove(item);
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
        return inventory.ContainsKey(item);
    }

    public Dictionary<GameResources.PickedUpItemName, int> WhatsInBackpack()
    {
        return inventory;
    }

    void updateAllSlot()
    {
        int count = 0;
        foreach (var item in inventory)
        {
            Debug.Log("背包更新: "+ item.Key + "=" + item.Value);
            inventoryUI.UpdateOne(count, item.Key, item.Value);
            count++;
        }
    }
}
