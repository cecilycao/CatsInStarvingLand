using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{

    public static int LengthOfDayInSecond = 30;

    private static WorldManager instance = null;
    public static WorldManager Instance { get { return instance; } }
    

    public float currentSecond = 0;
    public int currentDay = 0;

    float startTime = -1f;

    public int[,] m_map;

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
        m_worldGenerator.GenerateWorld();

        m_map = m_worldGenerator.getInitialMap();

        beginWorldTime();
    }

    private void Update()
    {
        if(startTime != -1)
        {
            //in seconds
            currentSecond = Time.time - startTime;
            currentDay = ((int)currentSecond / 30) + 1;
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

    public bool UpdateTileMap(Vector2Int index, int val)
    {
        if (!checkEmptyAround(index.x, index.y))
        {
            m_map[index.x, index.y] = val;
            m_worldGenerator.FillWithTileType(val, index.x, index.y);
            return true;
        }
        return false;
    }

    //Buggy!!!!!
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
}
