using Encumbrance.Journal;
using Encumbrance.Resonance;
using Encumbrance.Stop;
using Encumbrance.Weather;
using UnityEngine;

namespace Encumbrance.Audio
{
    public sealed class GameplayMusicController : MonoBehaviour
    {
        [Header("Systems")]
        [SerializeField] private TypingController typing;
        [SerializeField] private ResonanceController resonance;
        [SerializeField] private WeatherController weather;
        [SerializeField] private StopController stop;
        [Header("Music clips (optional)")]
        [SerializeField] private AudioClip beginningMusic;
        [SerializeField] private AudioClip conflictMusic;
        [SerializeField] private AudioClip resolutionEndingMusic;
        [Header("Progress (match ChapterAtmosphereController)")]
        [SerializeField, Min(1)] private int clearAfter = 3;
        [Header("Mix")]
        [SerializeField, Range(0f, 1f)] private float beginningVolume = .25f;
        [SerializeField, Range(0f, 1f)] private float conflictVolume = .20f;
        [SerializeField, Range(0f, 1f)] private float resolutionVolume = .35f;
        [SerializeField, Range(0f, 1f)] private float endingVolume = .50f;
        [SerializeField, Min(0f)] private float crossfadeSeconds = 2f;
        private enum Phase { Silent, Beginning, Conflict, Resolution, Ending }
        private Phase phase;
        private AudioSource beginning, conflict, resolution;
        private bool openingBlocked;
        private float elapsed, bFrom, cFrom, rFrom;

        private void Awake()
        {
            beginning = MakeSource("Beginning BGM", beginningMusic);
            conflict = MakeSource("Conflict BGM", conflictMusic);
            resolution = MakeSource("Resolution Ending BGM", resolutionEndingMusic);
            if (typing == null || resonance == null || weather == null || stop == null)
                Debug.LogWarning("GameplayMusic: assign Typing, Resonance, Weather and Stop.", this);
        }
        private AudioSource MakeSource(string label, AudioClip clip)
        {
            var child = new GameObject(label);
            child.transform.SetParent(transform, false);
            var source = child.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.loop = true;
            source.clip = clip;
            source.volume = 0f;
            return source;
        }
        // Optional UnityEvent hooks for P2's opening controller.
        public void BlockForOpening() => openingBlocked = true;
        public void AllowAfterOpening() => openingBlocked = false;
        private Phase GetPhase()
        {
            if (typing == null || resonance == null || weather == null || stop == null || openingBlocked)
                return Phase.Silent;
            if (resonance.Current <= 0f) return Phase.Silent;
            if (typing.IsChapterCompleted) return Phase.Ending;
            if (stop.IsAtStop || !resonance.IsAscentActive || !typing.IsReady) return Phase.Silent;
            if (weather.EffectiveWeather != WeatherMode.Dry) return Phase.Conflict;
            return typing.CurrentSentenceIndex >= clearAfter ? Phase.Resolution : Phase.Beginning;
        }
        private void LateUpdate()
        {
            Phase next = GetPhase();
            if (next != phase)
            {
                phase = next;
                elapsed = 0f;
                bFrom = beginning.volume;
                cFrom = conflict.volume;
                rFrom = resolution.volume;
            }
            elapsed += Time.unscaledDeltaTime;
            float t = crossfadeSeconds <= 0f ? 1f : Mathf.Clamp01(elapsed / crossfadeSeconds);
            Mix(beginning, bFrom, phase == Phase.Beginning ? beginningVolume : 0f, t);
            Mix(conflict, cFrom, phase == Phase.Conflict ? conflictVolume : 0f, t);
            Mix(resolution, rFrom, phase == Phase.Resolution ? resolutionVolume : phase == Phase.Ending ? endingVolume : 0f, t);
        }
        private static void Mix(AudioSource source, float from, float target, float t)
        {
            if (source.clip == null) return;
            if (target > 0f && !source.isPlaying) source.Play();
            source.volume = Mathf.Lerp(from, target, t);
            if (target <= 0f && t >= 1f && source.isPlaying) source.Stop();
        }
        private void OnDisable()
        {
            Silence(beginning); Silence(conflict); Silence(resolution);
            phase = Phase.Silent;
            elapsed = bFrom = cFrom = rFrom = 0f;
        }
        private static void Silence(AudioSource source)
        {
            if (source == null) return;
            source.Stop();
            source.volume = 0f;
        }
    }
}
