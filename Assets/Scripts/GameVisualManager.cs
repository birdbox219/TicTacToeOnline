using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class GameVisualManager : NetworkBehaviour
{
    [SerializeField] private Transform crossPrefab;
    [SerializeField] private Transform circlePrefab;
    [SerializeField] private Transform lineCompletePrefab;

    private List<GameObject> visualGameObjectList;

    // Cached board config values
    private float cellSpacing = 3.1f;
    private float offsetX;
    private float offsetY;

    private void Awake()
    {
        visualGameObjectList = new List<GameObject>();
    }

    private void Start()
    {
        // Cache board config values for positioning
        BoardConfig config = GameManager.instance.ActiveBoardConfig;
        if (config != null)
        {
            cellSpacing = config.cellSpacing;
            offsetX = (config.width - 1) * cellSpacing / 2f;
            offsetY = (config.height - 1) * cellSpacing / 2f;
        }
        else
        {
            // Fallback: 3x3 with old spacing
            cellSpacing = 3.1f;
            offsetX = (3 - 1) * cellSpacing / 2f;
            offsetY = (3 - 1) * cellSpacing / 2f;
        }

        GameManager.instance.OnClickOnGridPosition += GameManager_OnClickOnGridPosition;
        GameManager.instance.OnGameWin += Instance_OnGameWin;
        GameManager.instance.OnRematch += Instance_OnRematch;
    }

    private void Instance_OnRematch(object sender, System.EventArgs e)
    {
        if(!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        foreach (GameObject visualGameObject in visualGameObjectList)
        {
            Destroy(visualGameObject);
        }
        visualGameObjectList.Clear();
    }

    private void Instance_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if(!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        // Calculate rotation from the line direction, instead of hardcoded enum cases
        float angle = CalculateLineAngle(e.line);
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        // Calculate scale based on win length
        float lineLength = CalculateLineLength(e.line);

        Vector2 centerWorldPos = GetWorldPosition(e.line.centerPos.x, e.line.centerPos.y);
        Transform lineCompleteTransform = Instantiate(lineCompletePrefab, centerWorldPos, rotation);

        // Scale the line to match the win length
        // The original line prefab was sized for 3-in-a-row at spacing 3.1
        // We scale proportionally
        float baseLength = 3.1f * 2f; // original 3x3 line covers ~2 spacings
        float scaleFactor = lineLength / baseLength;
        if (scaleFactor > 0 && lineCompletePrefab.localScale.x > 0)
        {
            Vector3 scale = lineCompleteTransform.localScale;
            scale.x *= scaleFactor;
            lineCompleteTransform.localScale = scale;
        }

        lineCompleteTransform.GetComponent<NetworkObject>().Spawn();
        visualGameObjectList.Add(lineCompleteTransform.gameObject);
    }

    /// <summary>
    /// Calculate the angle of a win line from its start to end position.
    /// </summary>
    private float CalculateLineAngle(GameManager.Line line)
    {
        if (line.gridVector2Int == null || line.gridVector2Int.Count < 2)
            return 0f;

        Vector2 startPos = GetWorldPosition(line.gridVector2Int[0].x, line.gridVector2Int[0].y);
        Vector2 endPos = GetWorldPosition(line.gridVector2Int[line.gridVector2Int.Count - 1].x,
                                            line.gridVector2Int[line.gridVector2Int.Count - 1].y);

        Vector2 dir = endPos - startPos;
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }

    /// <summary>
    /// Calculate the world-space length of a win line.
    /// </summary>
    private float CalculateLineLength(GameManager.Line line)
    {
        if (line.gridVector2Int == null || line.gridVector2Int.Count < 2)
            return cellSpacing;

        Vector2 startPos = GetWorldPosition(line.gridVector2Int[0].x, line.gridVector2Int[0].y);
        Vector2 endPos = GetWorldPosition(line.gridVector2Int[line.gridVector2Int.Count - 1].x,
                                            line.gridVector2Int[line.gridVector2Int.Count - 1].y);

        return Vector2.Distance(startPos, endPos);
    }

    private void GameManager_OnClickOnGridPosition(object sender, GameManager.OnClickOnGridPositionEventArgs e)
    {
        SpawnObjectRpc(e.x , e.y , e.playerType );
    }


    [Rpc(SendTo.Server)]
    private void SpawnObjectRpc(int x , int y , GameManager.PlayerType playerType)
    {
        Transform prefab;

        switch (playerType)
        {
            default:
            case GameManager.PlayerType.Cross:
                    prefab = crossPrefab;
                    break;
            case GameManager.PlayerType.Cricle:
                    prefab = circlePrefab;
                    break;
        }

        Transform spawnedTransform = Instantiate(prefab, GetWorldPosition(x, y), Quaternion.identity);
        spawnedTransform.GetComponent<NetworkObject>().Spawn();
        visualGameObjectList.Add(spawnedTransform.gameObject);
    }

    /// <summary>
    /// Dynamic world position calculation based on BoardConfig.
    /// Centers the grid around (0, 0) for any board size.
    /// </summary>
    private Vector2 GetWorldPosition(int x, int y)
    {
        return new Vector2(x * cellSpacing - offsetX, y * cellSpacing - offsetY);
    }
}
