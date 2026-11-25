using System.Collections;
using Encumbrance.Journal;
using Encumbrance.Resonance;
using Encumbrance.Weather;
using Encumbrance.World;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Encumbrance.Stop
{
    /// <summary>
    /// Pauses the ascent after a configured number of completed journal sentences.
    /// It preserves TypingController's current sentence/index; it never calls BeginChapter.
    /// </summary>
    public sealed class StopController : MonoBehaviour
    {
        [Header("Checkpoint")]
        [SerializeField, Min(1)] private int stopAfterCompletedSentences = 9;
        [SerializeField] private TypingController typing;
        [SerializeField] private ResonanceController resonance;
        [SerializeField] private WorldScroller worldScroller;
        [SerializeField] private TerrainRecycler terrainRecycler;
        [SerializeField] private WeatherController weather;

        [Header("Stop UI")]
        [SerializeField] private GameObject stopPanel;
        [SerializeField] private Button continueButton;
        [SerializeField] private TMP_Text resonanceReadout;

        [Header("Optional terrain transition")]
        [SerializeField] private Transform worldRoot;
        [SerializeField] private bool animateTerrainToFlat = true;
        [SerializeField] private float flatLocalZAngle = 0f;
        [SerializeField, Min(0f)] private float terrainTransitionSeconds = 0.8f;

        private int completedSentences;
        private bool stopTriggered;
        private bool isAtStop;
        private bool previousTerrainRecyclerEnabled;
        private WeatherMode previousWeather = WeatherMode.Dry;
        private Quaternion inclineLocalRotation;
        private Coroutine transitionRoutine;

        public bool IsAtStop => isAtStop;

        private void Awake()
        {
            if (typing == null) Debug.LogError("StopController: assign TypingController.", this);
            if (resonance == null) Debug.LogError("StopController: assign ResonanceController.", this);
            if (worldScroller == null) Debug.LogError("StopController: assign WorldScroller.", this);
            if (stopPanel == null) Debug.LogError("StopController: assign StopPanel.", this);
            if (continueButton == null) Debug.LogError("StopController: assign Continue Button.", this);

            if (worldRoot != null) inclineLocalRotation = worldRoot.localRotation;
            if (terrainRecycler != null) previousTerrainRecyclerEnabled = terrainRecycler.enabled;
            if (stopPanel != null) stopPanel.SetActive(false);
        }

        private void OnEnable()
        {
            if (typing != null) typing.SentenceCompleted += OnSentenceCompleted;
            if (continueButton != null) continueButton.onClick.AddListener(ContinueAscent);
        }

        private void OnDisable()
        {
            if (typing != null) typing.SentenceCompleted -= OnSentenceCompleted;
            if (continueButton != null) continueButton.onClick.RemoveListener(ContinueAscent);
            if (transitionRoutine != null) StopCoroutine(transitionRoutine);
        }

        private void OnSentenceCompleted()
        {
            if (stopTriggered) return;
            completedSentences++;
            if (completedSentences >= stopAfterCompletedSentences)
                ReachStop();
        }

        private void ReachStop()
        {
            if (stopTriggered || typing == null || resonance == null) return;
            stopTriggered = true;
            isAtStop = true;

            // Capture the selected weather before pausing the ascent.
            if (weather != null)
            {
                previousWeather = weather.Current;
                weather.SetDry();
            }

            typing.SetInputEnabled(false);
            if (worldScroller != null) worldScroller.SetScrollEnabled(false);
            resonance.EndAscent();

            if (terrainRecycler != null)
            {
                previousTerrainRecyclerEnabled = terrainRecycler.enabled;
                terrainRecycler.enabled = false;
            }

            UpdateResonanceReadout();
            if (transitionRoutine != null) StopCoroutine(transitionRoutine);
            transitionRoutine = StartCoroutine(TransitionTerrain(flat: true, showPanelWhenDone: true));
        }

        public void ContinueAscent()
        {
            if (!isAtStop) return;
            if (continueButton != null) continueButton.interactable = false;
            if (stopPanel != null) stopPanel.SetActive(false);
            if (transitionRoutine != null) StopCoroutine(transitionRoutine);
            transitionRoutine = StartCoroutine(ResumeAfterTerrainTransition());
        }

        private IEnumerator ResumeAfterTerrainTransition()
        {
            yield return TransitionTerrain(flat: false, showPanelWhenDone: false);

            // Restore in this order: resource, weather, movement, then keyboard input.
            resonance.RestoreAtStop();
            if (weather != null) weather.SetWeather(previousWeather);
            resonance.StartAscent();
            if (terrainRecycler != null) terrainRecycler.enabled = previousTerrainRecyclerEnabled;
            if (worldScroller != null) worldScroller.SetScrollEnabled(true);
            if (typing != null) typing.SetInputEnabled(true);

            isAtStop = false;
            if (continueButton != null) continueButton.interactable = true;
            transitionRoutine = null;
        }

        private IEnumerator TransitionTerrain(bool flat, bool showPanelWhenDone)
        {
            if (animateTerrainToFlat && worldRoot != null && terrainTransitionSeconds > 0f)
            {
                Quaternion from = worldRoot.localRotation;
                Quaternion to = inclineLocalRotation;
                if (flat)
                {
                    Vector3 euler = inclineLocalRotation.eulerAngles;
                    to = Quaternion.Euler(euler.x, euler.y, flatLocalZAngle);
                }

                float elapsed = 0f;
                while (elapsed < terrainTransitionSeconds)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = Mathf.Clamp01(elapsed / terrainTransitionSeconds);
                    // SmoothStep avoids a sudden start/stop in the terrain tilt.
                    t = t * t * (3f - 2f * t);
                    worldRoot.localRotation = Quaternion.Slerp(from, to, t);
                    yield return null;
                }
                worldRoot.localRotation = to;
            }

            if (showPanelWhenDone && stopPanel != null)
                stopPanel.SetActive(true);
        }

        private void UpdateResonanceReadout()
        {
            if (resonanceReadout == null || resonance == null) return;
            resonanceReadout.text = $"Resonance  {Mathf.CeilToInt(resonance.Current)} / {Mathf.CeilToInt(resonance.Maximum)}";
        }
    }
}
