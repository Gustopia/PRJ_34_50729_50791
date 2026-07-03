using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static bool IsPaused = false;

    [Header("Input")]
    public KeyCode pauseKey = KeyCode.Escape;

    [Header("UI Panels")]
    public GameObject pauseMenuUI;
    public GameObject optionsMenuUI;

    [Header("Audio")]
    public AudioMixer audioMixer;
    public Slider musicSlider;
    public Slider sfxSlider;
    public Slider voiceSlider;

    [Header("UI Toggle")]
    public Toggle uiToggle;
    public GameObject[] uiElements;

    [Header("Player Behaviour")]
    public Toggle flyingToggle;
    public PlayerMovement playerMovement;

    [Header("Cursor")]
    public bool lockCursorWhenPlaying = true;

    [Header("Player UI")]
    public GameObject playerUICanvas;

    private void Start()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (optionsMenuUI != null) optionsMenuUI.SetActive(false);

        LoadSettings();

        if (lockCursorWhenPlaying)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (IsPaused && optionsMenuUI != null && optionsMenuUI.activeSelf)
                CloseOptions();
            else if (IsPaused)
                Resume();
            else
                Pause();
        }
    }

    // Pausa
    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        if (optionsMenuUI != null) optionsMenuUI.SetActive(false);

        Time.timeScale = 0f;
        IsPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerUICanvas != null) playerUICanvas.SetActive(false); 
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        if (optionsMenuUI != null) optionsMenuUI.SetActive(false);

        Time.timeScale = 1f;
        IsPaused = false;

        if (lockCursorWhenPlaying)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (playerUICanvas != null) playerUICanvas.SetActive(true);

        // Reaplica o estado guardado do toggle da UI do jogador
        if (uiToggle != null)
            ToggleUI(uiToggle.isOn);
        else
            ToggleUI(PlayerPrefs.GetInt("UIEnabled", 1) == 1);
    }

    // Opcções
    public void OpenOptions()
    {
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(true);
    }

    public void CloseOptions()
    {
        SaveSettings();
        optionsMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true);
    }

    // Menu Principal
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        IsPaused = false;

        VoiceManager.Instance.StopVoice();

        if (LevelManager.Instance != null)
            LevelManager.Instance.LoadScene("Menu", "CrossFade");
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }

    // Volume
    public void UpdateMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20f);
    }

    public void UpdateSFXVolume(float volume)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20f);
    }

    public void UpdateVoiceVolume(float volume)
    {
        audioMixer.SetFloat("VoiceVolume", Mathf.Log10(volume) * 20f);
    }

    // Toggles
    public void ToggleUI(bool enabled)
    {
        if (uiElements == null) return;
        foreach (var el in uiElements)
        {
            if (el != null) el.SetActive(enabled);
        }
        PlayerPrefs.SetInt("UIEnabled", enabled ? 1 : 0);
    }

    public void ToggleFlying(bool enabled)
    {
        if (playerMovement == null) return;

        if (playerMovement.isFlying != enabled)
            playerMovement.SetFlying(enabled);

        PlayerPrefs.SetInt("FlyingEnabled", enabled ? 1 : 0);
    }

    // Save e load
    private void SaveSettings()
    {

        if (musicSlider != null) PlayerPrefs.SetFloat("MusicVolume", musicSlider.value);
        if (sfxSlider != null) PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);
        if (voiceSlider != null) PlayerPrefs.SetFloat("VoiceVolume", voiceSlider.value);

        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        if (musicSlider != null)
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        if (sfxSlider != null)
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        if (voiceSlider != null)
            voiceSlider.value = PlayerPrefs.GetFloat("VoiceVolume", 0.75f);

        bool ui = PlayerPrefs.GetInt("UIEnabled", 1) == 1;
        if (uiToggle != null) uiToggle.isOn = ui;
        ToggleUI(ui);

        bool fly = PlayerPrefs.GetInt("FlyingEnabled", 0) == 1;
        if (flyingToggle != null) flyingToggle.isOn = fly;
    }
}