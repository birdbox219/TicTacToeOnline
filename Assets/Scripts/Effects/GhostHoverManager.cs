using UnityEngine;
using DG.Tweening;
using System;

public class GhostHoverManager : MonoBehaviour
{
    [Header("Visuals")]
    [Tooltip("The SpriteRenderer that will display the ghost piece")]
    [SerializeField] private SpriteRenderer ghostSpriteRenderer;

    [Tooltip("Drag your 2D sprites here")]
    [SerializeField] private Sprite crossSprite;
    [SerializeField] private Sprite circleSprite;


    private bool isHoveringCell = false;
    private int currentHoverX;
    private int currentHoverY;
    private Vector3 currentHoverPos;

    private void OnEnable()
    {
        GridPos.OnHoverEnter += GridPos_OnHoverEnter;
        GridPos.OnHoverExit += GridPos_OnHoverExit;

        if (GameManager.instance != null)
        {
            GameManager.instance.OnPlaceObject += GameManager_OnPlaceObject;

            GameManager.instance.OnCurrentPlayblePlayerTypeChanged += GameManager_OnTurnChanged;
        }
    }

    private void OnDisable()
    {
        GridPos.OnHoverEnter -= GridPos_OnHoverEnter;
        GridPos.OnHoverExit -= GridPos_OnHoverExit;

        if (GameManager.instance != null)
        {
            GameManager.instance.OnPlaceObject -= GameManager_OnPlaceObject;
            GameManager.instance.OnCurrentPlayblePlayerTypeChanged -= GameManager_OnTurnChanged;
        }
    }

    private void Start()
    {
        ghostSpriteRenderer.color = new Color(1, 1, 1, 0);
        transform.localScale = new Vector3(2.6f, 2.6f, 1f);
    }


    private void GameManager_OnTurnChanged(object sender, EventArgs e)
    {
        if (isHoveringCell)
        {
            GridPos_OnHoverEnter(currentHoverX, currentHoverY, currentHoverPos);
        }
    }

    private void GridPos_OnHoverEnter(int x, int y, Vector3 cellWorldPosition)
    {

        isHoveringCell = true;
        currentHoverX = x;
        currentHoverY = y;
        currentHoverPos = cellWorldPosition;

        if (GameManager.instance.GetCurrentTurnPlayerType() != GameManager.instance.GetLocalplayerType())
            return;

        if (!GameManager.instance.IsCellEmpty(x, y))
        {
            HideGhost();
            return;
        }

        ghostSpriteRenderer.sprite = GameManager.instance.GetLocalplayerType() == GameManager.PlayerType.Cross
            ? crossSprite
            : circleSprite;


        if (ghostSpriteRenderer.color.a == 0f)
        {
            transform.position = cellWorldPosition;
        }
        else
        {
            transform.DOKill();
            transform.DOMove(cellWorldPosition, 0.15f).SetEase(Ease.OutCubic);
        }

        ghostSpriteRenderer.DOFade(0.3f, 0.15f);
    }

    private void GridPos_OnHoverExit()
    {

        isHoveringCell = false;
        HideGhost();
    }

    private void GameManager_OnPlaceObject(object sender, EventArgs e)
    {
        HideGhost();
    }

    private void HideGhost()
    {
        ghostSpriteRenderer.DOFade(0f, 0.15f);
    }
}