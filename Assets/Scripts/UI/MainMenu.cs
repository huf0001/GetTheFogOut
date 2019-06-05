using System.Collections;
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

    public void playGame()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Insert(0, playButton.DOFade(0, 0.5f))
            .Insert(0, quitButton.DOFade(0, 0.5f))
            .Insert(0, titleText.DOFade(0, 0.5f))
            .Insert(0, quitText.DOFade(0, 0.5f))
            .Insert(0, playText.DOFade(0, 0.5f))
            .OnComplete(
            delegate
            {
                SceneManager.LoadScene(gameScene);
            });
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
