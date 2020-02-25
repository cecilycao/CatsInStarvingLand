/* Copyright (c) 2014 Advanced Platformer 2D */

using UnityEngine;

[System.Serializable]
public class APInputConstrainedAxis
{
	////////////////////////////////////////////////////////
	// PUBLIC/HIGH LEVEL	
	public string m_name;					// name of axis in project input settings
	public Compare m_compare;
	public float m_value;

    public enum Compare
    {
        Equal,
        NotEqual,
        LessThan,
        MoreThan,
        LessOrEqualThan,
        MoreOrEqualThan
    }

    public bool IsConstraintValid()
    {
        if (!string.IsNullOrEmpty(m_name))
        {
            float fCurAxisValue = Input.GetAxisRaw(m_name);
            switch (m_compare)
            {
                case Compare.Equal: return fCurAxisValue == m_value;
                case Compare.NotEqual: return fCurAxisValue != m_value;
                case Compare.LessThan: return fCurAxisValue < m_value;
                case Compare.MoreThan: return fCurAxisValue > m_value;
                case Compare.LessOrEqualThan: return fCurAxisValue <= m_value;
                case Compare.MoreOrEqualThan: return fCurAxisValue >= m_value;
            }
        }
        return true;
    }

}

