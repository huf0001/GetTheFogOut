using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicFMOD : MonoBehaviour
{
    [FMODUnity.EventRef]
    public string music = "event:/MUSIC/Music2D";

    FMOD.Studio.EventInstance musicEvent;
    FMOD.Studio.PARAMETER_ID stageOne, stageTwo, stageThree, lose, win;
    FMOD.Studio.EventDescription stageOneDesc, stageTwoDesc, stageThreeDesc, loseDesc, winDesc;

    // Start is called before the first frame update
    void Start()
    {
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

        stageOneDesc.getParameterDescriptionByName("MusicStage1", out stageOneParamDesc);
        stageTwoDesc.getParameterDescriptionByName("MusicStage2", out stageTwoParamDesc);
        stageThreeDesc.getParameterDescriptionByName("MusicStage3", out stageThreeParamDesc);
        loseDesc.getParameterDescriptionByName("Lose", out loseParamDesc);
        winDesc.getParameterDescriptionByName("Win", out winParamDesc);

        stageOne = stageOneParamDesc.id;
        stageTwo = stageTwoParamDesc.id;
        stageThree = stageThreeParamDesc.id;
        lose = loseParamDesc.id;
        win = winParamDesc.id;

        musicEvent.start();
        DontDestroyOnLoad(this);
    }

    // Player begins the game from Menu, onto Stage 1 of Game. Will start here on Prototype Milestone 2 scene.
    public void StageOneMusic()
    {
        musicEvent.setParameterByID(lose, 0f);
        musicEvent.setParameterByID(win, 0f);
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

    // Close the class.
    public void Release()
    {
        musicEvent.release();
    }
}
