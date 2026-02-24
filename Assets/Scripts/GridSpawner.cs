using System.Collections.Generic;
using UnityEngine;


public class GridSpawner : MonoBehaviour
{
    [SerializeField] private GameObject gridCellPrefab;   // Assets/Prefabs/GridPos.prefab
    [SerializeField] private GameObject gridLinePrefab;    // A sprite prefab using Line.png

    private List<GameObject> spawnedObjects = new List<GameObject>();


    public void SpawnGrid(BoardConfig config)
    {
        ClearGrid();

        float offsetX = (config.width - 1) * config.cellSpacing / 2f;
        float offsetY = (config.height - 1) * config.cellSpacing / 2f;


        for (int y = 0; y < config.height; y++)
        {
            for (int x = 0; x < config.width; x++)
            {
                if (!config.IsCellValid(x, y)) continue;

                Vector2 worldPos = GetWorldPosition(x, y, config.cellSpacing, offsetX, offsetY);
                GameObject cell = Instantiate(gridCellPrefab, new Vector3(worldPos.x, worldPos.y, -1f), Quaternion.identity, transform);
                cell.name = $"GridPos_{x}_{y}";

                GridPos gridPos = cell.GetComponent<GridPos>();
                if (gridPos != null)
                {
                    gridPos.Initialize(x, y);
                }

                spawnedObjects.Add(cell);
            }
        }


        SpawnGridLines(config, offsetX, offsetY);
    }


    private void SpawnGridLines(BoardConfig config, float offsetX, float offsetY)
    {
        if (gridLinePrefab == null) return;


        bool IsValid(int cx, int cy) =>
            cx >= 0 && cx < config.width && cy >= 0 && cy < config.height && config.IsCellValid(cx, cy);

        // Horizontal edge segments
        for (int y = 0; y <= config.height; y++)
        {
            int runStart = -1;
            for (int x = 0; x <= config.width; x++)
            {
                bool edgeExists = x < config.width && (IsValid(x, y - 1) || IsValid(x, y));

                if (edgeExists && runStart < 0)
                {
                    runStart = x;
                }
                else if (!edgeExists && runStart >= 0)
                {
                    SpawnBorderSegment(config, offsetX, offsetY, runStart, x - 1, y, false);
                    runStart = -1;
                }
            }
        }

        // Vertical edge segments
        for (int x = 0; x <= config.width; x++)
        {
            int runStart = -1;
            for (int y = 0; y <= config.height; y++)
            {
                bool edgeExists = y < config.height && (IsValid(x - 1, y) || IsValid(x, y));

                if (edgeExists && runStart < 0)
                {
                    runStart = y;
                }
                else if (!edgeExists && runStart >= 0)
                {
                    SpawnBorderSegment(config, offsetX, offsetY, runStart, y - 1, x, true);
                    runStart = -1;
                }
            }
        }
    }


    private void SpawnBorderSegment(BoardConfig config, float offsetX, float offsetY,
        int runStart, int runEnd, int boundary, bool isVertical)
    {
        float spacing = config.cellSpacing;

        if (isVertical)
        {


            float xPos = (boundary - 0.5f) * spacing - offsetX;
            float yCenter = ((runStart + runEnd) / 2f) * spacing - offsetY;
            float segmentLength = (runEnd - runStart + 1) * spacing;

            GameObject line = Instantiate(gridLinePrefab, new Vector3(xPos, yCenter, 0f),
                Quaternion.Euler(0, 0, 90f), transform);
            line.name = $"Border_V_x{boundary}_y{runStart}to{runEnd}";

            float baseWidth = GetLineSpriteBaseWidth(line);
            if (baseWidth > 0)
                line.transform.localScale = new Vector3(segmentLength / baseWidth, 1f, 1f);

            spawnedObjects.Add(line);
        }
        else
        {


            float yPos = (boundary - 0.5f) * spacing - offsetY;
            float xCenter = ((runStart + runEnd) / 2f) * spacing - offsetX;
            float segmentLength = (runEnd - runStart + 1) * spacing;

            GameObject line = Instantiate(gridLinePrefab, new Vector3(xCenter, yPos, 0f),
                Quaternion.identity, transform);
            line.name = $"Border_H_y{boundary}_x{runStart}to{runEnd}";

            float baseWidth = GetLineSpriteBaseWidth(line);
            if (baseWidth > 0)
                line.transform.localScale = new Vector3(segmentLength / baseWidth, 1f, 1f);

            spawnedObjects.Add(line);
        }
    }


    private float GetLineSpriteBaseWidth(GameObject lineObj)
    {
        SpriteRenderer sr = lineObj.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            return sr.sprite.bounds.size.x;
        }
        return 1f;
    }


    public void ClearGrid()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedObjects.Clear();
    }


    public static Vector2 GetWorldPosition(int x, int y, float cellSpacing, float offsetX, float offsetY)
    {
        return new Vector2(x * cellSpacing - offsetX, y * cellSpacing - offsetY);
    }
}
