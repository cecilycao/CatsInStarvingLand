/* Copyright (c) 2014 Advanced Platformer 2D */
using UnityEngine;

[System.Serializable]
public class APUnityLayer
{
    [SerializeField]
    private int m_layerIndex = 0;
    public int LayerIndex
    {
        get { return m_layerIndex; }
    }

    public void Set(int _layerIndex)
    {
        if (_layerIndex >= 0 && _layerIndex < 32)
        {
            m_layerIndex = _layerIndex;
        }
    }

    public int Mask
    {
        get { return 1 << m_layerIndex; }
    }
}
