using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GridData
{
    [System.Serializable]
    public class SerializableCell
    {
        public int x, y;
        public bool buildable;
    }

    public List<SerializableCell> cells = new List<SerializableCell>();
}
