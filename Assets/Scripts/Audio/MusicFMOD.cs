using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicFMOD : MonoBehaviour
{
    [FMODUnity.EventRef]
    public string music = "event:/MUSIC/MusicMix";

    private FMOD.Studio.EventInstance musicEvent;
    private FMOD.Studio.PARAMETER_ID stageOne, stageTwo, stageThree, lose, win, outro;
    private FMOD.Studio.EventDescription stageOneDesc, stageTwoDesc, stageThreeDesc, loseDesc, winDesc, outroDesc;

    private static MusicFMOD _instance;

    // Start is called before the first frame update
    void Start()
    {
        if (GameObject.Find("MusicFMOD") != null)
        {
            Debug.Log("Start MusicFMOD");
            _instance = GameObject.Find("MusicFMOD").GetComponent<MusicFMOD>();
            Debug.Log("Instance: " + (_instance != null) + " This: " + (_instance != this));
            if (_instance != null && _instance != this)
            {
                Debug.Log("Instance not null. This Not Instance.");
                Destroy(gameObject);
                return;
            }
            else
            {
                Debug.Log("This becomes the Instance.");
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }
        else
        {
            Instantiate(gameObject);
            gameObject.name = "MusicFMOD";
            DontDestroyOnLoad(gameObject);
        }

        musicEvent = FMODUnity.RuntimeManager.CreateInstance(music);

        musicEvent.getDescription(out stageOneDesc);
        FMOD.Studio.PARAMETER_DESCRIPTION stageOneParamDesc;

        musicEvent.getDescription(out stageTwoDesc);
        FMOD.Studio.PARAMETER_DESCRIPTION stageTwoParamDesc;

        musicEvent.getDescription(out stageThreeDesc);
        FMOD.Studio.PARAMETER_DESCRIPTION stageThreeParamDesc;

        musicEvent.getDescription(out loseDesc);
        FMOD.Studio.PARAMETER_DESCRIPTION loseParamDesc;

        musicEvent.getDescription(out winDesc);
        FMOD.Studio.PARAMETER_DESCRIPTION winParamDesc;

        musicEvent.getDescription(out outroDesc);
        FMOD.Studio.PARAMETER_DESCRIPTION outroParamDesc;

        stageOneDesc.getParameterDescriptionByName("MusicStage1", out stageOneParamDesc);
        stageTwoDesc.getParameterDescriptionByName("MusicStage2", out stageTwoParamDesc);
        stageThreeDesc.getParameterDescriptionByName("MusicStage3", out stageThreeParamDesc);
        loseDesc.getParameterDescriptionByName("Lose", out loseParamDesc);
        winDesc.getParameterDescriptionByName("Win", out winParamDesc);
        outroDesc.getParameterDescriptionByName("ReturnMenu", out outroParamDesc);

        stageOne = stageOneParamDesc.id;
        stageTwo = stageTwoParamDesc.id;
        stageThree = stageThreeParamDesc.id;
        lose = loseParamDesc.id;
        win = winParamDesc.id;
        outro = outroParamDesc.id;
    }

    // Player begins the game from Menu, onto Stage 1 of Game. Will start here on Prototype Milestone 2 scene.
    public void StartMusic()
    {
        musicEvent.start();
    }

    public void StageOneMusic()
    {
        musicEvent.setParameterByID(lose, 0f);
        musicEvent.setParameterByID(win, 0f);
        musicEvent.setParameterByID(outro, 0f);
        musicEvent.setParameterByID(stageOne, 1f);
    }

    // First objective complete, move onto Stage 2 of Game.
    public void StageTwoMusic()
    {
        musicEvent.setParameterByID(stageTwo, 1f);
        musicEvent.setParameterByID(stageOne, 0f);
    }

    // Second objective complete, move onto Stage 3 of Game.
    public void StageThreeMusic()
    {
        musicEvent.setParameterByID(stageThree, 1f);
        musicEvent.setParameterByID(stageTwo, 0f);
    }

    // Player's ship is destroyed, GAME OVER. Return to Menu music.
    public void GameLoseMusic()
    {
        musicEvent.setParameterByID(stageOne, 0f);
        musicEvent.setParameterByID(stageTwo, 0f);
        musicEvent.setParameterByID(stageThree, 0f);
        musicEvent.setParameterByID(lose, 1f);
    }

    // Player leaves the planet, WIN! Return to Menu music.
    public void GameWinMusic()
    {
        musicEvent.setParameterByID(stageOne, 0f);
        musicEvent.setParameterByID(stageTwo, 0f);
        musicEvent.setParameterByID(stageThree, 0f);
        musicEvent.setParameterByID(win, 1f);
    }

    public void OutroMusic()
    {
        musicEvent.setParameterByID(stageOne, 0f);
        musicEvent.setParameterByID(stageTwo, 0f);
        musicEvent.setParameterByID(stageThree, 0f);
        musicEvent.setParameterByID(outro, 1f);
    }

    // Close the class.
    public void Release()
    {
        musicEvent.release();
    }
}
