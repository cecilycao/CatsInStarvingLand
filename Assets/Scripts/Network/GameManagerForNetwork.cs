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

    public int LocalPlayerID;

    

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
        LocalPlayerID = LocalPlayer.GetComponent<PhotonView>().ViewID;
        //if (PhotonNetwork.IsMasterClient)
        //{
        //    //instantiate world
        //    PhotonNetwork.Instantiate("Plant", new Vector3(2.49f, -0.03f, 0f), Quaternion.identity, 0);
        //    PhotonNetwork.Instantiate("Poopoo", new Vector3(3.36f, -0.27f, 0f), Quaternion.identity, 0);
        //}
        
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        PlayerComponent.RefreshInstance(ref LocalPlayer, PlayerPrefab);
        LocalPlayerID = LocalPlayer.GetComponent<PhotonView>().ViewID;
    }
}
