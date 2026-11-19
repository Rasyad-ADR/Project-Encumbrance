using UnityEngine;
using UnityEngine.UI;

namespace Encumbrance.Resonance
{
    /// <summary>
    /// A temporary CanvasGroup + Image(Filled) UI. Shows briefly on value changes.
    /// </summary>
    public sealed class ResonanceBarView : MonoBehaviour
    {
        [SerializeField] private ResonanceController resonance;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image fillImage;
        [SerializeField, Min(0f)] private float visibleSeconds = 1.5f;
        [SerializeField, Min(0.01f)] private float fadeSeconds = 0.75f;

        private float lastChangeTime = float.NegativeInfinity;

        private void Awake()
        {
            if (canvasGroup != null) canvasGroup.alpha = 0f;
        }

        private void OnEnable()
        {
            if (resonance != null) resonance.ValueChanged += OnValueChanged;
        }

        private void OnDisable()
        {
            if (resonance != null) resonance.ValueChanged -= OnValueChanged;
        }

        private void Start()
        {
            if (resonance == null || canvasGroup == null || fillImage == null)
            {
                Debug.LogError("ResonanceBarView: assign Resonance, Canvas Group, and Fill Image.", this);
                return;
            }
            fillImage.type = Image.Type.Filled;
            fillImage.fillAmount = resonance.Normalized;
        }

        private void OnValueChanged(float current, float maximum)
        {
            if (fillImage != null) fillImage.fillAmount = maximum > 0f ? current / maximum : 0f;
            lastChangeTime = Time.unscaledTime;
            if (canvasGroup != null) canvasGroup.alpha = 1f;
        }

        private void Update()
        {
            if (canvasGroup == null) return;
            float elapsed = Time.unscaledTime - lastChangeTime;
            if (elapsed <= visibleSeconds) return;
            canvasGroup.alpha = 1f - Mathf.Clamp01((elapsed - visibleSeconds) / fadeSeconds);
        }
    }
}
