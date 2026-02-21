using UnityEngine;
using Unity.Netcode;
using DG.Tweening;

public class SpawnAnimation : NetworkBehaviour
{
    [Tooltip("Drag the child GameObject containing the Sprite/Mesh here")]
    [SerializeField] private Transform visualTransform;

    [Tooltip("Drag the SpriteRenderer of the child visual here")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    //TO be impleneted in the future when we have particle effects ready, for now it just looks weird and out of place
    //[SerializeField] private ParticleSystem impactParticles;

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

        // This punches the whole object slightly downwards on the Z or Y axis 
        // (depending on if your game is 2D top-down or 3D) and makes it vibrate to a stop.
        // Assuming 2D, let's punch it slightly back on the Z axis:
        transform.DOPunchPosition(new Vector3(0, 0, 0.5f), duration: 0.4f, vibrato: 5, elasticity: 0.5f);

        // 3. The Pop-in: Scale the VISUAL from 0 up to its target scale
        visualTransform.DOScale(targetScale, 0.6f)
            .SetEase(Ease.OutBack);


        if (spriteRenderer != null)
        {
            // Save the target color (e.g., Red for Cross, Blue for Circle)
            Color targetColor = spriteRenderer.color;

            // Instantly turn it pure white
            spriteRenderer.color = Color.white;

            // Fade it back to its actual color over 0.4 seconds
            spriteRenderer.DOColor(targetColor, 0.4f).SetEase(Ease.OutFlash);
        }



        if (Camera.main != null)
        {
            Camera.main.DOComplete(); // Stops any active shakes so they don't stack wildly
            Camera.main.DOShakePosition(0.15f, strength: 0.2f, vibrato: 10, randomness: 90);
        }

        //visualTransform.DOScale(targetScale, 0.6f)
        //    .SetEase(Ease.OutBack)
        //    .OnComplete(() =>
        //    {
        //        if (impactParticles != null)
        //        {
        //            impactParticles.Play();
        //        }
        //    });



        // Wait for the spawn animation to finish (0.6s), then start a subtle breathing loop
        visualTransform.DOScale(targetScale * 1.05f, 1.5f)
            .SetDelay(0.6f) // Wait for the pop-in to finish
            .SetLoops(-1, LoopType.Yoyo) // -1 means infinite loops, Yoyo makes it go back and forth
            .SetEase(Ease.InOutSine); // Super smooth in and out
    }
}