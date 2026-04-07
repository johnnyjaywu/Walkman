using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private List<AudioSource> music;
    [SerializeField] private List<AudioSource> ambience;
    [SerializeField] private AudioMixerSnapshot realitySnapshot;
    [SerializeField] private AudioMixerSnapshot alternateSnapshot;
    [SerializeField] private float fadeInTime = 1f;
    [SerializeField] private float fadeOutTime = 1f;
    
    private bool isMusicPlaying;

    private void Start()
    {
        TransitionToReality();
    }

    public void PlayMusic()
    {
        foreach (var m in music)
        {
            m.Play();
        }

        isMusicPlaying = true;
    }

    [Button]
    public void TransitionToReality()
    {
        realitySnapshot.TransitionTo(fadeOutTime);
    }

    [Button]
    public void TransitionToAlternate()
    {
        if (!isMusicPlaying) PlayMusic();
        alternateSnapshot.TransitionTo(fadeInTime);
    }
}