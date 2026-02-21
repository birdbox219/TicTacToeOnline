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
    /// Spawn the # pattern lines between grid cells.
    /// For an NxM grid, there are (N-1) vertical lines and (M-1) horizontal lines.
    /// </summary>
    private void SpawnGridLines(BoardConfig config, float offsetX, float offsetY)
    {
        if (gridLinePrefab == null) return;

        float lineLength = config.height * config.cellSpacing;
        float lineWidth = config.width * config.cellSpacing;

        // Vertical lines (between columns)
        for (int x = 0; x < config.width - 1; x++)
        {
            float xPos = (x + 0.5f) * config.cellSpacing - offsetX;
            float yPos = 0f; // centered

            GameObject line = Instantiate(gridLinePrefab, new Vector3(xPos, yPos, 0f), Quaternion.Euler(0, 0, 90f), transform);
            line.name = $"GridLine_V_{x}";

            // Scale to fit the grid height
            float scaleX = lineLength / GetLineSpriteBaseWidth(line);
            if (scaleX > 0)
                line.transform.localScale = new Vector3(scaleX, 1f, 1f);

            spawnedObjects.Add(line);
        }

        // Horizontal lines (between rows)
        for (int y = 0; y < config.height - 1; y++)
        {
            float xPos = 0f; // centered
            float yPos = (y + 0.5f) * config.cellSpacing - offsetY;

            GameObject line = Instantiate(gridLinePrefab, new Vector3(xPos, yPos, 0f), Quaternion.identity, transform);
            line.name = $"GridLine_H_{y}";

            // Scale to fit the grid width
            float scaleX = lineWidth / GetLineSpriteBaseWidth(line);
            if (scaleX > 0)
                line.transform.localScale = new Vector3(scaleX, 1f, 1f);

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
