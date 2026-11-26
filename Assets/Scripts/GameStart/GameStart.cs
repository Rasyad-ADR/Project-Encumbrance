using System.Collections;
using Encumbrance.Journal;
using Encumbrance.Resonance;
using Encumbrance.World;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Encumbrance.GameStart
{
    /// <summary>
    /// Holds the game paused behind a black screen with a narrative and a Start button.
    /// Runs on every scene load, so it also appears after a restart.
    /// </summary>
    public sealed class GameStartController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ResonanceController resonance;
        [SerializeField] private TypingController typing;

        [Header("Movement (same as StopController)")]
        [SerializeField] private Terrain_Moving terrainMovement;
        [SerializeField] private Mountain_Moving mountainMovement;
        [SerializeField] private WorldScroller worldScroller;

        [Header("Start UI")]
        [SerializeField] private CanvasGroup startGroup;   // black screen, parent of text + button
        [SerializeField] private TMP_Text startText;
        [SerializeField] private Button startButton;
        [SerializeField, TextArea(2, 5)] private string firstNarrative = "I began to climb, one word at a time.";
        [SerializeField, TextArea(2, 5)] private string restartNarrative = "I tried again.";
        [SerializeField, Min(0f)] private float textFadeSeconds = 1f;
        [SerializeField, Min(0f)] private float screenFadeSeconds = 1.5f;

        // Survives scene reloads, so we can tell first start from restart.
        private static bool hasStartedBefore;

        private bool startRequested;

        public bool IsWaitingToStart { get; private set; }

        private void Awake()
        {
            if (startGroup == null || startText == null || startButton == null)
                Debug.LogError("GameStartController: assign Start Group, Start Text and Start Button.", this);

            // Cover the screen immediately so gameplay never flashes before Start().
            if (startGroup != null)
            {
                startGroup.alpha = 1f;
                startGroup.blocksRaycasts = true;
                startGroup.interactable = false;
                startGroup.gameObject.SetActive(true);
            }
            if (startText != null) startText.alpha = 0f;
            if (startButton != null) startButton.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (startButton != null) startButton.onClick.AddListener(OnStartClicked);
        }

        private void OnDisable()
        {
            if (startButton != null) startButton.onClick.RemoveListener(OnStartClicked);
        }

        private IEnumerator Start()
        {
            // Wait one frame so other scripts finish their own Start() first.
            yield return null;

            IsWaitingToStart = true;
            if (typing != null) typing.SetInputEnabled(false);
            if (resonance != null) resonance.EndAscent();
            SetMovement(false);

            startText.text = hasStartedBefore ? restartNarrative : firstNarrative;
            hasStartedBefore = true;

            yield return Fade(0f, 1f, textFadeSeconds, v => startText.alpha = v);

            startButton.gameObject.SetActive(true);
            startButton.interactable = true;
            startGroup.interactable = true;
            yield return new WaitUntil(() => startRequested);

            startGroup.interactable = false;
            startButton.gameObject.SetActive(false);
            yield return Fade(1f, 0f, textFadeSeconds, v => startText.alpha = v);
            yield return Fade(1f, 0f, screenFadeSeconds, v => startGroup.alpha = v);

            startGroup.blocksRaycasts = false;
            startGroup.gameObject.SetActive(false);

            if (resonance != null) resonance.StartAscent();
            SetMovement(true);
            if (typing != null) typing.SetInputEnabled(true);
            IsWaitingToStart = false;
        }

        private void OnStartClicked()
        {
            if (!IsWaitingToStart) return;
            startRequested = true;
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

        private void SetMovement(bool value)
        {
            if (terrainMovement != null) terrainMovement.SetMovementEnabled(value);
            else if (worldScroller != null) worldScroller.SetScrollEnabled(value);
            if (mountainMovement != null) mountainMovement.SetMovementEnabled(value);
        }
    }
}