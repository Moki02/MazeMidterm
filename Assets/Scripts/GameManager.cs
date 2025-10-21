using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridWidth = 15;
    public int gridHeight = 15;
    public float cellSize = 5f;

    [Header("Wall Settings")]
    public int largeCount = 3;
    public int mediumCount = 6;
    public int smallCount = 10;

    public Vector3 largeSize = new Vector3(50f, 3f, 1f);
    public Vector3 mediumSize = new Vector3(30f, 3f, 1f);
    public Vector3 smallSize = new Vector3(10f, 3f, 1f);
    public float spawnY = 1.5f;

    [Header("References")]
    public Transform startLine;
    public Transform finishLine;
    public Transform wallsParent;
    public Material wallMaterial;

    [Header("Debug")]
    public bool drawGrid = true;
    public bool drawPath = true;

    private HashSet<Vector2Int> pathCells = new HashSet<Vector2Int>();
    private List<Vector3> pathPoints = new List<Vector3>();
    private float offsetX, offsetZ;

    void Start()
    {
        if (wallsParent != null)
        {
            for (int i = wallsParent.childCount - 1; i >= 0; i--)
                Destroy(wallsParent.GetChild(i).gameObject);
        }

        offsetX = -((gridWidth - 1) * cellSize) / 2f;
        offsetZ = -((gridHeight - 1) * cellSize) / 2f;

        GeneratePath();
        SpawnWalls(largeCount, largeSize);
        SpawnWalls(mediumCount, mediumSize);
        SpawnWalls(smallCount, smallSize);
    }
    void GeneratePath()
    {
        pathCells.Clear();
        pathPoints.Clear();

        Vector2Int startCell = WorldToGrid(startLine.position);
        Vector2Int finishCell = WorldToGrid(finishLine.position);

        startCell = ClampToGrid(startCell);
        finishCell = ClampToGrid(finishCell);

        Vector2Int current = startCell;
        pathCells.Add(current);


        System.Random rand = new System.Random();

        while (current != finishCell)
        {
            Vector2Int step = Vector2Int.zero;

            if (rand.NextDouble() < 0.5)
            {
                if (current.x < finishCell.x) step = Vector2Int.right;
                else if (current.x > finishCell.x) step = Vector2Int.left;
            }
            else
            {
                if (current.y < finishCell.y) step = Vector2Int.up;
                else if (current.y > finishCell.y) step = Vector2Int.down;
            }


            if (step == Vector2Int.zero)
            {
                if (current.x != finishCell.x) step.x = current.x < finishCell.x ? 1 : -1;
                else if (current.y != finishCell.y) step.y = current.y < finishCell.y ? 1 : -1;
            }

            current += step;
            current = ClampToGrid(current);
            pathCells.Add(current);
        }


        foreach (var cell in pathCells)
            pathPoints.Add(GridToWorld(cell));
    }

    void SpawnWalls(int count, Vector3 size)
    {
        System.Random rand = new System.Random();
        int placed = 0;
        int attempts = 0;

        Vector2Int startCell = WorldToGrid(startLine.position);
        Vector2Int finishCell = WorldToGrid(finishLine.position);

        while (placed < count && attempts < count * 50)
        {
            attempts++;

            int x = rand.Next(0, gridWidth);
            int z = rand.Next(0, gridHeight);
            Vector2Int cell = new Vector2Int(x, z);


            if (pathCells.Contains(cell)) continue;
            if (Vector2Int.Distance(cell, startCell) <= 1) continue;
            if (Vector2Int.Distance(cell, finishCell) <= 1) continue;

            Vector3 pos = GridToWorld(cell);

            float rotY = 90f * rand.Next(0, 4);
            Quaternion rot = Quaternion.Euler(0f, rotY, 0f);

            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.position = new Vector3(pos.x, spawnY, pos.z);
            wall.transform.localScale = size;
            wall.transform.rotation = rot;
            wall.transform.parent = wallsParent;

            Renderer rend = wall.GetComponent<Renderer>();
            if (rend != null)
            {
                if (wallMaterial != null)
                    rend.material = wallMaterial;
                else
                    rend.material.color = Color.gray;
            }

            placed++;
        }
    }

    Vector2Int ClampToGrid(Vector2Int cell)
    {
        cell.x = Mathf.Clamp(cell.x, 0, gridWidth - 1);
        cell.y = Mathf.Clamp(cell.y, 0, gridHeight - 1);
        return cell;
    }

    Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt((worldPos.x - offsetX) / cellSize);
        int y = Mathf.RoundToInt((worldPos.z - offsetZ) / cellSize);
        return new Vector2Int(x, y);
    }

    Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(offsetX + gridPos.x * cellSize, spawnY, offsetZ + gridPos.y * cellSize);
    }

    void OnDrawGizmos()
    {
        offsetX = -((gridWidth - 1) * cellSize) / 2f;
        offsetZ = -((gridHeight - 1) * cellSize) / 2f;

        if (drawGrid)
        {
            Gizmos.color = Color.gray;
            for (int x = 0; x < gridWidth; x++)
            {
                for (int z = 0; z < gridHeight; z++)
                {
                    Vector3 pos = new Vector3(offsetX + x * cellSize, 0, offsetZ + z * cellSize);
                    Gizmos.DrawWireCube(pos, new Vector3(cellSize, 0.1f, cellSize));
                }
            }
        }

        if (drawPath && pathPoints.Count > 1)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < pathPoints.Count - 1; i++)
                Gizmos.DrawLine(pathPoints[i], pathPoints[i + 1]);
        }

        if (startLine != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(startLine.position, startLine.localScale);
        }

        if (finishLine != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(finishLine.position, finishLine.localScale);
        }
    }
}
