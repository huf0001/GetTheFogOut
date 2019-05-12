using System.Collections;
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


    //Non-Serialized Fields
    //private AudioSource audioSource;
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

    private void ChangeTrack(AudioClip transition, AudioClip track)
    {
        doubleAudioSource.CrossFade(transition, 1, 5f, .5f);

        nextTrack = track;
        tracklength = transition.length;
        timeUntilChange = 0;
        changeTrack = true;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.C))
        {
            StartStage2();
        }

        if (changeTrack)
        {
            timeUntilChange += Time.deltaTime;
            if (timeUntilChange >= tracklength)
            {
                doubleAudioSource.CrossFade(nextTrack, 5, 0.01f);
                changeTrack = false;
            }
        }
    }
}
