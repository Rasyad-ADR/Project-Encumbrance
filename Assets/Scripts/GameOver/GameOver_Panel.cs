using System.Collections;
using Encumbrance.Journal;
using Encumbrance.Resonance;
using Encumbrance.Weather;
using Encumbrance.World;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Encumbrance.GameOver
{
    /// <summary>
    /// Shows a game over screen when Resonance reaches 0, then restarts the scene.
    /// </summary>
    public sealed class GameOverController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ResonanceController resonance;
        [SerializeField] private TypingController typing;
        [SerializeField] private WeatherController weather;

        [Header("Movement (same as StopController)")]
        [SerializeField] private Terrain_Moving terrainMovement;
        [SerializeField] private Mountain_Moving mountainMovement;
        [SerializeField] private WorldScroller worldScroller;

        [Header("Game Over UI")]
        [SerializeField] private CanvasGroup gameOverGroup;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button restartButton;
        [SerializeField, TextArea(2, 5)] private string message = "My voice faded, and the climb ended here.";
        [SerializeField, Min(0f)] private float fadeSeconds = 1.5f;
        [Header("Restart Fade")]
        [SerializeField] private CanvasGroup fadeOverlay;
        [SerializeField, Min(0f)] private float restartFadeSeconds = 1f;

private bool restarting;

        public bool IsGameOver { get; private set; }

        private void Awake()
        {
            if (resonance == null) Debug.LogError("GameOverController: assign ResonanceController.", this);
            if (gameOverGroup == null || restartButton == null)
                Debug.LogError("GameOverController: assign Game Over Group and Restart Button.", this);

            if (gameOverGroup != null)
            {
                gameOverGroup.alpha = 0f;
                gameOverGroup.blocksRaycasts = false;
                gameOverGroup.interactable = false;
                gameOverGroup.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (resonance != null) resonance.Depleted += OnDepleted;
            if (restartButton != null) restartButton.onClick.AddListener(Restart);
        }

        private void OnDisable()
        {
            if (resonance != null) resonance.Depleted -= OnDepleted;
            if (restartButton != null) restartButton.onClick.RemoveListener(Restart);
        }

        private void OnDepleted()
        {
            if (IsGameOver) return;
            IsGameOver = true;

            if (typing != null) typing.SetInputEnabled(false);
            if (weather != null) weather.SetDry();
            SetMovement(false);

            StartCoroutine(ShowGameOver());
        }

        private IEnumerator ShowGameOver()
        {
            if (messageText != null) messageText.text = message;

            gameOverGroup.alpha = 0f;
            gameOverGroup.blocksRaycasts = true;
            gameOverGroup.interactable = false;
            gameOverGroup.gameObject.SetActive(true);

            float elapsed = 0f;
            while (elapsed < fadeSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                gameOverGroup.alpha = Mathf.Clamp01(elapsed / fadeSeconds);
                yield return null;
            }
            gameOverGroup.alpha = 1f;
            gameOverGroup.interactable = true;
        }

        public void Restart()
        {
            if (!IsGameOver || restarting) return;
            restarting = true;
            restartButton.interactable = false;
            StartCoroutine(RestartRoutine());
        }

        private IEnumerator RestartRoutine()
        {
        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);
            fadeOverlay.blocksRaycasts = true;

            float start = fadeOverlay.alpha;
            float elapsed = 0f;
            while (elapsed < restartFadeSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                fadeOverlay.alpha = Mathf.Lerp(start, 1f, Mathf.Clamp01(elapsed / restartFadeSeconds));
                yield return null;
            }
            fadeOverlay.alpha = 1f;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

        private void SetMovement(bool value)
        {
            if (terrainMovement != null) terrainMovement.SetMovementEnabled(value);
            else if (worldScroller != null) worldScroller.SetScrollEnabled(value);
            if (mountainMovement != null) mountainMovement.SetMovementEnabled(value);
        }
    }
}