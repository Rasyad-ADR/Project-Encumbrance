using System.Collections;
using Encumbrance.Journal;
using Encumbrance.Resonance;
using Encumbrance.World;
using Encumbrance.Weather;
using TMPro;
using UnityEngine;

namespace Encumbrance.Flow
{
    public sealed class GameFlow : MonoBehaviour
    {
        [Header("Systems")]
        [SerializeField] private TypingController typingController;
        [SerializeField] private ResonanceController resonanceController;
        [SerializeField] private WorldScroller worldScroller;
        [SerializeField] private Terrain_Moving terrainMovement;
        [SerializeField] private Mountain_Moving mountainMovement;

        [Header("Ending")]
        [SerializeField] private EndingSequenceController endingSequence;
        [SerializeField] private WeatherController weather;
        private bool endingStarted;

        [Header("UI")]
        [SerializeField] private CanvasGroup fadeOverlay;
        [SerializeField] private TMP_Text messageText;

        [SerializeField, Min(0f)] private float fadeDuration = 0.75f;

        private void Awake()
        {
            if (fadeOverlay != null)
            {
                fadeOverlay.alpha = 0f;
                fadeOverlay.interactable = false;
                fadeOverlay.blocksRaycasts = false;
            }
            if (messageText != null) messageText.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (typingController != null) typingController.ChapterCompleted += OnChapterCompleted;
        }

        private void OnDisable()
        {
            if (typingController != null) typingController.ChapterCompleted -= OnChapterCompleted;
        }

        private void OnChapterCompleted()
        {
            if (endingStarted || (resonanceController != null && resonanceController.Current <= 0f)) return;
            endingStarted = true;
            StopGameplay();
            if (weather != null) weather.SetDry();
            if (endingSequence != null && endingSequence.TryPlay()) return;
            Debug.LogError("GameFlow: ending controller missing or invalid. Check Inspector.", this);
            ShowMessage("ENCUMBRANCE");
        }

        private void StopGameplay()
        {
            if (typingController != null) typingController.SetInputEnabled(false);
            if (terrainMovement != null) terrainMovement.SetMovementEnabled(false);
            else if (worldScroller != null) worldScroller.SetScrollEnabled(false);
            if (mountainMovement != null) mountainMovement.SetMovementEnabled(false);
            if (resonanceController != null) resonanceController.EndAscent();
        }

        private void ShowMessage(string message)
        {
            if (messageText == null) return;
            messageText.text = message;
            messageText.gameObject.SetActive(true);
        }

        private IEnumerator FadeToBlack()
        {
            if (fadeOverlay == null) yield break;
            float elapsed = 0f;
            float startingAlpha = fadeOverlay.alpha;
            if (fadeDuration <= 0f)
            {
                fadeOverlay.alpha = 1f;
                yield break;
            }
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                fadeOverlay.alpha = Mathf.Lerp(startingAlpha, 1f, Mathf.Clamp01(elapsed / fadeDuration));
                yield return null;
            }
            fadeOverlay.alpha = 1f;
        }
    }
}