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

    public bool updateItem(Resources.PickedUpItemName ItemName, int count)
    {
        ItemCountText.text = count.ToString();
        Debug.Log(count.ToString());
        //find Image with ItemName
        ItemUIImage = findImageWithItemName(ItemName);
        return true;
    }

    Image findImageWithItemName(Resources.PickedUpItemName ItemName)
    {
        //Resources.load();
        return null;
    }
}
