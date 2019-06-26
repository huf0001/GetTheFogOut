﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string gameScene;
    [SerializeField] private Image playButton;
    [SerializeField] private TextMeshProUGUI playText;
    [SerializeField] private Image quitButton;
    [SerializeField] private TextMeshProUGUI quitText;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private CanvasGroup tutorialToggle;
    [SerializeField] private Slider loadingBar;
    [SerializeField] private CanvasGroup loadingCanvasGroup;
    private bool skipTutorial = false;

    public void ToggleTutorial(bool tutorialOn)
    {
        skipTutorial = !tutorialOn;
    }

    public void playGame()
    {
        GlobalVars.SkipTut = skipTutorial;
        Sequence sequence = DOTween.Sequence();
        sequence.Insert(0, playButton.DOFade(0, 0.5f))
            .Insert(0, quitButton.DOFade(0, 0.5f))
            .Insert(0, titleText.DOFade(0, 0.5f))
            .Insert(0, quitText.DOFade(0, 0.5f))
            .Insert(0, playText.DOFade(0, 0.5f))
            .Insert(0, tutorialToggle.DOFade(0, 0.5f))
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
