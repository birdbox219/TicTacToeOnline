using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.iOS;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private Button MainMenu;
    [SerializeField] private Button BackButton;


    private event EventHandler OnEscapeAction;


    private void Awake()
    {
     
        Hide();
    }



    private void Start()
    {

        
            MainMenu.onClick.AddListener(() =>
                {
                    GameManager.instance.DiconnectFromGame();
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
            if (gameObject.activeSelf)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }
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
