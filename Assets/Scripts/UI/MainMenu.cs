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
    [Header("Loading Screen")]
    [SerializeField] private Slider loadingBar;
    [SerializeField] private CanvasGroup loadingCanvasGroup;
    [SerializeField] private TextMeshProUGUI loadingMessageBox;
    [SerializeField] private string[] loadingMessages;
    [Header("Difficulty Sub-menu")]
    [SerializeField] private CanvasGroup difficultySubmenu;
    [SerializeField, TextArea] private string[] difficultyDescriptions;
    [SerializeField] private TextMeshProUGUI difficultyDescText;
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private Button dummyButton;

    private bool skipTutorial = false;
    private int difficulty = 1;

    private MusicFMOD musicFMOD;
    public MusicFMOD musicfmod;

    private void Start()
    {
        Time.timeScale = 1;

        if (GameObject.Find("MusicFMOD") != null)
        {
            musicFMOD = GameObject.Find("MusicFMOD").GetComponent<MusicFMOD>();
        }
        else if (GameObject.Find("MusicFMOD(Clone)") != null)
        {
            musicFMOD = GameObject.Find("MusicFMOD(Clone)").GetComponent<MusicFMOD>();
        }
        else
        {
            Instantiate(musicfmod);
            musicFMOD = musicfmod;
        }
        //musicFMOD = GameObject.Find("MusicFMOD").GetComponent<MusicFMOD>();
    }

    private void Update()
    {
        if (difficultySubmenu.alpha != 0 && !dropdown.IsExpanded && EventSystem.current.currentSelectedGameObject == dropdown.gameObject)
        {
            dummyButton.Select();
        }
    }

    public void ToggleTutorial(bool tutorialOn)
    {
        skipTutorial = !tutorialOn;
    }

    public void ChangeDifficulty(int diff)
    {
        difficulty = diff;
        difficultyDescText.text = difficultyDescriptions[difficulty];
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
        GlobalVars.LoadedFromMenu = true;
        mainCanvasGroup.DOFade(0, 0.5f)
            .OnComplete(
            delegate
            {
                StartCoroutine(Load());
            });
    }

    private IEnumerator Load()
    {
        //DontDestroyOnLoad(musicFMOD);
        yield return null;
        AsyncOperation loading = SceneManager.LoadSceneAsync(gameScene);
        loadingMessageBox.text = loadingMessages[Random.Range(0, loadingMessages.Length)];
        yield return loadingCanvasGroup.DOFade(1, 0.5f);
        InvokeRepeating("ChangeLoadingMessage", 3, 4);
        //loadingCanvasGroup.alpha = 1;
        while (!loading.isDone)
        {
            float progress = Mathf.Clamp01(loading.progress / 0.9f);
            loadingBar.DOValue(progress, 0.1f);
            yield return null;
        }
    }

    private void ChangeLoadingMessage()
    {
        loadingMessageBox.DOFade(0, 0.5f).OnComplete(
            delegate
            {
                string oldText = loadingMessageBox.text;
                while (loadingMessageBox.text == oldText)
                {
                    loadingMessageBox.text = loadingMessages[Random.Range(0, loadingMessages.Length)];
                }
                loadingMessageBox.DOFade(1, 0.5f);
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
