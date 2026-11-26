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

    [Header("Scene Settings")]
    public string gameSceneName = "mainScene";
    public float transitionDelay = 0.5f;

    private void Start()
    {
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
        StartCoroutine(PlayAndLoadScene());
    }

    public void OnSettingsClicked()
    {
        if (settingsMenu != null)
        {
            settingsMenu.OpenSettings();
        }
    }

    public void OnQuitClicked()
    {
        StartCoroutine(PlayAndQuit());
    }

    private IEnumerator PlayAndLoadScene()
    {
        PlayClickSound();
        yield return new WaitForSeconds(transitionDelay);
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    private IEnumerator PlayAndQuit()
    {
        PlayClickSound();
        yield return new WaitForSeconds(transitionDelay);
        Application.Quit();
        Debug.Log("Game Quit!");
    }
}