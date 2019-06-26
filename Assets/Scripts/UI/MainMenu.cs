using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string gameScene;
    [SerializeField] private CanvasGroup mainCanvasGroup;
    [SerializeField] private TextMeshProUGUI difficultyButtonText;
    [SerializeField] private Slider loadingBar;
    [SerializeField] private CanvasGroup loadingCanvasGroup;
    [SerializeField] private CanvasGroup difficultySubmenu;
    [SerializeField, TextArea] private string[] difficultyDescriptions;
    [SerializeField] private TextMeshProUGUI difficultyDescText;

    private bool skipTutorial = false;
    private int difficulty = 1;

    public void ToggleTutorial(bool tutorialOn)
    {
        skipTutorial = !tutorialOn;
    }

    public void ChangeDifficulty(int diff)
    {
        difficulty = diff;
        difficultyDescText.text = difficultyDescriptions[difficulty];
        StartCoroutine(Unhover());
    }

    private IEnumerator Unhover()
    {
        difficultySubmenu.blocksRaycasts = false;
        yield return new WaitForSeconds(0.1f);
        difficultySubmenu.blocksRaycasts = true;
    }

    public void OpenMenu()
    {
        StartCoroutine(OpenDifficultyMenu());
    }

    private IEnumerator OpenDifficultyMenu()
    {
        mainCanvasGroup.blocksRaycasts = false;
        yield return new WaitForSeconds(0.1f);
        mainCanvasGroup.interactable = false;
        mainCanvasGroup.DOFade(0, 0.5f)
            .OnComplete(
            delegate
            {
                difficultySubmenu.DOFade(1, 0.5f);
                difficultySubmenu.interactable = true;
                difficultySubmenu.blocksRaycasts = true;
            });
    }

    public void CloseMenu()
    {
        StartCoroutine(CloseDifficultyMenu());
    }

    private IEnumerator CloseDifficultyMenu()
    {
        switch (difficulty)
        {
            case 0:
                difficultyButtonText.text = "Difficulty: Easy";
                break;
            case 1:
                difficultyButtonText.text = "Difficulty: Medium";
                break;
            case 2:
                difficultyButtonText.text = "Difficulty: Hard";
                break;
        }
        difficultySubmenu.blocksRaycasts = false;
        yield return new WaitForSeconds(0.1f);
        difficultySubmenu.interactable = false;
        difficultySubmenu.DOFade(0, 0.5f)
            .OnComplete(
            delegate
            {
                mainCanvasGroup.DOFade(1, 0.5f);
                mainCanvasGroup.interactable = true;
                mainCanvasGroup.blocksRaycasts = true;
            });
    }

    public void playGame()
    {
        GlobalVars.SkipTut = skipTutorial;
        GlobalVars.Difficulty = difficulty;
        mainCanvasGroup.DOFade(0, 0.5f)
            .OnComplete(
            delegate
            {
                StartCoroutine(Load());
            });
    }

    private IEnumerator Load()
    {
        yield return null;
        AsyncOperation loading = SceneManager.LoadSceneAsync(gameScene);
        //loadingCanvasGroup.DOFade(1, 0.5f).OnComplete(
        //    delegate
        //    {
        //    });
        loadingCanvasGroup.alpha = 1;
        while (!loading.isDone)
        {
            float progress = Mathf.Clamp01(loading.progress / 0.9f);
            //Debug.Log($"Loading progress: {progress * 100}%");
            loadingBar.DOValue(progress, 0.1f);
            yield return null;
        }
    }

    public void quitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}
