using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviourPun, IPunObservable
{

    public static int LengthOfDayInSecond = 30;

    private static WorldManager instance = null;
    public static WorldManager Instance { get { return instance; } }

    public float currentSecond = 0;
    public int currentDay = 0;
    
    float startTime = -1f;

    public int[,] m_map;
    public bool EndGame = false;

    public WorldGenerator m_worldGenerator;

    // Start is called before the first frame update

    void Awake()
    {
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

    void Start()
    {
        m_worldGenerator = FindObjectOfType<WorldGenerator>();
        m_worldGenerator.GenerateWorld();

        m_map = m_worldGenerator.getInitialMap();

        beginWorldTime();
    }

    private void Update()
    {
        if (photonView.IsMine && !EndGame)
        {
            if (startTime != -1)
            {
                //in seconds
                currentSecond = Time.time - startTime;
                currentDay = ((int)currentSecond / 30) + 1;
            }
        }

    }

    public void OnPlayerNumberChange()
    {
        photonView.RPC("RpcOnPlayerNumberChange", RpcTarget.MasterClient);
    }

    [PunRPC]
    public void RpcOnPlayerNumberChange()
    {
        int count = 0;
        PlayerComponent[] players = FindObjectsOfType<PlayerComponent>();
        foreach (PlayerComponent player in players)
        {
            if (player.m_status != PlayerComponent.PlayerStatus.DEAD)
            {
                count++;
            }
        }
        Debug.Log("Current Alive Number: " + count);
        if (count == 0)
        {
            Debug.Log("No One Alive.");
            //stop time
            EndGame = true;

            //load End Scene
            GameManagerForNetwork.Instance.LoadEndScene();
        }
    }

    public int getCurrentDay()
    {
        return currentDay;
    }

    public float getCurrentSecond()
    {
        return currentSecond;
    }

    void beginWorldTime()
    {
        startTime = Time.time;
    }

    void pauseWorldTime()
    {

    }

    public bool Plant(Vector2Int index, int obj)
    {
        if(m_worldGenerator.PlantGenerationConditions(index.x, index.y))
        {
            photonView.RPC("RpcPlant", RpcTarget.MasterClient, index.x, index.y, obj);
            return true;
        }
        return false;
    }

    [PunRPC]
    public void RpcPlant(int x, int y, int obj)
    {
        m_worldGenerator.GeneratePlant(obj, x, y, false);
    }

    public bool UpdateTileMap(Vector2Int index, int val)
    {
        print("update tile map (" + index.x + ", " + index.y + ")" + " = " + val);
        if(val == 0)
        {
            //this is a landtile, change it to an empty tile
            photonView.RPC("RpcUpdateTileMap", RpcTarget.AllBuffered, index.x, index.y, val);
            return true;
        }
        else if (!checkEmptyAround(index.x, index.y))
        {
            print("can put tile!!");
            //this is an empty tile, put a tile on it
            photonView.RPC("RpcUpdateTileMap", RpcTarget.AllBuffered, index.x, index.y, val);
            return true;
        }
        return false;
    }

    [PunRPC]
    public void RpcUpdateTileMap(int x, int y, int val)
    {
        //currently it is a LandBrick, crack the landbrick
        if(m_map[x, y] != 0)
        {
            LandBrick tile = (LandBrick)m_worldGenerator.getTileComponent(x, y);
            tile.isCracked = true;
            tile.crackALandTile();
        }

        //print("update tile map with value: " + val);
        m_map[x, y] = val;
        m_worldGenerator.FillWithTileType(val, x, y);
    }

    public void clearObjectAt(int x, int y)
    {
        EmptyTile tile = (EmptyTile)m_worldGenerator.getTileComponent(x, y);
        tile.occupied = false;
    }


    bool checkEmptyAround(int x, int y)
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                int checkX = x + i;
                int checkY = y + j;
                if ((i == 0 && j != 0) || (j == 0 && i != 0) && checkX > 0 && checkY > 0 && checkX < m_map.GetLength(0) && checkY < m_map.GetLength(1))
                {
                    if (m_map[checkX, checkY] != 0)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(currentSecond);
            stream.SendNext(currentDay);

        } else
        {
            currentSecond = (float)stream.ReceiveNext();
            currentDay = (int)stream.ReceiveNext();
        }
    }
}
