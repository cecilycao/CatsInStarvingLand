using System.Collections.Generic;
using UnityEngine;


public class Backpack
{
    private int maxCapacity = 100;
    private int maxItemSpace = 12;
    private int currentWeight = 0;

    private Dictionary<Resources.PickedUpItemName, int> inventory = new Dictionary<Resources.PickedUpItemName, int>();
    public Backpack(Inventory ivControllor)
    {
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

    public bool AddNewItem(Resources.PickedUpItemName newItem)
    {
        int weight = 1;
        if (weight > this.CapacityLeft())
        {
            return false;
        }

        int tmpCount;
        if (inventory.TryGetValue(newItem, out tmpCount))
        {
            inventory.Add(newItem, tmpCount + 1);
        }
        else
        {
            if (ItemSpaceLeft() == 0)
            {
                return false;
            }
            inventory.Add(newItem, 1);
        }
        currentWeight += weight;
        return true;
    }

    public bool PopItem(Resources.PickedUpItemName item)
    {
        int weight = 1;
        int tmpCount;
        if (inventory.TryGetValue(item, out tmpCount))
        {
            if (tmpCount - 1 > 0)
            {
                inventory.Add(item, tmpCount - 1);
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
        return true;
    }

    public bool DoIHave(Resources.PickedUpItemName item)
    {
        return inventory.ContainsKey(item);
    }

    public Dictionary<Resources.PickedUpItemName, int> WhatsInBackpack()
    {
        return inventory;
    }
}
