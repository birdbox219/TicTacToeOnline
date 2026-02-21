using Unity.VisualScripting;
using UnityEngine;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject crossArrowGameObject;
    [SerializeField] private GameObject circleArrowGameObject;
    [SerializeField] private GameObject crossTouTextGameOject;
    [SerializeField] private GameObject circleTouTextGameOject;
    [SerializeField] private TextMeshProUGUI circleScoreTextMesh;
    [SerializeField] private TextMeshProUGUI crossScoreTextMesh;




    private void Awake()
    {
        crossArrowGameObject.SetActive(false);
        circleArrowGameObject.SetActive(false);
        crossTouTextGameOject.SetActive(false);
        circleTouTextGameOject.SetActive(false);


        crossScoreTextMesh.text = "";
        circleScoreTextMesh.text = "";
    }

    private void Start()
    {


        GameManager.instance.OnGameStarted += Instance_OnGameStarted;

        GameManager.instance.OnCurrentPlayblePlayerTypeChanged += Instance_OnCurrentPlayblePlayerTypeChanged ;
        GameManager.instance.OnScoreChanged += Instance_OnScoreChanged;
    }

    private void Instance_OnScoreChanged(object sender, System.EventArgs e)
    {
        GameManager.instance.GetScores(out int crossScore, out int circleScore);

        crossScoreTextMesh.text = crossScore.ToString();
        circleScoreTextMesh.text = circleScore.ToString();
    }

    

    private void Instance_OnCurrentPlayblePlayerTypeChanged(object sender, System.EventArgs e)
    {
        UpdateCurrentArrow();
    }

    private void Instance_OnGameStarted(object sender, System.EventArgs e)
    {
        if (GameManager.instance.GetLocalplayerType() == GameManager.PlayerType.Cross)
        {
            //crossArrowGameObject.SetActive(true);
            crossTouTextGameOject.SetActive(true);
        }
        else
        {
            //circleArrowGameObject.SetActive(true);
            circleTouTextGameOject.SetActive(true);
        }


        circleScoreTextMesh.text = "0";
        crossScoreTextMesh.text = "0";

        UpdateCurrentArrow();
    }


   

    private void UpdateCurrentArrow()
    {
            if(GameManager.instance.GetCurrentTurnPlayerType() == GameManager.PlayerType.Cross)
            {
                crossArrowGameObject.SetActive(true);
                circleArrowGameObject.SetActive(false);
            }
            else
            {
                crossArrowGameObject.SetActive(false);
                circleArrowGameObject.SetActive(true);
        }
    }



    //private void HideChildren(Transform parent)
    //    {
    //    foreach (Transform child in parent)
    //    {
    //        child.gameObject.SetActive(false);

    //        HideChildren(child);
    //    }


    //}
    
}

