using UnityEngine;
using Unity.Netcode;
using DG.Tweening;

public class SpawnAnimation : NetworkBehaviour
{
    [Tooltip("Drag the child GameObject containing the Sprite/Mesh here")]
    [SerializeField] private Transform visualTransform;

    private Vector3 targetScale;

    private void Awake()
    {
        // Capture the visual's default scale from the editor
        if (visualTransform != null)
        {
            targetScale = visualTransform.localScale;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (visualTransform == null)
        {
            Debug.LogWarning("Visual Transform is not assigned on " + gameObject.name);
            return;
        }

        // 1. Immediately shrink the VISUAL to size 0
        visualTransform.localScale = Vector3.zero;

        // 2. The Spin: Rotate the VISUAL 360 degrees
        visualTransform.DORotate(new Vector3(0, 0, 360), 0.6f, RotateMode.FastBeyond360)
            .SetRelative(true)
            .SetEase(Ease.OutCubic);

        // 3. The Pop-in: Scale the VISUAL from 0 up to its target scale
        visualTransform.DOScale(targetScale, 0.6f)
            .SetEase(Ease.OutBack);



        if (Camera.main != null)
        {
            Camera.main.DOComplete(); // Stops any active shakes so they don't stack wildly
            Camera.main.DOShakePosition(0.15f, strength: 0.2f, vibrato: 10, randomness: 90);
        }



        // Wait for the spawn animation to finish (0.6s), then start a subtle breathing loop
        visualTransform.DOScale(targetScale * 1.05f, 1.5f)
            .SetDelay(0.6f) // Wait for the pop-in to finish
            .SetLoops(-1, LoopType.Yoyo) // -1 means infinite loops, Yoyo makes it go back and forth
            .SetEase(Ease.InOutSine); // Super smooth in and out
    }
}