using System.Collections;
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
        ItemUIImage = findImageWithItemName(ItemName);
        return true;
    }

    Image findImageWithItemName(GameResources.PickedUpItemName ItemName)
    {
        //Resources.load();
        //Load Sprite From The Resources Folder and use
        var sp = Resources.Load("SpriteFolder/abc") as Sprite;
        Sprite sp  = Resources.Load("path") as Sprite;
        return null;
    }
}
