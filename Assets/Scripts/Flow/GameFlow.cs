using System.Collections;
using Encumbrance.Journal;
using Encumbrance.Resonance;
using Encumbrance.World;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        [Header("UI")]
        [SerializeField] private CanvasGroup fadeOverlay;
        [SerializeField] private TMP_Text messageText;

        [Header("Lose")]
        [SerializeField] private string loseMessage = "The pages slipped away...";
        [SerializeField, Min(0f)] private float loseMessageDuration = 1.25f;
        [SerializeField, Min(0f)] private float fadeDuration = 0.75f;
        [SerializeField, Min(0f)] private float blackScreenDuration = 0.5f;

        [Header("Win")]
        [SerializeField] private string winMessage = "Stop reached.";
        [SerializeField] private bool fadeOnWin = false;
        [SerializeField, Min(0f)] private float winMessageDuration = 1.5f;
        [SerializeField] private string nextSceneName = "";

        private bool finished;
        private Coroutine sequence;

        private void Awake()
        {
            if (typingController == null) Debug.LogError("GameFlow: assign TypingController.", this);
            if (resonanceController == null) Debug.LogError("GameFlow: assign ResonanceController.", this);
            if (terrainMovement == null && worldScroller == null) Debug.LogError("GameFlow: assign terrain movement or WorldScroller.", this);
            if (fadeOverlay == null) Debug.LogError("GameFlow: assign FadeOverlay CanvasGroup.", this);
            if (messageText == null) Debug.LogError("GameFlow: assign MessageText TMP_Text.", this);
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
            if (resonanceController != null) resonanceController.Depleted += OnDepleted;
            if (typingController != null) typingController.ChapterCompleted += OnChapterCompleted;
        }

        private void OnDisable()
        {
            if (resonanceController != null) resonanceController.Depleted -= OnDepleted;
            if (typingController != null) typingController.ChapterCompleted -= OnChapterCompleted;
            if (sequence != null)
            {
                StopCoroutine(sequence);
                sequence = null;
            }
        }

        private void OnDepleted()
        {
            if (finished) return;
            finished = true;
            StopGameplay();
            ShowMessage(loseMessage);
            sequence = StartCoroutine(LoseSequence());
        }

        private void OnChapterCompleted()
        {
            if (finished) return;
            finished = true;
            StopGameplay();
            ShowMessage(winMessage);
            if (fadeOnWin || !string.IsNullOrWhiteSpace(nextSceneName))
                sequence = StartCoroutine(WinSequence());
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

        private IEnumerator LoseSequence()
        {
            yield return new WaitForSecondsRealtime(loseMessageDuration);
            yield return FadeToBlack();
            yield return new WaitForSecondsRealtime(blackScreenDuration);
            Scene scene = SceneManager.GetActiveScene();
            if (scene.buildIndex < 0)
            {
                Debug.LogError("GameFlow: active scene is not in Build Profiles. Add it before testing restart.", this);
                yield break;
            }
            SceneManager.LoadScene(scene.buildIndex);
        }

        private IEnumerator WinSequence()
        {
            yield return new WaitForSecondsRealtime(winMessageDuration);
            if (fadeOnWin) yield return FadeToBlack();
            if (!string.IsNullOrWhiteSpace(nextSceneName))
                SceneManager.LoadScene(nextSceneName);
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
