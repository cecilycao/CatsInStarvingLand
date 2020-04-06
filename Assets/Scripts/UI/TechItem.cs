using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static GameResources;

[System.Serializable]
public class NeedItem
{
    public PickedUpItemName NeededItemName;
    public int quantity;

}

public class TechItem : MonoBehaviour
{
    public PickedUpItems techItem;
    public GameObject Description;
    public Button itemButton;
    public Button createButton;
    public Text ErrorMessage;

    [SerializeField]
    public NeedItem[] needItems;

    TechnologyUI techUI;
    Inventory m_Inventory;

    // Start is called before the first frame update
    void Start()
    {
        techUI = GetComponentInParent<TechnologyUI>();
        itemButton.onClick.AddListener(ShowDescription);
        DescriptionUI desUI = Description.GetComponent<DescriptionUI>();
        createButton = desUI.CreateButton;
        ErrorMessage = desUI.ErrorMessage;
        createButton.onClick.AddListener(CreateObject);
        m_Inventory = FindObjectOfType<Inventory>();
    }

    void ShowDescription()
    {
        if(techUI.currentShowingDescription == Description)
        {
            techUI.CloseCurrentDescription();
            return;
        }
        techUI.CloseCurrentDescription();
        ErrorMessage.gameObject.SetActive(false);
        Description.SetActive(true);
        techUI.SetCurrentDescription(Description);
    }

    void CloseDescriptipn()
    {
        ErrorMessage.gameObject.SetActive(false);
        Description.SetActive(false);
    }

    void CreateObject()
    {
        //check inventory has needed objects
        bool canCreate = true;
        string MissingItem = "";
        foreach (NeedItem item in needItems)
        {
            if(m_Inventory.HowManyDoIHave(item.NeededItemName) < item.quantity)
            {
                MissingItem = item.NeededItemName.ToString();
                canCreate = false;
            }
        }
        //if has, delete objects from inventory, create new object
        //close description
        if (canCreate)
        {
            GameObject newItem = Instantiate(techItem.gameObject);
            if (m_Inventory.AddNewItem(newItem.GetComponent<PickedUpItems>()))
            {
                foreach (NeedItem item in needItems)
                {
                    for (int i = 0; i < item.quantity; i++)
                    {
                        m_Inventory.PopItem(item.NeededItemName);
                    }
                }
                newItem.SetActive(false);
                techUI.CloseCurrentDescription();
            } else
            {
                ErrorMessage.text = "Bag is Full! can not put item in bag";
                ErrorMessage.gameObject.SetActive(true);
            }
        } else
        {
            //if not, show message
            ErrorMessage.text = "You don't have enough " + MissingItem;
            ErrorMessage.gameObject.SetActive(true);
        }


    }

}
