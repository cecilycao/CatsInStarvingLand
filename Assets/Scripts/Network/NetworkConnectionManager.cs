using UnityEngine;
using System.Collections;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using UnityEngine.SceneManagement;


public class NetworkConnectionManager : MonoBehaviourPunCallbacks
{
    public static NetworkConnectionManager instance;

    public Text UserNameText;
    public Text JoinRoomNameText;
    public Text CreateRoomName;
    public Text WelcomeText;
    public Text Message;

    public GameObject StartScene;
    public GameObject BtnConnectMaster;
    public GameObject BtnConnectRoom;
    public string MainSceneName = "Prototype";

    public bool TriesToConnectToMaster;
    public bool TriesToConnectToRoom;
    //public bool IsStartGame = false;

    public string UserName;
    public string RoomName;
    // Use this for initialization
    

    private void Awake()
    {
        //Singleton
        Debug.Log("Awake......");
        if(instance != null)
        {
            Debug.Log("More than one exist.....");
            if (instance != this)
            {
                UserName = instance.UserName;
                setUserName();
                Destroy(instance.gameObject);
                Debug.Log("Destroy.......");
            }
        }
        instance = this;
        
        DontDestroyOnLoad(gameObject);

    }

    void Start()
    {
        //DontDestroyOnLoad(this);
        TriesToConnectToMaster = false;
        TriesToConnectToRoom = false;
    }

    // Update is called once per frame
    void Update()
    {
        //if (IsStartGame)
        //{
            if (BtnConnectMaster != null)
                BtnConnectMaster.SetActive(!PhotonNetwork.IsConnected && !TriesToConnectToMaster);
            if (BtnConnectRoom != null)
                BtnConnectRoom.SetActive(PhotonNetwork.IsConnected && !TriesToConnectToMaster/* && !TriesToConnectToRoom*/);
        //}

    }

    public void CloseStartScene()
    {
        StartScene.SetActive(false);
    }

    //log in
    public void OnClickConnectToMaster()
    {
        UserName = UserNameText.text;
        setUserName();
        TriesToConnectToMaster = true;

        //Settings (all optional and only for tutorial purpose)
        PhotonNetwork.OfflineMode = true;           //true would "fake" an online connection
        PhotonNetwork.NickName = UserName;       //to set a player name
        PhotonNetwork.AutomaticallySyncScene = true; //to call PhotonNetwork.LoadLevel()
        PhotonNetwork.GameVersion = "v1";            //only people with the same game version can play together

        //PhotonNetwork.ConnectToMaster(ip,port,appid); //manual connection
        if (!PhotonNetwork.OfflineMode)
            PhotonNetwork.ConnectUsingSettings();           //automatic connection based on the config file in Photon/PhotonUnityNetworking/Resources/PhotonServerSettings.asset

    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        TriesToConnectToMaster = false;
        Debug.Log("Connected to Master!");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        TriesToConnectToMaster = false;
        TriesToConnectToRoom = false;
        Debug.Log("Oh No!!! " + cause);
    }

    public void OnClickConnectToRandomRoom()
    {
        if (!PhotonNetwork.IsConnected)
            return;

        TriesToConnectToRoom = true;
        Message.text = "Tries to connect to a random room...";
        //PhotonNetwork.CreateRoom("Peter's Game 1"); //Create a specific Room - Error: OnCreateRoomFailed
        //PhotonNetwork.JoinRoom("Peter's Game 1");   //Join a specific Room   - Error: OnJoinRoomFailed  
        PhotonNetwork.JoinRandomRoom();               //Join a random Room     - Error: OnJoinRandomRoomFailed  
    }

    public void OnClickCreateRoom()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Message.text = "Not Connected, Try Again";
            return;
        }

        RoomName = CreateRoomName.text;
        TriesToConnectToRoom = true;
        Message.text = "Tries to create room and start game...";
        PhotonNetwork.CreateRoom(RoomName); //Create a specific Room - Error: OnCreateRoomFailed
        //PhotonNetwork.JoinRoom("Peter's Game 1");   //Join a specific Room   - Error: OnJoinRoomFailed  
        //PhotonNetwork.JoinRandomRoom();               //Join a random Room     - Error: OnJoinRandomRoomFailed  
    }

    public void OnClickConnectToRoom()
    {
        if (!PhotonNetwork.IsConnected)
            return;
        RoomName = JoinRoomNameText.text;
        TriesToConnectToRoom = true;
        Message.text = "Tries to connect to a room " + RoomName + " ...";
        //PhotonNetwork.CreateRoom("Peter's Game 1"); //Create a specific Room - Error: OnCreateRoomFailed
        PhotonNetwork.JoinRoom(RoomName);   //Join a specific Room   - Error: OnJoinRoomFailed  
        //PhotonNetwork.JoinRandomRoom();               //Join a random Room     - Error: OnJoinRandomRoomFailed  
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);
        //no room available
        //create a room (null as a name means "does not matter")
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 20 });
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        Debug.Log(message);
        Message.text = "Fail to create a room. ";
        base.OnCreateRoomFailed(returnCode, message);
        TriesToConnectToRoom = false;
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        Debug.Log(message);
        Message.text = "Fail to join room "+ RoomName +" . ";
        base.OnJoinRoomFailed(returnCode, message);
        TriesToConnectToRoom = false;
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        TriesToConnectToRoom = false;


        Debug.Log("Master: " + PhotonNetwork.IsMasterClient + " | Players In Room: " + PhotonNetwork.CurrentRoom.PlayerCount + " | RoomName: " + PhotonNetwork.CurrentRoom.Name + " Region: " + PhotonNetwork.CloudRegion);
        //if(PhotonNetwork.IsMasterClient && SceneManager.GetActiveScene().name != "Network")
        //    PhotonNetwork.LoadLevel("Network");
        if (PhotonNetwork.IsMasterClient && SceneManager.GetActiveScene().name != MainSceneName)
        {
            PhotonNetwork.LoadLevel(MainSceneName);
            //SceneManager.LoadScene(MainSceneName);
        }
            
    }

    public void setUserName()
    {
        WelcomeText.text = "Hi " + UserName + "!";
    }
}

