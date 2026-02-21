using UnityEngine;
using Unity.Netcode;
using DG.Tweening;

public class WinLineAnimation : NetworkBehaviour
{
    [SerializeField] private Transform visualTransform;

    public override void OnNetworkSpawn()
    {
        if (visualTransform == null) return;

        // Save the final scale that the server calculated
        Vector3 finalScale = visualTransform.localScale;

        // Set the X scale to 0 so it starts completely collapsed
        visualTransform.localScale = new Vector3(0, finalScale.y, finalScale.z);

        // Tween the X scale back to its full length over 0.5 seconds
        // Ease.OutQuint gives it a fast, aggressive start that slows down smoothly
        visualTransform.DOScaleX(finalScale.x, 3f).SetEase(Ease.OutQuint);
    }
}