using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject _lobbyListUI;

    [SerializeField] private Button _playButton;
    [SerializeField] private Button _quitButton;

    [SerializeField] private Button _BackButtonInLobby;

    private bool _isTransitioning = false;

    private void Start()
    {
        // Start with lobby hidden, main menu visible
        _lobbyListUI.SetActive(false);

        _playButton.gameObject.SetActive(true);
        _quitButton.gameObject.SetActive(true);

        // Staggered entrance animation for main menu buttons
        PlayEntranceAnimation();

        _playButton.onClick.AddListener(() =>
        {
            if (!_isTransitioning) ShowLobby();
        });

        _quitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });

        _BackButtonInLobby.onClick.AddListener(() =>
        {
            if (!_isTransitioning) ShowMainMenu();
        });
    }

    private void PlayEntranceAnimation()
    {
        // Staggered pop-in for each button
        _playButton.transform.localScale = Vector3.zero;
        _quitButton.transform.localScale = Vector3.zero;

        _playButton.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetDelay(0.2f);
        _quitButton.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetDelay(0.4f);
    }

    private void ShowMainMenu()
    {
        _isTransitioning = true;

        // Kill any active animations
        _playButton.transform.DOKill();
        _quitButton.transform.DOKill();

        // Hide lobby, show main menu buttons
        _lobbyListUI.SetActive(false);

        _playButton.gameObject.SetActive(true);
        _quitButton.gameObject.SetActive(true);

        // Stagger button re-entrance
        _playButton.transform.localScale = Vector3.zero;
        _quitButton.transform.localScale = Vector3.zero;
        _playButton.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack).SetDelay(0.1f);
        _quitButton.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack).SetDelay(0.2f)
            .OnComplete(() => _isTransitioning = false);
    }

    private void ShowLobby()
    {
        _isTransitioning = true;

        // Kill any active animations
        _playButton.transform.DOKill();
        _quitButton.transform.DOKill();

        // Animate buttons out, then switch
        _playButton.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack);
        _quitButton.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                _playButton.gameObject.SetActive(false);
                _quitButton.gameObject.SetActive(false);

                // Show lobby
                _lobbyListUI.SetActive(true);
                _isTransitioning = false;
            });
    }
}
