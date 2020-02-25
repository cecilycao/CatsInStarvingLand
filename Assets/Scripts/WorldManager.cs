using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public int day;
    public int totalSeconds;
    public int[,] m_map;

    public WorldGenerator m_worldGenerator;

    // Start is called before the first frame update
    void Start()
    {
        m_worldGenerator.GenerateLands();

        m_map = m_worldGenerator.getInitialMap();


    }

    public void UpdateTileMap(Vector2Int index, int val)
    {
        m_map[index.x, index.y] = val;
        m_worldGenerator.FillWithTileType(val, index.x, index.y);
    }

}
