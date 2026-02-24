using UnityEngine;
using UnityEngine.EventSystems;

public class GridPos : MonoBehaviour, IPointerDownHandler
{

    

    [SerializeField] private int x;
    [SerializeField] private int y;
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"Grid Position Clicked: {gameObject.name} at ({x} , {y})");

        GameManager.instance.ClickedOnGridPosRpc(x, y , GameManager.instance.GetLocalplayerType());
    }
}
