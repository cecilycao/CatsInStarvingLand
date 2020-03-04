using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerForNetwork : MonoBehaviourPunCallbacks
{

    public PlayerComponent PlayerPrefab;

    public PlayerComponent LocalPlayer;

    private void Awake()
    {
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene("Menu");
            return;
        }
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
