using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioMixer mixer;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;

    const string MIXER_MUSIC = "MusicVolume";
    const string MIXER_SFX = "SFXVolume";

    private void OnEnable()
    {
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    private void OnDisable()
    {
        musicSlider.onValueChanged.RemoveListener(SetMusicVolume);
        sfxSlider.onValueChanged.RemoveListener(SetSFXVolume);
    }

    private void Start()
    {
        float musicVolume = PlayerPrefs.GetFloat(MIXER_MUSIC, 1f);
        float sfxVolume = PlayerPrefs.GetFloat(MIXER_SFX, 1f);

        musicSlider.value = musicVolume;
        sfxSlider.value = sfxVolume;

        mixer.SetFloat(MIXER_MUSIC, Mathf.Log10(musicVolume) * 20);
        mixer.SetFloat(MIXER_SFX, Mathf.Log10(sfxVolume) * 20);
    }

    private void SetMusicVolume(float _value)
    {
        mixer.SetFloat(MIXER_MUSIC, Mathf.Log10(_value) * 20);

        PlayerPrefs.SetFloat(MIXER_MUSIC, _value);
    }

    private void SetSFXVolume(float _value)
    {
        mixer.SetFloat(MIXER_SFX, Mathf.Log10(_value) * 20);

        PlayerPrefs.SetFloat(MIXER_SFX, _value);
    }
}
