using System;
using UnityEngine;

namespace Encumbrance.Resonance
{
    /// <summary>
    /// Runtime Resonance value. Does not know about journal, UI, or inventory.
    /// RestoreAtStop is the only recovery API; do not call during an ascent.
    /// </summary>
    public sealed class ResonanceController : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float maximum = 100f;
        [SerializeField] private bool startAscentOnPlay = true;
        [SerializeField] private bool idleDrainEnabled = false;
        [SerializeField, Min(0f)] private float idleDrainPerSecond = 2f;
        [SerializeField] private bool passiveDrainEnabled = false;
        [SerializeField, Min(0f)] private float passiveDrainPerSecond = 1f;

        public event Action<float, float> ValueChanged; // current, maximum
        public event Action Depleted;

        public float Current { get; private set; }
        public float Maximum => maximum;
        public float Normalized => maximum > 0f ? Current / maximum : 0f;
        public bool IsAscentActive { get; private set; }
        public bool IsTypingIdle { get; private set; } = true;
        public bool Immortal { get; set; }

        private bool depletedNotified;

        private void Awake()
        {
            maximum = Mathf.Max(1f, maximum);
            Current = maximum;
            IsAscentActive = startAscentOnPlay;
        }

        private void Update()
        {
            if (!IsAscentActive || Current <= 0f) return;
            float rate = (passiveDrainEnabled ? passiveDrainPerSecond : 0f) +
                         (idleDrainEnabled && IsTypingIdle ? idleDrainPerSecond : 0f);
            if (rate > 0f) ApplyLoss(rate * Time.deltaTime);
        }

        public void ApplyLoss(float amount)
        {
            if (Immortal) return;   // <-- tambahan
            if (!IsAscentActive || Current <= 0f || amount <= 0f || float.IsNaN(amount)) return;
            float old = Current;
            Current = Mathf.Max(0f, Current - amount);
            if (Mathf.Approximately(old, Current)) return;
            ValueChanged?.Invoke(Current, maximum);
            if (Current <= 0f && !depletedNotified)
            {
                depletedNotified = true;
                IsAscentActive = false;
                Depleted?.Invoke();
            }
        }

        public void SetTypingIdle(bool idle) => IsTypingIdle = idle;
        public void EndAscent() => IsAscentActive = false;

        // Called by a future Stop/camp system, NEVER by the typing component.
        public void RestoreAtStop()
        {
            IsAscentActive = false;
            IsTypingIdle = true;
            depletedNotified = false;
            Current = maximum;
            ValueChanged?.Invoke(Current, maximum);
        }

        public void StartAscent()
        {
            if (Current <= 0f)
            {
                Debug.LogWarning("Resonance: restore at a Stop before starting an ascent.", this);
                return;
            }
            IsTypingIdle = true;
            IsAscentActive = true;
        }
    }
}
