using UnityEngine;
using Unity.Netcode;
using DG.Tweening;

public class WinLineAnimation : NetworkBehaviour
{
    [SerializeField] private Transform visualTransform;

    public override void OnNetworkSpawn()
    {
        if (visualTransform == null) return;

        Vector3 finalScale = visualTransform.localScale;

        visualTransform.localScale = new Vector3(0, finalScale.y, finalScale.z);

        visualTransform.DOScaleX(finalScale.x, 3f).SetEase(Ease.OutQuint);
    }
}