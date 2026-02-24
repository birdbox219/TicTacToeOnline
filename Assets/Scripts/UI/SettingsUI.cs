using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private Button MainMenu;
    [SerializeField] private Button BackButton;
    [Tooltip("Make sure this CanvasGroup is attached to the actual visual Panel you want to animate!")]
    [SerializeField] private CanvasGroup canva;


    private bool isOpen = false;

    private void Start()
    {

        if (canva != null)
        {
            canva.alpha = 0f;
            canva.interactable = false;
            canva.blocksRaycasts = false;
            canva.transform.localScale = Vector3.zero;
        }

        MainMenu.onClick.AddListener(() =>
        {

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


        canva.interactable = true;
        canva.blocksRaycasts = true;

        canva.transform.DOKill();
        canva.DOKill();

        canva.transform.localScale = Vector3.one * 0.5f;
        canva.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack).SetUpdate(true);

        canva.DOFade(1f, 0.3f).SetUpdate(true);
    }

    private void Hide()
    {
        if (!isOpen) return;
        isOpen = false;


        canva.interactable = false;
        canva.blocksRaycasts = false;

        canva.transform.DOKill();
        canva.DOKill();

        canva.transform.DOScale(Vector3.one * 0.8f, 0.3f).SetEase(Ease.InBack).SetUpdate(true);


        canva.DOFade(0f, 0.3f).SetUpdate(true);
    }
}