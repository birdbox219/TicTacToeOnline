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

    // --- NEW: Track the currently hovered cell ---
    private bool isHoveringCell = false;
    private int currentHoverX;
    private int currentHoverY;
    private Vector3 currentHoverPos;

    private void OnEnable()
    {
        // Listen to the mouse entering and exiting cells
        GridPos.OnHoverEnter += GridPos_OnHoverEnter;
        GridPos.OnHoverExit += GridPos_OnHoverExit;

        // Listen to game state changes
        if (GameManager.instance != null)
        {
            GameManager.instance.OnPlaceObject += GameManager_OnPlaceObject;

            // --- NEW: Listen for when the turn changes ---
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

    // --- NEW: Triggered the moment the turn switches ---
    private void GameManager_OnTurnChanged(object sender, EventArgs e)
    {
        // If our mouse is already resting on a cell when our turn starts, manually refresh the ghost!
        if (isHoveringCell)
        {
            GridPos_OnHoverEnter(currentHoverX, currentHoverY, currentHoverPos);
        }
    }

    private void GridPos_OnHoverEnter(int x, int y, Vector3 cellWorldPosition)
    {
        // --- NEW: Remember where the mouse currently is ---
        isHoveringCell = true;
        currentHoverX = x;
        currentHoverY = y;
        currentHoverPos = cellWorldPosition;

        // 1. Don't show the ghost if it is NOT our turn
        if (GameManager.instance.GetCurrentTurnPlayerType() != GameManager.instance.GetLocalplayerType())
            return;

        // 2. Don't show the ghost if the cell is already taken
        if (!GameManager.instance.IsCellEmpty(x, y))
        {
            HideGhost();
            return;
        }

        // 3. Assign the correct sprite depending on who we are playing as
        ghostSpriteRenderer.sprite = GameManager.instance.GetLocalplayerType() == GameManager.PlayerType.Cross
            ? crossSprite
            : circleSprite;

        // 4. Smoothly Glide and Fade in!
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
        // --- NEW: Forget the hovered cell when we leave it ---
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