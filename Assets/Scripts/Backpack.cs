using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Backpack : MonoBehaviour
{
    public GameObject m_player;
    public int maxCapacity = 100;

    int currentWeight = 0;

    private List<Resources.PickedUpItemName> inventory;
    void Start()
    {
        inventory.Clear();
    }

    public bool IsEmpty()
    {
        return currentWeight == 0;
    }

    public int CapacityLeft()
    {
        return maxCapacity - currentWeight;
    }

    public int CurrentOccupied()
    {
        return currentWeight;
    }

    public bool UpdateCapacity(int to)
    {
        if (CurrentOccupied() > to)
        {
            return false;
        }
        maxCapacity = to;
        return true;
    }

    public bool AddNewItem(Resources.PickedUpItemName item)
    {
        int weight = 1;
        if (weight <= this.CapacityLeft())
        {
            return false;
        }
        inventory.Add(item);
        currentWeight += weight;
        return true;
    }

    public bool PopItem(Resources.PickedUpItemName item)
    {
        int weight = 1;
        bool prs = inventory.Remove(item);
        if (prs)
        {
            currentWeight -= weight;
        }
        return prs;
    }

    public bool DoIHave(Resources.PickedUpItemName item)
    {
        return inventory.Contains(item);
    }

    public Dictionary<Resources.PickedUpItemName, int> WhatsInBackpack()
    {
        var sum = new Dictionary<Resources.PickedUpItemName, int>();
        int tmpCount;
        foreach (var eachItem in inventory)
        {
            if (sum.TryGetValue(eachItem, out tmpCount))
            {
                sum.Add(eachItem, tmpCount + 1);
            }
            else
            {
                sum.Add(eachItem, 1);
            }

        }
        return sum;
    }

}
