using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SurviveMessage : MonoBehaviour
{
    public Text Message;
    public Button ReturnToMenuButton;
    // Start is called before the first frame update
    void Start()
    {
        Message.text = "Survive Days: " + WorldManager.Instance.currentDay;
        ReturnToMenuButton.onClick.AddListener(ReturnToMenu);
    }

    public void ReturnToMenu()
    {
        Debug.Log("Click return to Menu...");
        GameManagerForNetwork.Instance.ReturnToLobby();
    }
}
