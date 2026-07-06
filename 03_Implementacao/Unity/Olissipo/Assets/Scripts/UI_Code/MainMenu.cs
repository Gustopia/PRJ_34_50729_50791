/*
 * Este script é responsável por gerir o menu principal do jogo, incluindo a
 * navegação entre cenas, controlo do volume e salvar as preferências do utilizador.
 */

using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

// #my_code
public class MainMenu : MonoBehaviour
{
    public AudioMixer audioMixer;

    public Slider musicSlider;
    public Slider sfxSlider;
    public Slider voiceSlider;

    private void Start()
    {
        LoadVolume();
    }

    public void Play()
    {
        LevelManager.Instance.LoadScene("Olissipo_Superficie", "CrossFade");
    }

    public void ModelView3D()
    {
        LevelManager.Instance.LoadScene("Olissipo_360View", "CrossFade");
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void UpdateMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20f);
    }

    public void UpdateSoundVolume(float volume)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20f);
    }

    public void UpdateVoiceVolume(float volume)
    {
        audioMixer.SetFloat("VoiceVolume", Mathf.Log10(volume) * 20f);
    }

    public void SaveVolume()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);
        PlayerPrefs.SetFloat("VoiceVolume", voiceSlider.value);
        PlayerPrefs.Save();
    }

    public void LoadVolume()
    {
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        voiceSlider.value = PlayerPrefs.GetFloat("VoiceVolume", 0.75f);
    }
}