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


    public event EventHandler<OnClickOnGridPositionEventArgs> OnClickOnGridPosition;
    public class OnClickOnGridPositionEventArgs : EventArgs
    {
        public int x;
        public int y;
        public PlayerType playerType;

        public OnClickOnGridPositionEventArgs(int x, int y   /* , PlayerType playerType)*/)
        {
            this.x = x;
            this.y = y;
            //this.playerType = playerType;
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
            // Optional: DontDestroyOnLoad(gameObject);


        }
        else if (instance != this)
        {
            Debug.LogWarning("Found duplicate GameManager instance, destroying the new one.");
            Destroy(gameObject);
        }

        playerTypeArray = new PlayerType[3, 3];

        lineList = new List<Line>
        {
            //Horzinatol Lines
            new Line
            {
                gridVector2Int = new List<Vector2Int> { new Vector2Int(0,0) , new Vector2Int(1,0) , new Vector2Int(2,0) },
                centerPos = new Vector2Int(1,0),
                oriantation = Oriantation.Horizontal,

            },

            new Line
            {
                gridVector2Int = new List<Vector2Int> { new Vector2Int(0,1) , new Vector2Int(1,1) , new Vector2Int(2,1) },
                centerPos = new Vector2Int(1,1),
                oriantation = Oriantation.Horizontal,

            },

            new Line
            {
                gridVector2Int = new List<Vector2Int> { new Vector2Int(0,2) , new Vector2Int(1,2) , new Vector2Int(2,2) },
                centerPos = new Vector2Int(1,2),
                oriantation = Oriantation.Horizontal,
            },

            //Vertical Lines
            new Line
            {
                gridVector2Int = new List<Vector2Int> { new Vector2Int(0,0) , new Vector2Int(0,1) , new Vector2Int(0,2) },
                centerPos = new Vector2Int(0,1),
                oriantation = Oriantation.Vertical,
            },
            new Line
            {
                gridVector2Int = new List<Vector2Int> { new Vector2Int(1,0) , new Vector2Int(1,1) , new Vector2Int(1,2) },
                centerPos = new Vector2Int(1,1),
                oriantation = Oriantation.Vertical,
            },
            new Line
            {
                gridVector2Int = new List<Vector2Int> { new Vector2Int(2,0) , new Vector2Int(2,1) , new Vector2Int(2,2) },
                centerPos = new Vector2Int(2,1),
                oriantation = Oriantation.Vertical,
            },

            //Diagonal Lines    
            new Line
            {
                gridVector2Int = new List<Vector2Int> { new Vector2Int(0,0) , new Vector2Int(1,1) , new Vector2Int(2,2) },
                centerPos = new Vector2Int(1,1),
                oriantation = Oriantation.DiagonalA,
            },
            new Line
            {
                gridVector2Int = new List<Vector2Int> { new Vector2Int(0,2) , new Vector2Int(1,1) , new Vector2Int(2 ,0) },
                centerPos = new Vector2Int(1,1),
                oriantation = Oriantation.DiagonalB,
            },
            
            


        };




    }


    public override void OnNetworkSpawn()
    {
        Debug.Log($"On NetworkSpawn : {NetworkManager.Singleton.LocalClientId}");

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

        if(playerTypeArray[x,y] != PlayerType.None)
        {
            Debug.Log("Position already taken!");
            return;
        }

        playerTypeArray[x, y] = playerType;
        TriggerOnPlaceObjectRpc();

        OnClickOnGridPosition?.Invoke(this, new OnClickOnGridPositionEventArgs(x, y  /* GetLocalplayerType() */ )

        {
            playerType = playerType
        }

        );

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

        //TriggerOnCurrentPlayablePlayerTypeChangedRpc();

        TestWinner();


    }




    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnPlaceObjectRpc()
    {
        OnPlaceObject?.Invoke(this, EventArgs.Empty);
    }

    //[Rpc(SendTo.ClientsAndHost)]

    //private void TriggerOnCurrentPlayablePlayerTypeChangedRpc()
    //{
    //    OnCurrentPlayblePlayerTypeChanged?.Invoke(this, EventArgs.Empty);
    //}

    private bool TestWinnerLine(Line line)
    {
        return TestWinnerLine(
            playerTypeArray[line.gridVector2Int[0].x , line.gridVector2Int[0].y] ,
            playerTypeArray[line.gridVector2Int[1].x , line.gridVector2Int[1].y] ,
            playerTypeArray[line.gridVector2Int[2].x , line.gridVector2Int[2].y]
            );

    }

    private bool TestWinnerLine(PlayerType aPlayerType , PlayerType bPlayerType , PlayerType cPlayertype)
    {
        return
            aPlayerType != PlayerType.None && aPlayerType == bPlayerType && bPlayerType == cPlayertype;
    }
    private void TestWinner()
    {
        //foreach(Line line in lineList)
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
                //break;
            }
            

        }

        bool hasTie = true;
        for(int x = 0; x < playerTypeArray.GetLength(0); x++)
        {
            for(int y = 0; y < playerTypeArray.GetLength(1); y++)
            {
                if(playerTypeArray[x,y] == PlayerType.None)
                {
                    hasTie = false;
                    break;
                }
            }
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

    //CANT SAND COMPLEX TYPES VIA RPC, SO INVOKE DIRECTLY
    //STRUCT NOT SUPPORTED
    //[Rpc(SendTo.ClientsAndHost)]
    //private void TriggerOnGameWinRpc(Line line)
    //{
    //    OnGameWin?.Invoke(this, new OnGameWinEventArgs { line = line });
    //}
    [Rpc(SendTo.Server)]
    public void RematchRpc()
    {
        
        for(int x = 0; x < playerTypeArray.GetLength(0); x++)
        {
            for(int y = 0; y < playerTypeArray.GetLength(1); y++)
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
 