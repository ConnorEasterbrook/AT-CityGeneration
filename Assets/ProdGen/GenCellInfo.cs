using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GenCellInfo
{
    /// <summary>
    /// Is the cell being used?
    /// </summary>
    public bool cellUsed;
    public Vector3 cellCoordinate;
    public List<GenCellState> superPosition = new List<GenCellState>();
}


