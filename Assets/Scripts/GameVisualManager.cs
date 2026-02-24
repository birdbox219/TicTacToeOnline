using DG.Tweening;
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
    private Dictionary<Vector2Int, SpawnAnimation> pieceMap;


    private float cellSpacing = 3.1f;
    private float offsetX;
    private float offsetY;
    private bool configCached = false;

    private void Awake()
    {
        visualGameObjectList = new List<GameObject>();
        pieceMap = new Dictionary<Vector2Int, SpawnAnimation>();
    }

    private void Start()
    {
        GameManager.instance.OnClickOnGridPosition += GameManager_OnClickOnGridPosition;
        GameManager.instance.OnGameWin += Instance_OnGameWin;
        GameManager.instance.OnRematch += Instance_OnRematch;


        //FadingXO
        GameManager.instance.OnPieceRemoved += Instance_OnPieceRemoved;
    }

    


    public void RefreshBoardConfig()
    {
        BoardConfig config = GameManager.instance?.ActiveBoardConfig;
        if (config != null)
        {
            cellSpacing = config.cellSpacing;
            offsetX = (config.width - 1) * cellSpacing / 2f;
            offsetY = (config.height - 1) * cellSpacing / 2f;
            configCached = true;
        }
        else
        {
            // Fallback: 3x3
            cellSpacing = 3.1f;
            offsetX = (3 - 1) * cellSpacing / 2f;
            offsetY = (3 - 1) * cellSpacing / 2f;
        }
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

        pieceMap.Clear();
    }

    private void Instance_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if(!NetworkManager.Singleton.IsServer)
        {
            return;
        }


        float angle = CalculateLineAngle(e.line);
        Quaternion rotation = Quaternion.Euler(0, 0, angle);


        float lineLength = CalculateLineLength(e.line);

        Vector2 centerWorldPos = GetWorldPosition(e.line.centerPos.x, e.line.centerPos.y);
        Transform lineCompleteTransform = Instantiate(lineCompletePrefab, centerWorldPos, rotation);

        float baseLength = 3.1f * 2f;
        float scaleFactor = lineLength / baseLength;
        if (scaleFactor > 0 && lineCompletePrefab.localScale.x > 0)
        {
            Vector3 scale = lineCompleteTransform.localScale;
            scale.x *= scaleFactor;
            lineCompleteTransform.localScale = scale;
        }

        lineCompleteTransform.GetComponent<NetworkObject>().Spawn();
        visualGameObjectList.Add(lineCompleteTransform.gameObject);



        float lineDrawTime = 3f;

        for (int i = 0; i < e.line.gridVector2Int.Count; i++)
        {
            Vector2Int gridPos = e.line.gridVector2Int[i];


            if (pieceMap.TryGetValue(gridPos, out SpawnAnimation piece))
            {

                float delay = i * (lineDrawTime / (e.line.gridVector2Int.Count - 1));


                piece.PlayWinSequenceRpc(delay);
            }
        }

    }


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


        SpawnAnimation spawnAnim = spawnedTransform.GetComponent<SpawnAnimation>();
        if (spawnAnim != null)
        {
            pieceMap[new Vector2Int(x, y)] = spawnAnim;
        }
    }


    private Vector2 GetWorldPosition(int x, int y)
    {
        if (!configCached)
            RefreshBoardConfig();

        return new Vector2(x * cellSpacing - offsetX, y * cellSpacing - offsetY);
    }



    private void Instance_OnPieceRemoved(object sender, Vector2Int gridPos)
    {
        if (pieceMap.TryGetValue(gridPos, out SpawnAnimation pieceAnim))
        {
            // Remove from tracking
            pieceMap.Remove(gridPos);
            visualGameObjectList.Remove(pieceAnim.gameObject);


            pieceAnim.transform.DOScale(Vector3.zero, 0.4f).SetEase(Ease.InBack);
            pieceAnim.transform.DORotate(new Vector3(0, 0, -180), 0.4f, RotateMode.FastBeyond360).SetRelative(true);

            Destroy(pieceAnim.gameObject, 0.45f);
        }
    }
}
