using System;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListUI : MonoBehaviour {

    public static LobbyListUI Instance { get; private set; }

    [SerializeField] private Transform lobbySingleTemplate;
    [SerializeField] private Transform container;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Button createLobbyButton;

    [Header("Juice Settings")]
    [SerializeField] private CanvasGroup canvasGroup;

    private void Awake() {
        Instance = this;

        lobbySingleTemplate.gameObject.SetActive(false);

        refreshButton.onClick.AddListener(RefreshButtonClick);
        createLobbyButton.onClick.AddListener(CreateLobbyButtonClick);
    }

    private void Start() {
        LobbyManager.Instance.OnLobbyListChanged += LobbyManager_OnLobbyListChanged;
        LobbyManager.Instance.OnJoinedLobby += LobbyManager_OnJoinedLobby;
        LobbyManager.Instance.OnLeftLobby += LobbyManager_OnLeftLobby;
        LobbyManager.Instance.OnKickedFromLobby += LobbyManager_OnKickedFromLobby;
    }

    private void OnEnable() {

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.transform.localScale = Vector3.one * 0.8f;
            canvasGroup.DOFade(1f, 0.3f);
            canvasGroup.transform.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack);
        }
    }

    private void LobbyManager_OnKickedFromLobby(object sender, LobbyManager.LobbyEventArgs e) {
        Show();
    }

    private void LobbyManager_OnLeftLobby(object sender, EventArgs e) {
        Show();
    }

    private void LobbyManager_OnJoinedLobby(object sender, LobbyManager.LobbyEventArgs e) {
        Hide();
    }

    private void LobbyManager_OnLobbyListChanged(object sender, LobbyManager.OnLobbyListChangedEventArgs e) {
        UpdateLobbyList(e.lobbyList);
    }

    private void UpdateLobbyList(List<Lobby> lobbyList) {
        foreach (Transform child in container) {
            if (child == lobbySingleTemplate) continue;

            Destroy(child.gameObject);
        }

        int index = 0;
        foreach (Lobby lobby in lobbyList) {
            Transform lobbySingleTransform = Instantiate(lobbySingleTemplate, container);
            lobbySingleTransform.gameObject.SetActive(true);
            LobbyListSingleUI lobbyListSingleUI = lobbySingleTransform.GetComponent<LobbyListSingleUI>();
            lobbyListSingleUI.UpdateLobby(lobby);


            lobbyListSingleUI.AnimateIn(index * 0.06f);
            index++;
        }
    }

    private void RefreshButtonClick() {
        LobbyManager.Instance.RefreshLobbyList();


        refreshButton.transform.DOKill(true);
        refreshButton.transform.DOPunchScale(new Vector3(0.15f, 0.15f, 0f), 0.3f, 6);
    }

    private void CreateLobbyButtonClick() {
        LobbyCreateUI.Instance.Show();
    }

    private void Hide() {
        if (canvasGroup != null)
        {
            canvasGroup.DOKill();
            canvasGroup.transform.DOKill();
            canvasGroup.transform.DOScale(Vector3.one * 0.9f, 0.2f).SetEase(Ease.InBack);
            canvasGroup.DOFade(0f, 0.2f).OnComplete(() => gameObject.SetActive(false));
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void Show() {
        gameObject.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.transform.localScale = Vector3.one * 0.8f;
            canvasGroup.DOFade(1f, 0.3f);
            canvasGroup.transform.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack);
        }
    }
}