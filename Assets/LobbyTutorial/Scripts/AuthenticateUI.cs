using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class AuthenticateUI : MonoBehaviour {

    [SerializeField] private Button authenticateButton;

    [Header("Juice Settings")]
    [SerializeField] private CanvasGroup canvasGroup;

    private void Start() {
        authenticateButton.onClick.AddListener(() => {
            LobbyManager.Instance.Authenticate(EditPlayerName.Instance.GetPlayerName());
            Hide();
        });
    }

    private void OnEnable() {

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.transform.localScale = Vector3.one * 0.5f;
            canvasGroup.DOFade(1f, 0.4f);
            canvasGroup.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        }
    }

    private void Hide() {
        if (canvasGroup != null)
        {
            canvasGroup.DOKill();
            canvasGroup.transform.DOKill();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.transform.DOScale(Vector3.one * 0.8f, 0.25f).SetEase(Ease.InBack);
            canvasGroup.DOFade(0f, 0.25f).OnComplete(() => gameObject.SetActive(false));
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}