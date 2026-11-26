using Encumbrance.Journal;
using Encumbrance.Resonance;
using Encumbrance.Stop;
using UnityEngine;

namespace Encumbrance.Weather
{
    // Visual only: never changes typing, weather, resonance or scene flow.
    public sealed class EndingFogController : MonoBehaviour
    {
        [Header("Systems")]
        [SerializeField] private TypingController typing;
        [SerializeField] private ResonanceController resonance;
        [SerializeField] private StopController stop;
        [SerializeField] private WeatherController weather;

        [Header("Fog UI")]
        [SerializeField] private RectTransform fogRect;
        [SerializeField] private CanvasGroup fogGroup;

        [Header("Progression")]
        [SerializeField, Min(0)] private int fogAfter = 3;
        [SerializeField, Min(0f)] private float smoothingSeconds = 0.5f;

        [Header("Appearance")]
        [SerializeField, Range(0f, 1f)] private float startOpacity = 0.15f;
        [SerializeField, Range(0f, 1f)] private float endOpacity = 1f;
        [SerializeField, Min(1f)] private float startScale = 1.2f;
        [SerializeField, Min(1f)] private float endScale = 1.6f;
        [SerializeField, Range(0f, 0.5f)] private float startHeightOffset = 0.10f;
        [SerializeField, Range(0f, 0.5f)] private float endHeightOffset = 0f;

        private float visible;
        private float progress;

        private void Awake()
        {
            if (typing == null || resonance == null || stop == null || weather == null ||
                fogRect == null || fogGroup == null)
            {
                Debug.LogError("EndingFog: assign all system and fog UI references.", this);
                if (fogGroup != null) fogGroup.alpha = 0f;
                enabled = false;
                return;
            }

            fogGroup.interactable = false;
            fogGroup.blocksRaycasts = false;
            ResetVisual();
        }

        private void LateUpdate()
        {
            if (resonance.Current <= 0f || typing.TotalSentenceCount <= fogAfter)
            {
                ResetVisual();
                return;
            }

            bool completed = typing.IsChapterCompleted;
            bool resolution = typing.IsReady && resonance.IsAscentActive &&
                !stop.IsAtStop && weather.EffectiveWeather == WeatherMode.Dry &&
                typing.CurrentSentenceIndex >= fogAfter;

            if (!completed && !resolution)
            {
                ResetVisual();
                return;
            }

            float target = 1f;
            if (!completed)
            {
                float fraction = typing.CurrentSentence.Length > 0
                    ? (float)typing.CurrentCharacterIndex / typing.CurrentSentence.Length : 0f;
                target = Mathf.Clamp01((typing.CurrentSentenceIndex - fogAfter + fraction) /
                    (typing.TotalSentenceCount - fogAfter));
            }

            float step = smoothingSeconds <= 0f ? 1f : Time.unscaledDeltaTime / smoothingSeconds;
            visible = Mathf.MoveTowards(visible, 1f, step);
            progress = Mathf.MoveTowards(progress, target, step);
            ApplyVisual();
        }

        private void ResetVisual()
        {
            visible = 0f;
            progress = 0f;
            ApplyVisual();
        }

        private void ApplyVisual()
        {
            if (fogRect == null || fogGroup == null) return;
            fogGroup.alpha = visible * Mathf.Lerp(startOpacity, endOpacity, progress);
            float scale = Mathf.Lerp(startScale, endScale, progress);
            fogRect.localScale = new Vector3(scale, scale, 1f);
            float height = fogRect.parent is RectTransform parent ? parent.rect.height : fogRect.rect.height;
            fogRect.anchoredPosition = new Vector2(0f,
                height * Mathf.Lerp(startHeightOffset, endHeightOffset, progress));
        }

        private void OnDisable()
        {
            ResetVisual();
        }
    }
}
