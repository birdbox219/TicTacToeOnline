using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns grid cells and grid lines at runtime based on a BoardConfig.
/// Replaces manually-placed GridPos objects and Line sprites in the scene.
/// </summary>
public class GridSpawner : MonoBehaviour
{
    [SerializeField] private GameObject gridCellPrefab;   // Assets/Prefabs/GridPos.prefab
    [SerializeField] private GameObject gridLinePrefab;    // A sprite prefab using Line.png

    private List<GameObject> spawnedObjects = new List<GameObject>();

    /// <summary>
    /// Spawn the grid for the given BoardConfig.
    /// Creates grid cells at calculated world positions, and grid lines between them.
    /// </summary>
    public void SpawnGrid(BoardConfig config)
    {
        ClearGrid();

        float offsetX = (config.width - 1) * config.cellSpacing / 2f;
        float offsetY = (config.height - 1) * config.cellSpacing / 2f;

        // Spawn grid cells
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

        // Spawn grid lines between cells
        SpawnGridLines(config, offsetX, offsetY);
    }

    /// <summary>
    /// Spawn cell-border grid lines. Each valid cell gets borders on all 4 sides.
    /// Shared edges between adjacent valid cells are merged into single line segments.
    /// This correctly handles any board shape (pyramid, diamond, rectangular, etc).
    /// </summary>
    private void SpawnGridLines(BoardConfig config, float offsetX, float offsetY)
    {
        if (gridLinePrefab == null) return;

        // Helper: is this cell valid? Out-of-bounds = false
        bool IsValid(int cx, int cy) =>
            cx >= 0 && cx < config.width && cy >= 0 && cy < config.height && config.IsCellValid(cx, cy);

        // ── Horizontal edge segments ──
        // For each y boundary (0 to height): draw an edge where at least one
        // adjacent cell (above or below) is valid. Merge consecutive x into runs.
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

        // ── Vertical edge segments ──
        // For each x boundary (0 to width): draw an edge where at least one
        // adjacent cell (left or right) is valid. Merge consecutive y into runs.
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

    /// <summary>
    /// Spawn a single border edge segment.
    /// - Horizontal: at y boundary, spanning cells runStart..runEnd in x
    /// - Vertical:   at x boundary, spanning cells runStart..runEnd in y
    /// </summary>
    private void SpawnBorderSegment(BoardConfig config, float offsetX, float offsetY,
        int runStart, int runEnd, int boundary, bool isVertical)
    {
        float spacing = config.cellSpacing;

        if (isVertical)
        {
            // Vertical edge at column boundary 'boundary' (between col boundary-1 and col boundary)
            // Spans cells runStart..runEnd in y direction
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
            // Horizontal edge at row boundary 'boundary' (between row boundary-1 and row boundary)
            // Spans cells runStart..runEnd in x direction
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

    /// <summary>
    /// Get the base width of the line sprite (unscaled).
    /// </summary>
    private float GetLineSpriteBaseWidth(GameObject lineObj)
    {
        SpriteRenderer sr = lineObj.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            return sr.sprite.bounds.size.x;
        }
        return 1f; // fallback
    }

    /// <summary>
    /// Clear all spawned grid objects.
    /// </summary>
    public void ClearGrid()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedObjects.Clear();
    }

    /// <summary>
    /// Calculate world position for a grid cell.
    /// </summary>
    public static Vector2 GetWorldPosition(int x, int y, float cellSpacing, float offsetX, float offsetY)
    {
        return new Vector2(x * cellSpacing - offsetX, y * cellSpacing - offsetY);
    }
}
