using UnityEngine;
using Unity.Netcode;
using DG.Tweening;

public class SpawnAnimation : NetworkBehaviour
{
    [Tooltip("Drag the child GameObject containing the Sprite/Mesh here")]
    [SerializeField] private Transform visualTransform;

    [Tooltip("Drag the SpriteRenderer of the child visual here")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    // TO be implemented in the future when we have particle effects ready, for now it just looks weird and out of place
    // [SerializeField] private ParticleSystem impactParticles;

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

        // [FIXED] Punch the local position of the visual child, not the networked parent!
        visualTransform.DOPunchPosition(new Vector3(0, 0, 0.5f), duration: 0.4f, vibrato: 5, elasticity: 0.5f);

        // 3. The Color Flash
        if (spriteRenderer != null)
        {
            Color targetColor = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            spriteRenderer.DOColor(targetColor, 0.4f).SetEase(Ease.OutFlash);
        }

        // 4. Camera Shake
        if (Camera.main != null)
        {
            Camera.main.DOComplete(); // Stops any active shakes so they don't stack wildly
            Camera.main.DOShakePosition(0.15f, strength: 0.2f, vibrato: 10, randomness: 90);
        }

        // 5. The Pop-in AND Breathing Loop [FIXED]
        // We chain the breathing loop so it only starts AFTER the pop-in is 100% finished.
        visualTransform.DOScale(targetScale, 0.6f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                // --- Future Particles ---
                // if (impactParticles != null)
                // {
                //     impactParticles.Play();
                // }

                // Start the subtle breathing loop
                visualTransform.DOScale(targetScale * 1.05f, 1.5f)
                    .SetLoops(-1, LoopType.Yoyo) // -1 means infinite loops
                    .SetEase(Ease.InOutSine); // Super smooth in and out
            });
    }


    [Rpc(SendTo.ClientsAndHost)]
    public void PlayWinSequenceRpc(float delay)
    {
        // DOVirtual.DelayedCall lets us wait for the exact moment the line touches this piece
        DOVirtual.DelayedCall(delay, () =>
        {
            if (visualTransform != null)
            {
                // 1. Kill the infinite breathing loop so it doesn't fight our animation
                visualTransform.DOKill();

                // 2. Do a massive bouncy pop upwards
                visualTransform.DOScale(targetScale * 1.4f, 0.2f).SetEase(Ease.OutBack)
                    .OnComplete(() => visualTransform.DOScale(targetScale, 0.3f));

                // 3. Flash the piece gold/yellow to highlight it!
                if (spriteRenderer != null)
                {
                    spriteRenderer.DOColor(Color.yellow, 0.3f).SetEase(Ease.OutFlash);
                }
            }
        });
    }
}