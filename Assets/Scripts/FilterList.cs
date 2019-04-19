using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FilterList : ScriptableObject
{
    public Filter[] list;
    public int Length
    {
        get
        {
            return list == null ? 0 : list.Length;
        }
    }
}

[System.Serializable]
public class Filter
{
    public enum SelectionType
    {
        Highest,
        Lowest
    }

    public SelectionType selectionType;
    public DerivedStat derivedProperty;
}
