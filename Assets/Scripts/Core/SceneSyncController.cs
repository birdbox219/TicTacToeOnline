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

        NetworkManager.Singleton.OnServerStarted += OnHostStarted;
    }

    private void OnHostStarted()
    {

        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log($"Host started successfully. Syncing all clients to scene: {gameSceneName}...");

            NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
        }
    }

    private void OnDestroy()
    {

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted -= OnHostStarted;
        }
    }
}