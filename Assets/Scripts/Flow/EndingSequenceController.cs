using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Encumbrance.Flow
{
    public sealed class EndingSequenceController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup endingGroup;
        [SerializeField] private TMP_Text endingText;
        [SerializeField] private Button menuButton;
        [SerializeField, TextArea(2, 5)] private string[] endingLines = {
            "The weight was still there.",
            "Yet the journey no longer felt the same.",
            "Perhaps I had learned how to carry it."
        };
        [SerializeField] private string finalTitle = "ENCUMBRANCE";
        [SerializeField, Min(0f)] private float screenFadeSeconds = 2f;
        [SerializeField, Min(0f)] private float textFadeSeconds = .8f;
        [SerializeField, Min(0f)] private float readingSeconds = 4f;
        [SerializeField] private string mainMenuSceneName = "mainMenu";
        [Header("Ending Slideshow")]
        [SerializeField] private Image slideshowImage;
        [SerializeField] private Sprite[] slideshowFrames;
        [SerializeField, Min(0f)] private float secondsPerSlide = 1.5f;
        [SerializeField, Min(0f)] private float slideshowFadeSeconds = 0.4f;
        private bool started, menuReady;

        private void Awake()
        {
            if (endingGroup != null)
            {
                endingGroup.alpha = 0f;
                endingGroup.interactable = false;
                endingGroup.blocksRaycasts = false;
                endingGroup.gameObject.SetActive(false);
            }
            if (slideshowImage != null) slideshowImage.gameObject.SetActive(false);
            if (menuButton != null) menuButton.gameObject.SetActive(false);
        }
        private void OnEnable() { if (menuButton != null) menuButton.onClick.AddListener(ReturnToMenu); }
        private void OnDisable() { if (menuButton != null) menuButton.onClick.RemoveListener(ReturnToMenu); }

        public bool TryPlay()
        {
            if (started) return true;
            if (!isActiveAndEnabled || endingGroup == null || endingText == null || menuButton == null)
            {
                Debug.LogError("EndingSequence: assign active controller, group, text and menu button.", this);
                return false;
            }
            started = true;
            StartCoroutine(Play());
            return true;
        }
        private IEnumerator Play()
        {
            endingText.alpha = 0f;
            menuButton.gameObject.SetActive(false);
            endingGroup.blocksRaycasts = true;
            endingGroup.interactable = false;
            endingGroup.alpha = 0f;
            endingGroup.gameObject.SetActive(true);
            yield return Fade(0f, 1f, screenFadeSeconds, v => endingGroup.alpha = v);
            yield return PlaySlideshow();
            if (endingLines != null)
            {
                foreach (string line in endingLines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    endingText.text = line;
                    yield return Fade(0f, 1f, textFadeSeconds, v => endingText.alpha = v);
                    yield return new WaitForSecondsRealtime(readingSeconds);
                    yield return Fade(1f, 0f, textFadeSeconds, v => endingText.alpha = v);
                }
            }
            endingText.text = finalTitle;
            yield return Fade(0f, 1f, textFadeSeconds, v => endingText.alpha = v);
            menuButton.gameObject.SetActive(true);
            menuButton.interactable = true;
            endingGroup.interactable = true;
            menuReady = true;
        }
        private IEnumerator PlaySlideshow()
        {
            if (slideshowImage == null || slideshowFrames == null || slideshowFrames.Length == 0)
                yield break;

            bool first = true;
            slideshowImage.raycastTarget = false;
            foreach (Sprite frame in slideshowFrames)
            {
                if (frame == null) continue;
                slideshowImage.sprite = frame;
                if (first)
                {
                    slideshowImage.color = new Color(1f, 1f, 1f, 0f);
                    slideshowImage.gameObject.SetActive(true);
                    yield return Fade(0f, 1f, slideshowFadeSeconds,
                        v => slideshowImage.color = new Color(1f, 1f, 1f, v));
                    first = false;
                }
                yield return new WaitForSecondsRealtime(secondsPerSlide);
            }
            if (!first)
            {
                yield return Fade(1f, 0f, slideshowFadeSeconds,
                    v => slideshowImage.color = new Color(1f, 1f, 1f, v));
                slideshowImage.gameObject.SetActive(false);
            }
        }
        private void ReturnToMenu()
        {
            if (!menuReady) return;
            if (string.IsNullOrWhiteSpace(mainMenuSceneName) || !Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
            {
                Debug.LogError("EndingSequence: add the main menu scene to Build Profiles and check its name.", this);
                return;
            }
            menuReady = false;
            menuButton.interactable = false;
            SceneManager.LoadScene(mainMenuSceneName);
        }
        private static IEnumerator Fade(float from, float to, float seconds, System.Action<float> apply)
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
    }
}
