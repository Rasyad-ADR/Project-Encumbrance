using System.Collections.Generic;
using Encumbrance.Journal;
using Encumbrance.Resonance;
using Encumbrance.Stop;
using UnityEngine;

namespace Encumbrance.Weather
{
    public sealed class ChapterAtmosphereController : MonoBehaviour
    {
        [SerializeField] private TypingController typing;
        [SerializeField] private WeatherController weather;
        [SerializeField] private ResonanceController resonance;
        [SerializeField] private StopController stop;
        [Header("Progress (completed sentences)")]
        [SerializeField, Min(1)] private int rainAfter = 1;
        [SerializeField, Min(2)] private int clearAfter = 3;
        [Header("Tint roots (include recycling parents)")]
        [SerializeField] private Transform[] tintRoots;
        [SerializeField] private Color beginningTint = new Color32(171,177,193,255);
        [SerializeField] private Color rainTint = new Color32(104,117,143,255);
        [SerializeField] private Color resolutionTint = Color.white;
        [SerializeField, Min(0f)] private float transitionSeconds = 4f;
        private Color currentTint, fromTint, targetTint;
        private float elapsed;
        private int phase = -1;
        private readonly List<SpriteRenderer> renderers = new List<SpriteRenderer>();

        private void Awake()
        {
            currentTint = fromTint = targetTint = beginningTint;
            elapsed = transitionSeconds;
            if (typing == null || weather == null || resonance == null || stop == null)
                Debug.LogError("ChapterAtmosphere: assign typing, weather, resonance and stop.", this);
            if (weather != null) weather.SetDry();
            ApplyTint();
        }
        private void OnValidate()
        {
            rainAfter = Mathf.Max(1, rainAfter);
            clearAfter = Mathf.Max(rainAfter + 1, clearAfter);
        }
        private int GetPhase()
        {
            int completed = typing != null ? typing.CurrentSentenceIndex : 0;
            return completed >= clearAfter ? 2 : completed >= rainAfter ? 1 : 0;
        }
        // Stop calls this before restarting ascent, so previous rain cannot leak into resolution.
        public void ApplyProgressWeather()
        {
            if (weather == null) return;
            var mode = GetPhase() == 1 ? WeatherMode.HeavyRain : WeatherMode.Dry;
            if (weather.Current != mode) weather.SetWeather(mode);
        }
        private void LateUpdate()
        {
            if (typing != null && resonance != null && resonance.IsAscentActive &&
                typing.IsReady && !typing.IsChapterCompleted && (stop == null || !stop.IsAtStop))
            {
                int next = GetPhase();
                if (phase != next)
                {
                    phase = next;
                    fromTint = currentTint;
                    targetTint = phase == 0 ? beginningTint : phase == 1 ? rainTint : resolutionTint;
                    elapsed = 0f;
                }
                ApplyProgressWeather();
                elapsed += Time.deltaTime;
                float t = transitionSeconds <= 0f ? 1f : Mathf.Clamp01(elapsed / transitionSeconds);
                currentTint = Color.Lerp(fromTint, targetTint, Mathf.SmoothStep(0f, 1f, t));
            }
            // Also tint newly instantiated and inactive recycling templates.
            ApplyTint();
        }
        private void ApplyTint()
        {
            if (tintRoots == null) return;
            foreach (Transform root in tintRoots)
            {
                if (root == null) continue;
                renderers.Clear();
                root.GetComponentsInChildren<SpriteRenderer>(true, renderers);
                foreach (SpriteRenderer sprite in renderers)
                {
                    if (sprite == null) continue;
                    Color c = currentTint;
                    c.a = sprite.color.a;
                    sprite.color = c;
                }
            }
        }
    }
}
