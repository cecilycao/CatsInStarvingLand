using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SuccessMessage : MonoBehaviour
{
    public Button ReturnToMenuButton;

    // Start is called before the first frame update
    void Start()
    {
        ReturnToMenuButton.onClick.AddListener(ReturnToMenu);
    }

    public void ReturnToMenu()
    {
        Debug.Log("Click return to Menu...");
        GameManagerForNetwork.Instance.ReturnToLobby();
    }
}
