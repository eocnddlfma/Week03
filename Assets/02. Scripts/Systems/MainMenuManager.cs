using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Button tutorialButton;
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private string IntroSceneName = "IntroStory";
    [SerializeField] private string CharacterMakingSceneName = "CharacterMakingScene";

    void Start()
    {
        tutorialButton.onClick.AddListener(OnTutorial);
        startButton.onClick.AddListener(OnStart);
        quitButton.onClick.AddListener(OnQuit);
    }

    void OnTutorial() => SceneManager.LoadScene(IntroSceneName);
    void OnStart()    => SceneManager.LoadScene(CharacterMakingSceneName);
    void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
