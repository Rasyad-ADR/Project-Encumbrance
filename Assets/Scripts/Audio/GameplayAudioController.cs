using Encumbrance.Journal;
using Encumbrance.Resonance;
using Encumbrance.Weather;
using UnityEngine;

namespace Encumbrance.Audio
{
    public sealed class GameplayAudioController : MonoBehaviour
    {
        [Header("Systems")]
        [SerializeField] private TypingController typing;
        [SerializeField] private ResonanceController resonance;
        [SerializeField] private WeatherController weather;
        [Header("Clips")]
        [SerializeField] private AudioClip calmAmbience;
        [SerializeField] private AudioClip heavyRain;
        [SerializeField] private AudioClip storm;
        [SerializeField] private AudioClip distantThunder;
        [SerializeField] private AudioClip fillJournal;
        [SerializeField] private AudioClip draggingStone;
        [SerializeField] private AudioClip[] pageFlips;
        [SerializeField] private AudioClip pagesFlyingAway;
        [Header("Mix")]
        [SerializeField, Range(0,1)] private float calmVolume = .25f;
        [SerializeField, Range(0,1)] private float rainVolume = .35f;
        [SerializeField, Range(0,1)] private float stormVolume = .45f;
        [SerializeField, Range(0,1)] private float writingVolume = .25f;
        [SerializeField, Range(0,1)] private float stoneVolume = .2f;
        [SerializeField, Range(0,1)] private float pageVolume = .45f;
        [SerializeField, Range(0,1)] private float loseVolume = .7f;
        [SerializeField, Range(0,1)] private float thunderVolume = .25f;
        [SerializeField, Min(.01f)] private float ambienceFadeSeconds = 1.5f;
        [SerializeField, Min(.01f)] private float activityFadeSeconds = .25f;
        [SerializeField, Min(0)] private float activityHoldSeconds = .45f;
        [SerializeField, Min(1)] private float minThunderInterval = 20f;
        [SerializeField, Min(1)] private float maxThunderInterval = 35f;
        private AudioSource calmSource, rainSource, stormSource, writingSource, stoneSource, pageSource, loseSource, thunderSource;
        private float writingUntil = -1, stoneUntil = -1, thunderTimer;
        private WeatherMode previousWeather;
        private bool Active => resonance != null && resonance.IsAscentActive;

        private void Awake()
        {
            calmSource = CreateSource("Calm", calmAmbience, true);
            rainSource = CreateSource("Rain", heavyRain, true);
            stormSource = CreateSource("Storm", storm, true);
            writingSource = CreateSource("Writing", fillJournal, true);
            stoneSource = CreateSource("Stone", draggingStone, true);
            pageSource = CreateSource("Page", null, false);
            loseSource = CreateSource("Lose", null, false);
            thunderSource = CreateSource("Thunder", distantThunder, false);
            if (typing == null || resonance == null) Debug.LogWarning("GameplayAudio: assign Typing and Resonance.", this);
        }
        private AudioSource CreateSource(string label, AudioClip clip, bool loop)
        {
            var child = new GameObject(label + " Audio");
            child.transform.SetParent(transform, false);
            var source = child.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0;
            source.loop = loop;
            source.clip = clip;
            source.volume = loop ? 0 : 1;
            return source;
        }
        private void OnEnable()
        {
            if (typing != null)
            {
                typing.CharacterTyped += OnTyped;
                typing.CharacterMistyped += OnMistyped;
                typing.SentenceCompleted += OnSentence;
            }
            if (resonance != null) resonance.Depleted += OnLose;
        }
        private void OnDisable()
        {
            if (typing != null)
            {
                typing.CharacterTyped -= OnTyped;
                typing.CharacterMistyped -= OnMistyped;
                typing.SentenceCompleted -= OnSentence;
            }
            if (resonance != null) resonance.Depleted -= OnLose;
            foreach (var source in new[]{calmSource,rainSource,stormSource,writingSource,stoneSource,pageSource,loseSource,thunderSource})
                if (source != null) { source.Stop(); if (source.loop) source.volume = 0; }
            writingUntil = stoneUntil = -1;
            previousWeather = WeatherMode.Dry;
        }
        private void OnTyped(char c)
        {
            if (!Active) return;
            writingUntil = stoneUntil = Time.time + activityHoldSeconds;
        }
        private void OnMistyped(char expected, char actual)
        {
            if (Active) writingUntil = Time.time + activityHoldSeconds;
        }
        private void OnSentence()
        {
            if (!Active || pageFlips == null || pageFlips.Length == 0) return;
            var clip = pageFlips[Random.Range(0,pageFlips.Length)];
            if (clip == null) return;
            pageSource.Stop();
            pageSource.clip = clip;
            pageSource.volume = pageVolume;
            pageSource.Play();
        }
        private void OnLose()
        {
            writingUntil = stoneUntil = -1;
            pageSource.Stop();
            thunderSource.Stop();
            if (pagesFlyingAway != null) loseSource.PlayOneShot(pagesFlyingAway, loseVolume);
        }
        private void Update()
        {
            var mode = Active && weather != null ? weather.EffectiveWeather : WeatherMode.Dry;
            float dt = Time.unscaledDeltaTime;
            Fade(calmSource, Active && mode == WeatherMode.Dry ? calmVolume : 0, ambienceFadeSeconds, dt);
            Fade(rainSource, mode == WeatherMode.HeavyRain ? rainVolume : 0, ambienceFadeSeconds, dt);
            Fade(stormSource, mode == WeatherMode.Storm ? stormVolume : 0, ambienceFadeSeconds, dt);
            bool activity = Active && Time.timeScale > 0;
            Fade(writingSource, activity && Time.time < writingUntil ? writingVolume : 0, activityFadeSeconds, dt);
            Fade(stoneSource, activity && Time.time < stoneUntil ? stoneVolume : 0, activityFadeSeconds, dt);
            if (!Active) writingUntil = stoneUntil = -1;
            if (mode != previousWeather)
            {
                thunderSource.Stop();
                thunderTimer = NextThunder();
                previousWeather = mode;
            }
            if (mode == WeatherMode.Storm && activity)
            {
                thunderTimer -= Time.deltaTime;
                if (thunderTimer <= 0 && !thunderSource.isPlaying)
                {
                    if (distantThunder != null) { thunderSource.volume = thunderVolume; thunderSource.Play(); }
                    thunderTimer = NextThunder();
                }
            }
        }
        private float NextThunder() => Random.Range(minThunderInterval, Mathf.Max(minThunderInterval,maxThunderInterval));
        private static void Fade(AudioSource source, float target, float seconds, float dt)
        {
            if (source.clip == null) return;
            if (target > 0 && !source.isPlaying) source.Play();
            // Exponential fade: time constant is approximately one third of requested duration.
            source.volume = Mathf.Lerp(source.volume, target, 1 - Mathf.Exp(-3 * dt / Mathf.Max(.01f,seconds)));
            if (target == 0 && source.volume < .002f) { source.volume = 0; if (source.isPlaying) source.Stop(); }
        }
    }
}
