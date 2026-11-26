using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class mainMenu : MonoBehaviour
{
    [Header("Buttons")]
    public Button startButton;
    public Button settingsButton;
    public Button quitButton;

    [Header("References")]
    public SettingsMenu settingsMenu;
    public AudioSource sfxSource;
    public AudioSource bgmSource;
    public Image fadeOverlay;

    [Header("Scene Settings")]
    public string gameSceneName = "gameplay";
    public float transitionDelay = 1.0f;

    private bool isTransitioning = false;

    private void Start()
    {
        if (fadeOverlay != null)
        {
            Color c = fadeOverlay.color;
            c.a = 0f;
            fadeOverlay.color = c;
            fadeOverlay.raycastTarget = false;
        }

        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(OnStartClicked);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.AddListener(OnSettingsClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(OnQuitClicked);
        }
    }

    private void PlayClickSound()
    {
        if (sfxSource != null)
        {
            sfxSource.Play();
        }
    }

    public void OnStartClicked()
    {
        if (!isTransitioning)
        {
            StartCoroutine(PlayAndLoadScene());
        }
    }

    public void OnSettingsClicked()
    {
        if (!isTransitioning && settingsMenu != null)
        {
            settingsMenu.OpenSettings();
        }
    }

    public void OnQuitClicked()
    {
        if (!isTransitioning)
        {
            StartCoroutine(PlayAndQuit());
        }
    }

    private IEnumerator PlayAndLoadScene()
    {
        isTransitioning = true;
        PlayClickSound();

        if (fadeOverlay != null)
        {
            fadeOverlay.raycastTarget = true; 
        }

        float startVolume = bgmSource != null ? bgmSource.volume : 0f;
        float elapsed = 0f;

        while (elapsed < transitionDelay)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / transitionDelay);

            if (bgmSource != null)
            {
                bgmSource.volume = Mathf.Lerp(startVolume, 0f, progress);
            }

            if (fadeOverlay != null)
            {
                Color c = fadeOverlay.color;
                c.a = progress;
                fadeOverlay.color = c;
            }

            yield return null;
        }

        if (bgmSource != null) bgmSource.volume = 0f;
        if (fadeOverlay != null)
        {
            Color c = fadeOverlay.color;
            c.a = 1f;
            fadeOverlay.color = c;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    private IEnumerator PlayAndQuit()
    {
        isTransitioning = true;
        PlayClickSound();
        yield return new WaitForSecondsRealtime(transitionDelay);
        Application.Quit();
        Debug.Log("Game Quit!");
    }
}