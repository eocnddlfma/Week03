using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingManager : MonoBehaviour
{
    [SerializeField] private string endingScene = "EndingScene";

    void OnEnable()  => BilliardBall.OnLastDeepBlackDefeated += GoToEnding;
    void OnDisable() => BilliardBall.OnLastDeepBlackDefeated -= GoToEnding;

    void GoToEnding() => SceneManager.LoadScene(endingScene);
}
