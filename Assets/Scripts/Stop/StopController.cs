using System.Collections;
using Encumbrance.Journal;
using Encumbrance.Hazard;
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
        [Header("Elang Movement (optional; replaces legacy movement)")]
        [SerializeField] private Terrain_Moving terrainMovement;
        [SerializeField] private Mountain_Moving mountainMovement;
        [SerializeField] private WeatherController weather;

        [Header("Chapter progression (optional)")]
        [SerializeField] private ChapterAtmosphereController atmosphere;
        [SerializeField] private JournalWetness journalWetness;
        [Header("Stop UI")]
        [SerializeField] private GameObject stopPanel;
        [SerializeField] private Button continueButton;
        [SerializeField] private TMP_Text resonanceReadout;

        [Header("Narrative cutscene (optional; replaces Stop UI)")]
        [SerializeField] private CanvasGroup cutsceneGroup;
        [SerializeField] private TMP_Text cutsceneText;
        [SerializeField, TextArea(2, 5)] private string narrative = "For a moment, I allowed myself to rest.";
        [SerializeField, Min(0f)] private float screenFadeSeconds = 1.5f;
        [SerializeField, Min(0f)] private float textFadeSeconds = 1f;
        private bool waitingForContinue;
        private bool continueRequested;
        private bool HasCutscene => cutsceneGroup != null && cutsceneText != null;

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
            if (terrainMovement == null && worldScroller == null) Debug.LogError("StopController: assign terrain movement or WorldScroller.", this);
            if (continueButton == null || (!HasCutscene && stopPanel == null))
                Debug.LogError("StopController: assign Continue Button and either cutscene group/text or legacy StopPanel.", this);
            if (HasCutscene && continueButton != null && !continueButton.transform.IsChildOf(cutsceneGroup.transform))
                Debug.LogError("StopController: Continue Button must be a child of the cutscene group.", this);
            if (cutsceneGroup != null)
            {
                cutsceneGroup.alpha = 0f;
                cutsceneGroup.blocksRaycasts = false;
                cutsceneGroup.gameObject.SetActive(false);
            }

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
            if (stopTriggered || typing == null) return;
            completedSentences++;
            if (completedSentences >= typing.TotalSentenceCount) return;
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
            SetMovement(false);
            resonance.EndAscent();

            if (terrainMovement == null && terrainRecycler != null)
            {
                previousTerrainRecyclerEnabled = terrainRecycler.enabled;
                terrainRecycler.enabled = false;
            }

            UpdateResonanceReadout();
            if (transitionRoutine != null) StopCoroutine(transitionRoutine);
            transitionRoutine = HasCutscene
                ? StartCoroutine(PlayStopCutscene())
                : StartCoroutine(TransitionTerrain(flat: true, showPanelWhenDone: true));
        }

        public void ContinueAscent()
        {
            if (!isAtStop) return;
            if (HasCutscene)
            {
                if (!waitingForContinue) return;
                waitingForContinue = false;
                continueRequested = true;
                cutsceneGroup.interactable = false;
                if (continueButton != null) continueButton.interactable = false;
                return;
            }
            if (continueButton != null) continueButton.interactable = false;
            if (stopPanel != null) stopPanel.SetActive(false);
            if (transitionRoutine != null) StopCoroutine(transitionRoutine);
            transitionRoutine = StartCoroutine(ResumeAfterTerrainTransition());
        }

        private IEnumerator ResumeAfterTerrainTransition()
        {
            yield return TransitionTerrain(flat: false, showPanelWhenDone: false);

            RestoreAndResume();
        }

        private IEnumerator PlayStopCutscene()
        {
            waitingForContinue = false;
            continueRequested = false;
            if (continueButton != null)
            {
                continueButton.interactable = false;
                continueButton.gameObject.SetActive(false);
            }
            cutsceneText.text = narrative;
            cutsceneText.alpha = 0f;
            cutsceneGroup.alpha = 0f;
            cutsceneGroup.blocksRaycasts = true;
            cutsceneGroup.interactable = false;
            cutsceneGroup.gameObject.SetActive(true);
            yield return Fade(0f, 1f, screenFadeSeconds, v => cutsceneGroup.alpha = v);
            yield return TransitionTerrain(flat: true, showPanelWhenDone: false);
            yield return Fade(0f, 1f, textFadeSeconds, v => cutsceneText.alpha = v);
            // Show Continue only after the narrative has faded in.
            waitingForContinue = true;
            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(true);
                continueButton.interactable = true;
            }
            cutsceneGroup.interactable = true;
            yield return new WaitUntil(() => continueRequested);
            waitingForContinue = false;
            cutsceneGroup.interactable = false;
            if (continueButton != null) continueButton.gameObject.SetActive(false);
            yield return Fade(1f, 0f, textFadeSeconds, v => cutsceneText.alpha = v);
            yield return TransitionTerrain(flat: false, showPanelWhenDone: false);
            // Recover behind the black screen, but keep ascent and input paused.
            resonance.RestoreAtStop();
            if (journalWetness != null) journalWetness.ResetWetness();
            if (atmosphere != null && atmosphere.isActiveAndEnabled) atmosphere.ApplyProgressWeather();
            else if (weather != null) weather.SetWeather(previousWeather);
            yield return Fade(1f, 0f, screenFadeSeconds, v => cutsceneGroup.alpha = v);
            cutsceneGroup.blocksRaycasts = false;
            cutsceneGroup.gameObject.SetActive(false);
            ResumeMovementAndInput();
        }

        private IEnumerator Fade(float from, float to, float seconds, System.Action<float> apply)
        {
            apply(from);
            float elapsed = 0f;
            while (elapsed < seconds)
            {
                elapsed += Time.unscaledDeltaTime;
                apply(Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / seconds)));
                yield return null;
            }
            apply(to);
        }

        private void RestoreAndResume()
        {
            // Restore in this order: resource, weather, movement, then keyboard input.
            resonance.RestoreAtStop();
            if (journalWetness != null) journalWetness.ResetWetness();
            if (atmosphere != null && atmosphere.isActiveAndEnabled) atmosphere.ApplyProgressWeather();
            else if (weather != null) weather.SetWeather(previousWeather);
            ResumeMovementAndInput();
        }

        private void ResumeMovementAndInput()
        {
            resonance.StartAscent();
            if (terrainMovement == null && terrainRecycler != null) terrainRecycler.enabled = previousTerrainRecyclerEnabled;
            SetMovement(true);
            if (typing != null) typing.SetInputEnabled(true);

            isAtStop = false;
            if (continueButton != null) continueButton.interactable = true;
            transitionRoutine = null;
        }

        private void SetMovement(bool value)
        {
            if (terrainMovement != null) terrainMovement.SetMovementEnabled(value);
            else if (worldScroller != null) worldScroller.SetScrollEnabled(value);
            if (mountainMovement != null) mountainMovement.SetMovementEnabled(value);
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
