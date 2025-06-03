using System;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    [SerializeField] private SettingsEnums.AudioType _audioType;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        ChangeAudio();
        UINoticer.OnSoundChanged += ChangeAudio;
    }

    private void OnDisable()
    {
        UINoticer.OnSoundChanged -= ChangeAudio;
    }

    private void ChangeAudio()
    {
        audioSource.volume = PlayerPrefs.GetFloat(_audioType.ToString());
    }
}