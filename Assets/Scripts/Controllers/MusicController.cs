﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MusicController : MonoBehaviour
{
    //Single Pattern
    private static MusicController _instance;
    public static MusicController Instance { get { return _instance; } }

    //Serialised Fields
    [SerializeField] private DoubleAudioSource doubleAudioSource;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip acMenutoStage1;
    [SerializeField] private AudioClip acStage1;
    [SerializeField] private AudioClip acStageToStage2;
    [SerializeField] private AudioClip acStage2;
    [SerializeField] private AudioClip acStage2ToStage3;
    [SerializeField] private AudioClip acStage3;
    [SerializeField] private AudioClip acOutroWin;
    [SerializeField] private AudioClip acOutroLose;


    //Non-Serialized Fields
    private AudioClip nextTrack;
    float tracklength;
    float timeUntilChange;
    bool changeTrack;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
        audioSource.volume = 0;
    }

    private void Start()
    {
        audioSource.DOFade(1, 20).SetUpdate(true);
    }


    public void SkipTutorial()
    {
        audioSource.clip = acStage1;
        audioSource.Play();
    }

    public void StartStage1()
    {
        ChangeTrack(acMenutoStage1, acStage1);
    }

    public void StartStage2()
    {
        ChangeTrack(acStageToStage2, acStage2);
    }

    public void StartStage3()
    {
        ChangeTrack(acStage2ToStage3, acStage3);
    }

    public void StartOutroWin()
    {
        doubleAudioSource.CrossFade(acOutroWin, 1, 3);
        foreach(AudioSource a in GetComponents<AudioSource>())
        {
            a.loop = false;
        }
    }

    public void StartOutroLose()
    {
        doubleAudioSource.CrossFade(acOutroLose, 1, 3);
        foreach (AudioSource a in GetComponents<AudioSource>())
        {
            a.loop = false;
        }
    }

    private void ChangeTrack(AudioClip transition, AudioClip track)
    {
        doubleAudioSource.CrossFade(transition, 1, 3f);

        tracklength = transition.length;

        doubleAudioSource.CrossFade(track, 1, 1f, tracklength - 2f);
    }
}
