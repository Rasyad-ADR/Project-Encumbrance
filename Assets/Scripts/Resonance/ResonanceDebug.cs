using UnityEngine;

namespace Encumbrance.Resonance
{
    /// <summary>Optional diagnostic subscriber; remove/disable after testing.</summary>
    public sealed class ResonanceDebug : MonoBehaviour
    {
        [SerializeField] private ResonanceController resonance;

        private void OnEnable()
        {
            if (resonance == null) return;
            resonance.ValueChanged += LogValue;
            resonance.Depleted += LogDepleted;
        }

        private void OnDisable()
        {
            if (resonance == null) return;
            resonance.ValueChanged -= LogValue;
            resonance.Depleted -= LogDepleted;
        }

        private void LogValue(float current, float maximum) =>
            Debug.Log($"Resonance: {current:0.0} / {maximum:0.0}", this);

        private void LogDepleted() =>
            Debug.Log("Resonance depleted! Lose-state integration TODO.", this);
    }
}
