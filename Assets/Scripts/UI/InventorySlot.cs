﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Text ItemCountText;
    public Image ItemUIImage;

    // Start is called before the first frame update
    void Start()
    {

    }

    public bool updateItem(GameResources.PickedUpItemName ItemName, int count)
    {
        ItemCountText.text = count.ToString();
        Debug.Log(count.ToString());
        //find Image with ItemName
        ItemUIImage.sprite = findImageWithItemName(ItemName);
        return true;
    }

    Sprite findImageWithItemName(GameResources.PickedUpItemName ItemName)
    {
        //Resources.load();
        //Load Sprite From The Resources Folder and use
        string spriteName = "InventoryUIs/" + (int)ItemName;
        Sprite sp = Resources.Load(spriteName) as Sprite;

        return sp;
    }
}
