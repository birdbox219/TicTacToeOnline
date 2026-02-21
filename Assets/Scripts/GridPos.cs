using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class GridPos : MonoBehaviour, IPointerDownHandler , IPointerEnterHandler , IPointerExitHandler
{
    [SerializeField] private int x;
    [SerializeField] private int y;



    public static event Action<int, int, Vector3> OnHoverEnter;
    public static event Action OnHoverExit;


    /// <summary>
    /// Initialize grid position at runtime (used by GridSpawner).
    /// </summary>
    public void Initialize(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public int X => x;
    public int Y => y;

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"Grid Position Clicked: {gameObject.name} at ({x} , {y})");

        GameManager.instance.ClickedOnGridPosRpc(x, y , GameManager.instance.GetLocalplayerType());
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        OnHoverEnter?.Invoke(x, y, transform.position);
    }

    // Triggers when the mouse leaves this cell
    public void OnPointerExit(PointerEventData eventData)
    {
        OnHoverExit?.Invoke();
    }
}
