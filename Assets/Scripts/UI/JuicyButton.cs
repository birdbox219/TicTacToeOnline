using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


[RequireComponent(typeof(Button))]
public class JuicyButton : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler
{
    [Header("Hover")]
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float hoverDuration = 0.2f;

    [Header("Press")]
    [SerializeField] private float pressScale = 0.92f;
    [SerializeField] private float pressDuration = 0.1f;

    [Header("Click Punch")]
    [SerializeField] private float punchStrength = 0.15f;
    [SerializeField] private float punchDuration = 0.3f;
    [SerializeField] private int punchVibrato = 6;

    private Vector3 _originalScale;
    private Button _button;
    private bool _isHovered = false;

    private void Awake()
    {
        _originalScale = transform.localScale;
        _button = GetComponent<Button>();


        _button.onClick.AddListener(OnClick);
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_button.interactable) return;
        _isHovered = true;

        transform.DOKill();
        transform.DOScale(_originalScale * hoverScale, hoverDuration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered = false;

        transform.DOKill();
        transform.DOScale(_originalScale, hoverDuration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_button.interactable) return;

        transform.DOKill();
        transform.DOScale(_originalScale * pressScale, pressDuration)
            .SetEase(Ease.InCubic)
            .SetUpdate(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_button.interactable) return;

        transform.DOKill();
        float targetScale = _isHovered ? hoverScale : 1f;
        transform.DOScale(_originalScale * targetScale, pressDuration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true);
    }

    private void OnClick()
    {
        transform.DOKill();
        transform.localScale = _originalScale;
        transform.DOPunchScale(Vector3.one * punchStrength, punchDuration, punchVibrato)
            .SetUpdate(true);

        ISoundManager.PlaySound(SoundType.AdjustingAudioLevel, 0.5f);
    }
}
