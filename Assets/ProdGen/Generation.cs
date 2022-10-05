using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Generation : MonoBehaviour
{
    [Header ("Main Generation Settings")]
    public List<GameObject> allPrefabs; // Contains all prefabs
    public float cellSize; // Size of each generated cell
    public Vector3 grid;

    [Header ("Additional Generation Settings")]
    public float spawnEvery; // Animation speed
    private float timer; // Timer for animation

    // CELL-BASED VARIABLES 
    private List<GenCellState> cellStateList = new List<GenCellState>();
    private List<GenCellInfo> allCells = new List<GenCellInfo>();

    void Start()
    {
        processPrefab();
        allocatePossiblitySpace();
        timer = spawnEvery;
    }

    private void processPrefab()
    {
        for (int i = 0; i < allPrefabs.Count; i++)
        {
            GameObject cell = allPrefabs[i];

            GenCellState cellState = new GenCellState();
            cellState.prefab = cell;
            cellState.rotationIndex = (int)cell.transform.localEulerAngles.y;
            cellStateList.Add(cellState);
        }
    }

    private void allocatePossiblitySpace()
    {
        for (int i = 0; i <= grid.x; i += (int)cellSize)
        {
            for (int j = 0; j <= grid.z; j += (int)cellSize)
            {
                GenCellInfo cellInfo = new GenCellInfo();
                cellInfo.cellCoordinate = new Vector3(i, 0, j);
                cellInfo.cellUsed = false;
                cellInfo.superPosition.AddRange(cellStateList);
                allCells.Add(cellInfo);
            }
        }
    }

    void Update()
    {
        timer = timer - Time.deltaTime;
        if (timer <= 0)
        {
            timer = spawnEvery;
            getProbableCell();
        }
    }

    // Get the most likely cell and instantiate it.
    private void getProbableCell()
    {
        int lowestCount = allPrefabs.Count * 400;
        GenCellInfo probableCellInfo = new GenCellInfo();
        GenCellState cellState = new GenCellState();
        foreach (GenCellInfo cellInfo in allCells)
        {
            if (!cellInfo.cellUsed && lowestCount > cellInfo.superPosition.Count)
            {
                lowestCount = cellInfo.superPosition.Count;
                probableCellInfo = cellInfo;
            }
        }
        if (lowestCount > 1)
        {
            lowestCount = Random.Range(0, lowestCount);
        }
        cellState = probableCellInfo.superPosition[lowestCount];
        if (probableCellInfo!=null)
        {
            spawn(cellState.prefab, probableCellInfo.cellCoordinate, cellState.rotationIndex);
        }
        foreach (GenCellState probableCellState in probableCellInfo.superPosition.ToArray())
        {
            if (probableCellState != cellState)
            {
                probableCellInfo.superPosition.Remove(probableCellState);
            }
        }
        probableCellInfo.cellUsed = true;
    }

    // Instantiate the tile
    private void spawn(GameObject prefab, Vector3 position, int rotationIndex)
    {
        GameObject cell = GameObject.Instantiate(prefab, position, Quaternion.identity);
        cell.transform.Rotate(0, rotationIndex, 0);
    }
}
