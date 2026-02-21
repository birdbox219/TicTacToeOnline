using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using static GameManager;

public class GameManager : NetworkBehaviour
{

    public static GameManager instance { get; private set; }

    // ── Board Configuration ──
    [Header("Board Configuration")]
    [SerializeField] private BoardConfig[] boardConfigs;  // All available board configs
    [SerializeField] private GridSpawner gridSpawner;

    private BoardConfig boardConfig;  // The currently active config
    public BoardConfig ActiveBoardConfig => boardConfig;

    /// <summary>
    /// Select a BoardConfig by GameMode name (matches BoardConfig.modeName).
    /// Call this BEFORE the game starts (e.g. from lobby flow).
    /// </summary>
    public void SetBoardConfig(LobbyManager.GameMode gameMode)
    {
        string targetName = gameMode switch
        {
            LobbyManager.GameMode.Classic3x3 => "Classic 3x3",
            LobbyManager.GameMode.PyramidXO => "Pyramid XO",
            _ => "Classic 3x3"
        };

        foreach (var config in boardConfigs)
        {
            if (config != null && config.modeName == targetName)
            {
                boardConfig = config;
                Debug.Log($"GameManager: Selected board config '{config.modeName}'");
                return;
            }
        }

        // Fallback to first config
        if (boardConfigs.Length > 0 && boardConfigs[0] != null)
        {
            boardConfig = boardConfigs[0];
            Debug.LogWarning($"GameManager: GameMode '{gameMode}' not found, falling back to '{boardConfig.modeName}'");
        }
    }

    public event EventHandler<OnClickOnGridPositionEventArgs> OnClickOnGridPosition;
    public class OnClickOnGridPositionEventArgs : EventArgs
    {
        public int x;
        public int y;
        public PlayerType playerType;

        public OnClickOnGridPositionEventArgs(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public event EventHandler OnGameStarted;
    public event EventHandler OnCurrentPlayblePlayerTypeChanged;
    public event EventHandler OnRematch;
    public event EventHandler OnGameTie;
    public event EventHandler OnScoreChanged;
    public event EventHandler OnPlaceObject;

    public event EventHandler<OnGameWinEventArgs> OnGameWin;
    public class OnGameWinEventArgs : EventArgs
    {
        public Line line;
        public PlayerType winPlayerType;
    }


    public enum PlayerType
    {
        None,
        Cross,
        Cricle
    }

    public enum Oriantation
    {
        Horizontal,
        Vertical,
        DiagonalA,
        DiagonalB
    }

    public struct Line
    {
        public List<Vector2Int> gridVector2Int;
        public Vector2Int centerPos;
        public Oriantation oriantation;
    }



    private PlayerType LocalplayerType;
    private NetworkVariable<PlayerType> currentTurnPlayerType = new NetworkVariable<PlayerType>();
    private PlayerType[,] playerTypeArray;
    private List<Line> lineList;
    private NetworkVariable<int> playerCrossScore = new NetworkVariable<int>();
    private NetworkVariable<int> playerCircleScore = new NetworkVariable<int>();



    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.LogWarning("Found duplicate GameManager instance, destroying the new one.");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Initialize (or reinitialize) the board from the current BoardConfig.
    /// Called from Awake and can be called again after SetBoardConfig.
    /// </summary>
    public void InitializeBoard()
    {
        if (boardConfig == null)
        {
            Debug.LogError("GameManager: BoardConfig is not assigned! Falling back to 3x3.");
            playerTypeArray = new PlayerType[3, 3];
            lineList = GenerateFallbackLines();
        }
        else
        {
            playerTypeArray = new PlayerType[boardConfig.width, boardConfig.height];
            lineList = GenerateWinLines(boardConfig);
        }

        // Spawn the grid at runtime
        if (gridSpawner != null && boardConfig != null)
        {
            gridSpawner.SpawnGrid(boardConfig);
        }
    }

    /// <summary>
    /// Generate all possible winning lines for a given BoardConfig.
    /// Scans horizontal, vertical, and both diagonal directions.
    /// </summary>
    private List<Line> GenerateWinLines(BoardConfig config)
    {
        List<Line> lines = new List<Line>();

        // Direction vectors: right, down, diagonal-down-right, diagonal-down-left
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0),  // Horizontal
            new Vector2Int(0, 1),  // Vertical
            new Vector2Int(1, 1),  // DiagonalA (bottom-left to top-right)
            new Vector2Int(1, -1)  // DiagonalB (top-left to bottom-right)
        };

        Oriantation[] orientations = new Oriantation[]
        {
            Oriantation.Horizontal,
            Oriantation.Vertical,
            Oriantation.DiagonalA,
            Oriantation.DiagonalB
        };

        for (int d = 0; d < directions.Length; d++)
        {
            Vector2Int dir = directions[d];
            Oriantation orientation = orientations[d];

            for (int y = 0; y < config.height; y++)
            {
                for (int x = 0; x < config.width; x++)
                {
                    // Check if a line of winLength fits starting from (x, y) in direction dir
                    List<Vector2Int> positions = new List<Vector2Int>();
                    bool allValid = true;

                    for (int step = 0; step < config.winLength; step++)
                    {
                        int px = x + dir.x * step;
                        int py = y + dir.y * step;

                        if (px < 0 || px >= config.width || py < 0 || py >= config.height)
                        {
                            allValid = false;
                            break;
                        }

                        if (!config.IsCellValid(px, py))
                        {
                            allValid = false;
                            break;
                        }

                        positions.Add(new Vector2Int(px, py));
                    }

                    if (allValid && positions.Count == config.winLength)
                    {
                        // Center position is the middle element
                        Vector2Int center = positions[config.winLength / 2];

                        lines.Add(new Line
                        {
                            gridVector2Int = positions,
                            centerPos = center,
                            oriantation = orientation
                        });
                    }
                }
            }
        }

        Debug.Log($"Generated {lines.Count} win lines for {config.modeName} ({config.width}x{config.height}, win={config.winLength})");
        return lines;
    }

    /// <summary>
    /// Fallback: hardcoded 3x3 lines if no BoardConfig is assigned.
    /// </summary>
    private List<Line> GenerateFallbackLines()
    {
        return new List<Line>
        {
            // Horizontal Lines
            new Line
            {
                gridVector2Int = new List<Vector2Int> { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(2,0) },
                centerPos = new Vector2Int(1,0),
                oriantation = Oriantation.Horizontal,
            },
            new Line
            {
                gridVector2Int = new List<Vector2Int> { new Vector2Int(0,1), new Vector2Int(1,1), new Vector2Int(2,1) },
                centerPos = new Vector2Int(1,1),
                oriantation = Oriantation.Horizontal,
            },
            new Line
            {
                gridVector2Int = new List<Vector2Int> { new Vector2Int(0,2), new Vector2Int(1,2), new Vector2Int(2,2) },
                centerPos = new Vector2Int(1,2),
                oriantation = Oriantation.Horizontal,
            },
            // Vertical Lines
            new Line
            {
                gridVector2Int = new List<Vector2Int> { new Vector2Int(0,0), new Vector2Int(0,1), new Vector2Int(0,2) },
                centerPos = new Vector2Int(0,1),
                oriantation = Oriantation.Vertical,
            },
            new Line
            {
                gridVector2Int = new List<Vector2Int> { new Vector2Int(1,0), new Vector2Int(1,1), new Vector2Int(1,2) },
                centerPos = new Vector2Int(1,1),
                oriantation = Oriantation.Vertical,
            },
            new Line
            {
                gridVector2Int = new List<Vector2Int> { new Vector2Int(2,0), new Vector2Int(2,1), new Vector2Int(2,2) },
                centerPos = new Vector2Int(2,1),
                oriantation = Oriantation.Vertical,
            },
            // Diagonal Lines
            new Line
            {
                gridVector2Int = new List<Vector2Int> { new Vector2Int(0,0), new Vector2Int(1,1), new Vector2Int(2,2) },
                centerPos = new Vector2Int(1,1),
                oriantation = Oriantation.DiagonalA,
            },
            new Line
            {
                gridVector2Int = new List<Vector2Int> { new Vector2Int(0,2), new Vector2Int(1,1), new Vector2Int(2,0) },
                centerPos = new Vector2Int(1,1),
                oriantation = Oriantation.DiagonalB,
            },
        };
    }


    public override void OnNetworkSpawn()
    {
        Debug.Log($"On NetworkSpawn : {NetworkManager.Singleton.LocalClientId}");

        // Initialize board from the lobby's selected game mode
        // By this point, LobbyManager.SelectedGameMode is set for both host and client
        SetBoardConfig(LobbyManager.SelectedGameMode);
        InitializeBoard();

        ulong clientId = NetworkManager.Singleton.LocalClientId;

        if (clientId == 0)
        {
            LocalplayerType = PlayerType.Cross;
        }
        else
        {
            LocalplayerType = PlayerType.Cricle;
        }

        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        }
        currentTurnPlayerType.OnValueChanged += (PlayerType oldPlayerType, PlayerType newPlayerType) =>
        {
            OnCurrentPlayblePlayerTypeChanged?.Invoke(this, EventArgs.Empty);
        };

        playerCrossScore.OnValueChanged += (int oldScore, int newScore) =>
        {
            OnScoreChanged?.Invoke(this, EventArgs.Empty);
        };

        playerCircleScore.OnValueChanged += (int oldScore, int newScore) =>
        {
            OnScoreChanged?.Invoke(this, EventArgs.Empty);
        };
    }

    private void NetworkManager_OnClientConnectedCallback(ulong obj)
    {
        if(NetworkManager.Singleton.ConnectedClientsList.Count == 2)
        {
            currentTurnPlayerType.Value = PlayerType.Cross;
            TriggerOnGameStartedRpc();
        }
    }


    [Rpc(SendTo.ClientsAndHost)]
    public void TriggerOnGameStartedRpc()
    {
        OnGameStarted?.Invoke(this, EventArgs.Empty);

    }

    [Rpc(SendTo.Server)]
    public void ClickedOnGridPosRpc(int x , int y , PlayerType playerType)
    {
        Debug.Log($"Grid Position Clicked at ({x} , {y})");

        if(playerType != currentTurnPlayerType.Value)
        {
            Debug.Log("Not your turn!");
            return;
        }

        // Validate cell is within bounds
        int gridWidth = boardConfig != null ? boardConfig.width : 3;
        int gridHeight = boardConfig != null ? boardConfig.height : 3;

        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight)
        {
            Debug.Log("Position out of bounds!");
            return;
        }

        // Validate cell is a valid playable cell
        if (boardConfig != null && !boardConfig.IsCellValid(x, y))
        {
            Debug.Log("Invalid cell position!");
            return;
        }

        if(playerTypeArray[x,y] != PlayerType.None)
        {
            Debug.Log("Position already taken!");
            return;
        }

        playerTypeArray[x, y] = playerType;
        TriggerOnPlaceObjectRpc();

        OnClickOnGridPosition?.Invoke(this, new OnClickOnGridPositionEventArgs(x, y)
        {
            playerType = playerType
        });

        switch(currentTurnPlayerType.Value)
        {
            default:
            case PlayerType.Cross:
                currentTurnPlayerType.Value = PlayerType.Cricle;
                break;
            case PlayerType.Cricle:
                currentTurnPlayerType.Value = PlayerType.Cross;
                break;
        }

        TestWinner();
    }


    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnPlaceObjectRpc()
    {
        OnPlaceObject?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Test if a single line is a winner. Works for any winLength.
    /// </summary>
    private bool TestWinnerLine(Line line)
    {
        if (line.gridVector2Int == null || line.gridVector2Int.Count == 0) return false;

        PlayerType first = playerTypeArray[line.gridVector2Int[0].x, line.gridVector2Int[0].y];
        if (first == PlayerType.None) return false;

        for (int i = 1; i < line.gridVector2Int.Count; i++)
        {
            if (playerTypeArray[line.gridVector2Int[i].x, line.gridVector2Int[i].y] != first)
                return false;
        }
        return true;
    }

    private void TestWinner()
    {
        for(int i = 0; i < lineList.Count; i++)
        {
            Line line = lineList[i];
            if (TestWinnerLine(line))
            {
                Debug.Log($"Player {playerTypeArray[line.centerPos.x , line.centerPos.y]} wins!");
                currentTurnPlayerType.Value = PlayerType.None;

                PlayerType winPlayerType = playerTypeArray[line.centerPos.x, line.centerPos.y];

                switch(winPlayerType)
                {
                    case PlayerType.Cross:
                        playerCrossScore.Value++;
                        break;
                    case PlayerType.Cricle:
                        playerCircleScore.Value++;
                        break;
                }

                TriggerOnGameWinRpc(i , winPlayerType);

                return;
            }
        }

        // ── Tie detection: only check valid cells ──
        bool hasTie = true;
        int gridWidth = boardConfig != null ? boardConfig.width : 3;
        int gridHeight = boardConfig != null ? boardConfig.height : 3;

        for(int x = 0; x < gridWidth; x++)
        {
            for(int y = 0; y < gridHeight; y++)
            {
                // Skip invalid cells in non-rectangular boards
                if (boardConfig != null && !boardConfig.IsCellValid(x, y)) continue;

                if(playerTypeArray[x,y] == PlayerType.None)
                {
                    hasTie = false;
                    break;
                }
            }
            if (!hasTie) break;
        }

        if (hasTie)
        {
            TriggerOnGameTieRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameWinRpc(int  lineIndex , PlayerType winplayerType)
    {
        Line line = lineList[lineIndex];
        OnGameWin?.Invoke(this, new OnGameWinEventArgs { line = line, winPlayerType = winplayerType });
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameTieRpc()
    {
        Debug.Log($"TriggerOnGameTiedRpc executed. IsServer={IsServer} LocalClientId={NetworkManager.Singleton.LocalClientId}");
        OnGameTie?.Invoke(this, EventArgs.Empty);
    }

    [Rpc(SendTo.Server)]
    public void RematchRpc()
    {
        int gridWidth = boardConfig != null ? boardConfig.width : 3;
        int gridHeight = boardConfig != null ? boardConfig.height : 3;

        for(int x = 0; x < gridWidth; x++)
        {
            for(int y = 0; y < gridHeight; y++)
            {
                playerTypeArray[x, y] = PlayerType.None;
            }
        }
        currentTurnPlayerType.Value = PlayerType.Cross;
        TriggerOnRematchRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnRematchRpc()
    {
        OnRematch?.Invoke(this, EventArgs.Empty);
    }

    public PlayerType GetLocalplayerType()
    {
        return LocalplayerType;
    }

    public PlayerType GetCurrentTurnPlayerType()
    {
        return currentTurnPlayerType.Value;
    }

    public void GetScores(out int playerCrossScore , out int playerCircleScore)
    {
        playerCrossScore = this.playerCrossScore.Value;
        playerCircleScore = this.playerCircleScore.Value;
    }
}