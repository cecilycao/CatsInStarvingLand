using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TechnologyUI : MonoBehaviour
{
    public Button WealponUI;
    public Button ToolUI;
    public Button LightingUI;
    public Button ClothsUI;
    public Button FurnitureUI;

    public GameObject WealponList;
    public GameObject ToolList;
    public GameObject LightingList;
    public GameObject ClothsList;
    public GameObject FurnitureList;

    public GameObject currentShowingList;
    public GameObject currentShowingDescription;

    // Start is called before the first frame update
    void Start()
    {
        WealponUI.onClick.AddListener(showWealponList);
        ToolUI.onClick.AddListener(showToolList);
        LightingUI.onClick.AddListener(showLightingList);
        ClothsUI.onClick.AddListener(showClothsList);
        FurnitureUI.onClick.AddListener(showFrunitureList);
    }

    void closeCurrentShowingList()
    {
        if(currentShowingList != null)
        {
            currentShowingList.SetActive(false);
            currentShowingList = null;
        }
        CloseCurrentDescription();
    }

    void showWealponList()
    {
        if (currentShowingList == WealponList)
        {
            closeCurrentShowingList();
        }
        else
        {
            closeCurrentShowingList();
            WealponList.SetActive(true);
            currentShowingList = WealponList;
        }
    }
    void showToolList()
    {
        if (currentShowingList == ToolList)
        {
            closeCurrentShowingList();
        }
        else
        {
            closeCurrentShowingList();
            ToolList.SetActive(true);
            currentShowingList = ToolList;
        }
    }
    void showLightingList()
    {
        if (currentShowingList == LightingList)
        {
            closeCurrentShowingList();
        }
        else
        {
            closeCurrentShowingList();
            LightingList.SetActive(true);
            currentShowingList = LightingList;
        }
    }
    void showClothsList()
    {
        if (currentShowingList == ClothsList)
        {
            closeCurrentShowingList();
        }
        else
        {
            closeCurrentShowingList();
            ClothsList.SetActive(true);
            currentShowingList = ClothsList;
        }
    }
    void showFrunitureList()
    {
        if (currentShowingList == FurnitureList)
        {
            closeCurrentShowingList();
        }
        else
        {
            closeCurrentShowingList();
            FurnitureList.SetActive(true);
            currentShowingList = FurnitureList;
        }
    }

    public void CloseCurrentDescription()
    {
        if(currentShowingDescription != null)
        {
            currentShowingDescription.SetActive(false);
            currentShowingDescription = null;
        }
    }

    public void SetCurrentDescription(GameObject description)
    {
        currentShowingDescription = description;
    }
}
