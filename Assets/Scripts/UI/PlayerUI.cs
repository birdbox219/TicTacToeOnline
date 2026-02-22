using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject crossArrowGameObject;
    [SerializeField] private GameObject circleArrowGameObject;
    [SerializeField] private GameObject crossTouTextGameOject;
    [SerializeField] private GameObject circleTouTextGameOject;
    [SerializeField] private TextMeshProUGUI circleScoreTextMesh;
    [SerializeField] private TextMeshProUGUI crossScoreTextMesh;

    [SerializeField] private Image crossImage;
    [SerializeField] private Image circleImage;


    private int lastCrossScore = 0;
    private int lastCircleScore = 0;



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



    private void OnDestroy()
    {
        // Always good practice to unhook events when the object is destroyed!
        if (GameManager.instance != null)
        {
            GameManager.instance.OnGameStarted -= Instance_OnGameStarted;
            GameManager.instance.OnCurrentPlayblePlayerTypeChanged -= Instance_OnCurrentPlayblePlayerTypeChanged;
            GameManager.instance.OnScoreChanged -= Instance_OnScoreChanged;
        }
    }

    private void Instance_OnScoreChanged(object sender, System.EventArgs e)
    {
        GameManager.instance.GetScores(out int crossScore, out int circleScore);

        crossScoreTextMesh.text = crossScore.ToString();
        circleScoreTextMesh.text = circleScore.ToString();


        if (crossScore > lastCrossScore)
        {
            crossScoreTextMesh.transform.DOKill(true); // Stop any playing animations
            crossScoreTextMesh.transform.DOPunchScale(new Vector3(0.5f, 0.5f, 0f), 0.4f, 6);
            crossScoreTextMesh.DOColor(Color.green, 0.4f).From(); // Flash green and fade back
        }

        if (circleScore > lastCircleScore)
        {
            circleScoreTextMesh.transform.DOKill(true);
            circleScoreTextMesh.transform.DOPunchScale(new Vector3(0.5f, 0.5f, 0f), 0.4f, 6);
            circleScoreTextMesh.DOColor(Color.green, 0.4f).From();
        }

        lastCrossScore = crossScore;
        lastCircleScore = circleScore;
    }

    

    private void Instance_OnCurrentPlayblePlayerTypeChanged(object sender, System.EventArgs e)
    {
        UpdateCurrentArrow();
        UpdateTurnVisuals();
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


        lastCrossScore = 0;
        lastCircleScore = 0;

        UpdateCurrentArrow();
        UpdateTurnVisuals();
    }


   

    private void UpdateCurrentArrow()
    {
            if(GameManager.instance.GetCurrentTurnPlayerType() == GameManager.PlayerType.Cross)
            {
                crossArrowGameObject.SetActive(true);
                circleArrowGameObject.SetActive(false);

                crossArrowGameObject.transform.DOKill(true);
                crossArrowGameObject.transform.localScale = Vector3.one; // Reset scale just in case
                crossArrowGameObject.transform.DOPunchScale(new Vector3(0.3f, 0.3f, 0f), 0.3f, 5);
        }
            else
            {
                crossArrowGameObject.SetActive(false);
                circleArrowGameObject.SetActive(true);

                circleArrowGameObject.transform.DOKill(true);
                circleArrowGameObject.transform.localScale = Vector3.one;
                circleArrowGameObject.transform.DOPunchScale(new Vector3(0.3f, 0.3f, 0f), 0.3f, 5);
        }
    }


    private void UpdateTurnVisuals()
    {
        if (GameManager.instance.GetCurrentTurnPlayerType() == GameManager.PlayerType.Cross)
        {
            // CROSS IS ACTIVE: Big, bouncy, and fully colored
            if (crossImage != null)
            {
                crossImage.transform.DOScale(Vector3.one * 1.3f, 0.4f).SetEase(Ease.OutBack);
                crossImage.DOColor(Color.white, 0.3f); // Assuming default is full color
            }

            // CIRCLE IS INACTIVE: Shrink down and turn gray/dim
            if (circleImage != null)
            {
                circleImage.transform.DOScale(Vector3.one * 0.8f, 0.3f).SetEase(Ease.OutCubic);
                circleImage.DOColor(new Color(0.4f, 0.4f, 0.4f, 0.8f), 0.3f); // Grayed out
            }

            
        }


        else if (GameManager.instance.GetCurrentTurnPlayerType() == GameManager.PlayerType.Cricle)
        {
            // CIRCLE IS ACTIVE
            if (circleImage != null)
            {
                circleImage.transform.DOScale(Vector3.one * 1.3f, 0.4f).SetEase(Ease.OutBack);
                circleImage.DOColor(Color.white, 0.3f);
            }

            // CROSS IS INACTIVE
            if (crossImage != null)
            {
                crossImage.transform.DOScale(Vector3.one * 0.8f, 0.3f).SetEase(Ease.OutCubic);
                crossImage.DOColor(new Color(0.4f, 0.4f, 0.4f, 0.8f), 0.3f);
            }

            
        }


    }





}

