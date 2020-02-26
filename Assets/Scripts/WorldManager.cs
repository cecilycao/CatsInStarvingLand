using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public static int LengthOfDayInSecond = 300;
    public float currentSecond = 0;
    public int currentDay = 0;

    float startTime = -1f;

    public int[,] m_map;

    public WorldGenerator m_worldGenerator;

    // Start is called before the first frame update
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
            currentDay = ((int)currentSecond / 300) + 1;
        }
    }

    void beginWorldTime()
    {
        startTime = Time.time;
    }

    void pauseWorldTime()
    {

    }

    public void UpdateTileMap(Vector2Int index, int val)
    {
        m_map[index.x, index.y] = val;
        m_worldGenerator.FillWithTileType(val, index.x, index.y);
    }

}
