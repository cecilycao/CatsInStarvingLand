using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerForNetwork : MonoBehaviourPunCallbacks
{
    private static GameManagerForNetwork instance = null;
    public static GameManagerForNetwork Instance { get { return instance; } }

    public PlayerComponent PlayerPrefab;

    public PlayerComponent LocalPlayer;

    private void Awake()
    {
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene("Menu");
            return;
        }


        // Singleton
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(gameObject);
        
    }

    private void Start()
    {
        PlayerComponent.RefreshInstance(ref LocalPlayer, PlayerPrefab);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        PlayerComponent.RefreshInstance(ref LocalPlayer, PlayerPrefab);
    }
}
