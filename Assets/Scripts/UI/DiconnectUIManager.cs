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
        if (disconnectPanel != null)
        {
            disconnectPanel.alpha = 0f;
            disconnectPanel.interactable = false;
            disconnectPanel.blocksRaycasts = false;
        }

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

        if (disconnectPanel != null)
        {
            disconnectPanel.interactable = true;
            disconnectPanel.blocksRaycasts = true;
            disconnectPanel.DOFade(1f, 0.4f).OnComplete(() =>
            {
                disconnectPanel.transform.DOShakePosition(0.5f, strength: 10f, vibrato: 12, randomness: 90f, snapping: false, fadeOut: true);
            });
        }
    }


    public void ReturnToMainMenu()
    {
        GameManager.DisconnectAndReturnToMenu();
    }
}