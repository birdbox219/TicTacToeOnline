using UnityEngine;
using Unity.Netcode;
using DG.Tweening;

public class SpawnAnimation : NetworkBehaviour
{
    [Tooltip("Drag the child GameObject containing the Sprite/Mesh here")]
    [SerializeField] private Transform visualTransform;

    [Tooltip("Drag the SpriteRenderer of the child visual here")]
    [SerializeField] private SpriteRenderer spriteRenderer;


    [SerializeField] private ParticleSystem impactParticles;

    private Vector3 targetScale;

    private void Awake()
    {
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

        visualTransform.localScale = Vector3.zero;

        visualTransform.DORotate(new Vector3(0, 0, 360), 0.6f, RotateMode.FastBeyond360)
            .SetRelative(true)
            .SetEase(Ease.OutCubic);

        visualTransform.DOPunchPosition(new Vector3(0, 0, 0.5f), duration: 0.4f, vibrato: 5, elasticity: 0.5f);


        if (spriteRenderer != null)
        {
            Color targetColor = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            spriteRenderer.DOColor(targetColor, 0.4f).SetEase(Ease.OutFlash);
        }

        if (Camera.main != null)
        {
            Camera.main.DOComplete();
            Camera.main.DOShakePosition(0.15f, strength: 0.2f, vibrato: 10, randomness: 90);
        }

        visualTransform.DOScale(targetScale, 0.6f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {


                if (impactParticles != null)
                {
                    var mainModule = impactParticles.main;


                    if (gameObject.name.Contains("Cross"))
                    {
                        mainModule.startColor = Color.red;
                        Debug.Log("Setting impact particles to Red for " + gameObject.name);
                    }
                    else
                    {
                        mainModule.startColor = Color.blue;
                    }

                    impactParticles.Play();
                    Debug.Log("Playing impact particles on " + gameObject.name);
                }



                visualTransform.DOScale(targetScale * 1.05f, 1.5f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            });
    }


    [Rpc(SendTo.ClientsAndHost)]
    public void PlayWinSequenceRpc(float delay)
    {

        DOVirtual.DelayedCall(delay, () =>
        {
            if (visualTransform != null)
            {
                visualTransform.DOKill();


                visualTransform.DOScale(targetScale * 1.4f, 0.2f).SetEase(Ease.OutBack)
                    .OnComplete(() => visualTransform.DOScale(targetScale, 0.3f));


                if (spriteRenderer != null)
                {
                    spriteRenderer.DOColor(Color.yellow, 0.3f).SetEase(Ease.OutFlash);
                }
            }
        });
    }
}