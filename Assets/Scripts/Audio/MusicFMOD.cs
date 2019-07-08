using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicFMOD : MonoBehaviour
{
    [FMODUnity.EventRef]
    public string music = "event:/MUSIC/Music2D";

    FMOD.Studio.EventInstance musicEvent;
    FMOD.Studio.PARAMETER_ID StartGame, Stage2, Stage3, IsReturnMenu, IsLoseGame, IsWinGame;
    FMOD.Studio.EventDescription startGameDesc, stage2Desc, stage3Desc, isReturnMenuDesc, isLoseGameDesc, isWinGameDesc;

    // Start is called before the first frame update
    void Start()
    {
        musicEvent = FMODUnity.RuntimeManager.CreateInstance(music);

        musicEvent.getDescription(out startGameDesc);
        FMOD.Studio.PARAMETER_DESCRIPTION startGameParameterDesc;
        startGameDesc.getParameterDescriptionByName("StartGame", out startGameParameterDesc);
        StartGame = startGameParameterDesc.id;

        musicEvent.getDescription(out stage2Desc);
        FMOD.Studio.PARAMETER_DESCRIPTION stage2ParameterDesc;
        stage2Desc.getParameterDescriptionByName("Stage2", out stage2ParameterDesc);
        Stage2 = stage2ParameterDesc.id;

        musicEvent.getDescription(out stage3Desc);
        FMOD.Studio.PARAMETER_DESCRIPTION stage3ParameterDesc;
        stage3Desc.getParameterDescriptionByName("Stage3", out stage3ParameterDesc);
        Stage3 = stage3ParameterDesc.id;

        musicEvent.getDescription(out isReturnMenuDesc);
        FMOD.Studio.PARAMETER_DESCRIPTION isReturnMenuParameterDesc;
        isReturnMenuDesc.getParameterDescriptionByName("IsReturnMenu", out isReturnMenuParameterDesc);
        IsReturnMenu = isReturnMenuParameterDesc.id;

        musicEvent.getDescription(out isLoseGameDesc);
        FMOD.Studio.PARAMETER_DESCRIPTION isLoseGameParameterDesc;
        isLoseGameDesc.getParameterDescriptionByName("IsLoseGame", out isLoseGameParameterDesc);
        IsLoseGame = isLoseGameParameterDesc.id;

        musicEvent.getDescription(out isWinGameDesc);
        FMOD.Studio.PARAMETER_DESCRIPTION isWinGameParameterDesc;
        isWinGameDesc.getParameterDescriptionByName("IsWinGame", out isWinGameParameterDesc);
        IsWinGame = isWinGameParameterDesc.id;

        musicEvent.start();
        DontDestroyOnLoad(this);
    }

    // Player begins the game from Menu, onto Stage 1 of Game. Will start here on Prototype Milestone 2 scene.
    public void StageOneMusic()
    {
        musicEvent.setParameterByID(StartGame, 1f);
    }

    // First objective complete, move onto Stage 2 of Game.
    public void StageTwoMusic()
    {
        musicEvent.setParameterByID(Stage2, 1f);
    }

    // Second objective complete, move onto Stage 3 of Game.
    public void StageThreeMusic()
    {
        musicEvent.setParameterByID(Stage3, 1f);
    }

    // Player quits the game, return to Menu scene, stop music.
    public void ReturnMenuMusic()
    {
        musicEvent.setParameterByID(IsReturnMenu, 1f);
    }

    // Player's ship is destroyed, GAME OVER.
    public void GameLoseMusic()
    {
        musicEvent.setParameterByID(IsLoseGame, 1f);
    }

    // Player leaves the planet, WIN!
    public void GameWinMusic()
    {
        musicEvent.setParameterByID(IsWinGame, 1f);
    }

    // Update is called once per frame
    //void Update()
    //{
        //
    //}
}
