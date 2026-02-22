using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // Added DOTween!

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI resultTextMesh;
    [SerializeField] private Color winColor;
    [SerializeField] private Color loseColor;
    [SerializeField] private Color tieColor;
    [SerializeField] private Button rematchButton;

    [Header("Juice Settings")]
    [Tooltip("Add a CanvasGroup to this UI object to allow smooth fading")]
    [SerializeField] private CanvasGroup canvasGroup;

    private void Awake()
    {
        rematchButton.onClick.AddListener(() =>
        {
            GameManager.instance.RematchRpc();
            Hide();
        });
    }

    private void Start()
    {
        GameManager.instance.OnGameWin += GameManger_OnGameWin;
        GameManager.instance.OnRematch += Instance_OnRematch;
        GameManager.instance.OnGameTie += Instance_OnGameTie;

        Hide();
    }

    private void OnDestroy()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.OnGameWin -= GameManger_OnGameWin;
            GameManager.instance.OnRematch -= Instance_OnRematch;
            GameManager.instance.OnGameTie -= Instance_OnGameTie;
        }
    }

    private void Instance_OnGameTie(object sender, System.EventArgs e)
    {
        resultTextMesh.text = "TIE!";
        resultTextMesh.color = tieColor;

        // Wait 1 second before showing the tie screen
        DOVirtual.DelayedCall(1.0f, Show);
    }

    private void Instance_OnRematch(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void GameManger_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if (e.winPlayerType == GameManager.instance.GetLocalplayerType())
        {
            resultTextMesh.text = "YOU WIN!";
            resultTextMesh.color = winColor;
        }
        else
        {
            resultTextMesh.text = "YOU LOSE!";
            resultTextMesh.color = loseColor;
        }

        // Wait 1.5 seconds so the player can watch the winning line draw and pieces pop!
        DOVirtual.DelayedCall(1.5f, Show);
    }

    private void Show()
    {
        gameObject.SetActive(true);

        // 1. Reset everything before animating
        canvasGroup.alpha = 0f;
        resultTextMesh.transform.localScale = Vector3.zero;
        rematchButton.transform.localScale = Vector3.zero;

        // 2. Smoothly fade in the dark background
        canvasGroup.DOFade(1f, 0.4f);

        // 3. Slam the text into existence with a massive bounce
        resultTextMesh.transform.DOScale(Vector3.one, 0.6f).SetEase(Ease.OutBack).SetDelay(0.2f);

        // 4. Pop the rematch button in right after the text
        rematchButton.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack).SetDelay(0.5f);
    }

    private void Hide()
    {
        // Instantly hide and kill animations to reset for the next match
        transform.DOKill(true);
        gameObject.SetActive(false);
    }
}