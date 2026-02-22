using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject _lobbyListUI;
    

    [SerializeField] private Button _playButton;
    [SerializeField] private Button _quitButton;

    [SerializeField] private Button _BackButtonInLobby;



    private void Start()
    {
        ShowMainMenu();

        _playButton.onClick.AddListener(() =>
        {
            ShowLobby();
        });

        _quitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });

        _BackButtonInLobby.onClick.AddListener(() =>
        {
            ShowMainMenu();
        });
    }

    private void ShowMainMenu()
    {
        _lobbyListUI.SetActive(false);

        _playButton.gameObject.SetActive(true);
        _quitButton.gameObject.SetActive(true);
    }

    private void ShowLobby()
    {
        _lobbyListUI.SetActive(true);

        _playButton.gameObject.SetActive(false);
        _quitButton.gameObject.SetActive(false);
    }


}
