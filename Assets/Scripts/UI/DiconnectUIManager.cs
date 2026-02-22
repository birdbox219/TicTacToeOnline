using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using DG.Tweening;
using System;

public class DisconnectUIManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Add a CanvasGroup component to your Disconnect Panel and drag it here")]
    [SerializeField] private CanvasGroup disconnectPanel;

    private void Start()
    {
        // 1. Ensure the panel starts completely invisible and unclickable
        if (disconnectPanel != null)
        {
            disconnectPanel.alpha = 0f;
            disconnectPanel.interactable = false;
            disconnectPanel.blocksRaycasts = false;
        }

        // 2. Listen for the disconnect event
        if (GameManager.instance != null)
        {
            GameManager.instance.OnPlayerDisconnect += GameManager_OnPlayerDisconnected;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.OnPlayerDisconnect -= GameManager_OnPlayerDisconnected;
        }
    }

    private void GameManager_OnPlayerDisconnected(object sender, EventArgs e)
    {
        // Smoothly fade the warning panel in and make it clickable
        if (disconnectPanel != null)
        {
            disconnectPanel.interactable = true;
            disconnectPanel.blocksRaycasts = true;
            disconnectPanel.DOFade(1f, 0.4f);
        }
    }

    /// <summary>
    /// Link this to the "Return to Menu" button on your Disconnect Panel.
    /// </summary>
    public void ReturnToMainMenu()
    {
        // 1. Safely disconnect from the network
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }

        // 2. Load your lobby/menu scene (Replace "LobbyScene" with your actual scene name!)
        SceneManager.LoadScene("LobbyTutorial_Done");
    }
}