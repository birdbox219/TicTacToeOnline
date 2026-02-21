using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSyncController : MonoBehaviour
{
    public static SceneSyncController Instance { get; private set; }

    [Header("Scene Settings")]
    [Tooltip("Type the EXACT name of your gameplay scene here.")]
    public string gameSceneName = "GameScene";

    private void Awake()
    {
        // Standard Singleton & DontDestroyOnLoad setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Listen for the exact moment the Host successfully starts the server
        NetworkManager.Singleton.OnServerStarted += OnHostStarted;
    }

    private void OnHostStarted()
    {
        // Safety check: Only the Server/Host has the authority to load a networked scene
        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log($"Host started successfully. Syncing all clients to scene: {gameSceneName}...");

            // This is the magic Netcode command that moves EVERYONE to the new scene
            NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
        }
    }

    private void OnDestroy()
    {
        // Always clean up event subscriptions to prevent memory leaks
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted -= OnHostStarted;
        }
    }
}