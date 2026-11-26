using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class SettingsMenu : MonoBehaviour
{
    [Header("Audio Mixer")]
    public AudioMixer mainMixer;

    [Header("Panel & Buttons")]
    public GameObject settingsPanel;
    public Button closeButton;

    [Header("Sliders")]
    public Slider bgmSlider;
    public Slider sfxSlider;

    [Header("UI Click Sound")]
    public AudioSource uiClickSource;

    [Header("Gameplay Options")]
    public bool allowEscapeToggle = false;
    public bool pauseGameWhenOpen = false;

    private void Start()
    {
        float savedBGM = PlayerPrefs.GetFloat("BGMVolume", 1f);
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 1f);

        if (bgmSlider != null)
        {
            bgmSlider.minValue = 0.0001f;
            bgmSlider.maxValue = 1f;
            bgmSlider.value = savedBGM;
            bgmSlider.onValueChanged.RemoveAllListeners();
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0.0001f;
            sfxSlider.maxValue = 1f;
            sfxSlider.value = savedSFX;
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        SetBGMVolume(savedBGM);
        SetSFXVolume(savedSFX);

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseSettings);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (allowEscapeToggle && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (settingsPanel != null && settingsPanel.activeSelf)
                CloseSettings();
            else
                OpenSettings();
        }
    }

    public void OpenSettings()
    {
        PlayClickSound();
        if (settingsPanel != null) settingsPanel.SetActive(true);
        if (pauseGameWhenOpen) Time.timeScale = 0f;
    }

    public void CloseSettings()
    {
        PlayClickSound();
        PlayerPrefs.Save();
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (pauseGameWhenOpen) Time.timeScale = 1f;
    }

    public void SetBGMVolume(float sliderValue)
    {
        float dB = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20f;
        if (mainMixer != null)
        {
            mainMixer.SetFloat("BGMVolume", dB);
        }
        PlayerPrefs.SetFloat("BGMVolume", sliderValue);
    }

    public void SetSFXVolume(float sliderValue)
    {
        float dB = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20f;
        if (mainMixer != null)
        {
            mainMixer.SetFloat("SFXVolume", dB);
        }
        PlayerPrefs.SetFloat("SFXVolume", sliderValue);
    }

    private void PlayClickSound()
    {
        if (uiClickSource != null)
        {
            uiClickSource.Play();
        }
    }
}