using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI resultTextMesh;
    [SerializeField] private Color winColor;
    [SerializeField] private Color loseColor;
    [SerializeField] private Color tieColor;
    [SerializeField] private Button rematchButton;


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

    private void Instance_OnGameTie(object sender, System.EventArgs e)
    {
        resultTextMesh.text = "TIE!";
        resultTextMesh.color = tieColor;
        Show();
    }

    private void Instance_OnRematch(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void GameManger_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if (e.winPlayerType == GameManager.instance.GetLocalplayerType())
        {             resultTextMesh.text = "YOU WIN!";
            resultTextMesh.color = winColor;
        }
        else
        {
            resultTextMesh.text = "YOU LOSE!";
            resultTextMesh.color = loseColor;
        }
        Show();
    }

    private void Show()
    {
               gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
