using Encumbrance.Resonance;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Encumbrance.Hazard
{
    public sealed class JournalWetness : MonoBehaviour
    {
        [SerializeField] private ResonanceController resonance;
        [SerializeField] private TMP_Text wetnessText;
        [SerializeField] private Image wetnessFill;
        [SerializeField] private Image wetOverlay;
        [SerializeField, Range(0, 1)] private float maxOverlayAlpha = 0.65f;
        [SerializeField, Min(0)] private float missedPenalty = 15f;
        [SerializeField, Min(0)] private float wipeRecovery = 5f;
        [SerializeField, Min(0)] private float criticalDrainPerSecond = 1f;
        public float Current { get; private set; }
        public float Normalized => Current / 100f;
        private void Awake() => Refresh();
        public void ResetWetness() { Current = 0f; Refresh(); }
        public void Missed() => Change(missedPenalty);
        public void Wiped() => Change(-wipeRecovery);
        private void Change(float amount)
        {
            Current = Mathf.Clamp(Current + amount, 0f, 100f);
            Refresh();
        }
        private void Update()
        {
            if (Current >= 100f && resonance != null && resonance.IsAscentActive)
                resonance.ApplyLoss(criticalDrainPerSecond * Time.deltaTime);
        }
        private void Refresh()
        {
            if (wetnessFill != null) wetnessFill.fillAmount = Normalized;
            if (wetnessText != null) wetnessText.text = $"Wetness: {Current:0}%";
            if (wetOverlay != null)
            {
                Color color = wetOverlay.color;
                color.a = Normalized * maxOverlayAlpha;
                wetOverlay.color = color;
                wetOverlay.raycastTarget = false;
            }
        }
    }
}
