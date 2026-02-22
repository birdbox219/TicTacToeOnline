using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening; // Don't forget the DOTween namespace!

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private Button MainMenu;
    [SerializeField] private Button BackButton;
    [Tooltip("Make sure this CanvasGroup is attached to the actual visual Panel you want to animate!")]
    [SerializeField] private CanvasGroup canva;

    // We use a boolean to track the state instead of gameObject.activeSelf
    private bool isOpen = false;

    private void Start()
    {
        // 1. Force the UI to start hidden and shrunk
        if (canva != null)
        {
            canva.alpha = 0f;
            canva.interactable = false;
            canva.blocksRaycasts = false;
            canva.transform.localScale = Vector3.zero;
        }

        MainMenu.onClick.AddListener(() =>
        {
            // Assuming this is a static method in your GameManager
            GameManager.DisconnectAndReturnToMenu();
        });

        BackButton.onClick.AddListener(() =>
        {
            Hide();
        });
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isOpen)
            {
                Debug.Log("Escape pressed, hiding settings UI");
                Hide();
            }
            else
            {
                Debug.Log("Escape pressed, showing settings UI");
                Show();
            }
        }
    }

    private void Show()
    {
        if (isOpen) return;
        isOpen = true;

        // 1. Instantly allow button clicks again
        canva.interactable = true;
        canva.blocksRaycasts = true;

        // 2. Kill any active animations so they don't fight each other if you spam Escape
        canva.transform.DOKill();
        canva.DOKill();

        // 3. The Pop-In Animation!
        // Start slightly smaller, then snap to full size with a satisfying bounce
        canva.transform.localScale = Vector3.one * 0.5f;
        canva.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack).SetUpdate(true);

        // 4. Fade in the opacity
        canva.DOFade(1f, 0.3f).SetUpdate(true);
    }

    private void Hide()
    {
        if (!isOpen) return;
        isOpen = false;

        // 1. Instantly block clicks so the player can't click buttons while it's disappearing
        canva.interactable = false;
        canva.blocksRaycasts = false;

        canva.transform.DOKill();
        canva.DOKill();

        // 2. The Pop-Out Animation!
        // Shrink down slightly while pulling back, using Ease.InBack
        canva.transform.DOScale(Vector3.one * 0.8f, 0.3f).SetEase(Ease.InBack).SetUpdate(true);

        // 3. Fade out the opacity
        canva.DOFade(0f, 0.3f).SetUpdate(true);
    }
}