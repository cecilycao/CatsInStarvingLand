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

    public void updateItem(Resources.PickedUpItemName ItemName, int count)
    {
        ItemCountText.text = count.ToString();
        //find Image with ItemName
        ItemUIImage = findImageWithItemName(ItemName);
    }

    Image findImageWithItemName(Resources.PickedUpItemName ItemName)
    {
        
        //Resources.load();
        return null;
    }
}
