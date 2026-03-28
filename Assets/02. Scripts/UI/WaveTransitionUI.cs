using System.Collections;
using TMPro;
using UnityEngine;

public class WaveTransitionUI : MonoBehaviour
{
    public static WaveTransitionUI Instance { get; private set; }

    [Header("텍스트")]
    [SerializeField] private string prefix = "Wave ";

    [Header("타이밍")]
    [SerializeField] private float fadeInDuration  = 0.3f;
    [SerializeField] private float holdDuration    = 1.2f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    [Header("참조 (UIBuilder로 자동 연결)")]
    [SerializeField] private CanvasGroup      canvasGroup;
    [SerializeField] private TextMeshProUGUI  label;

    private Coroutine animCoroutine;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        canvasGroup.alpha = 0f;
        WaveManager.OnWaveStarted += Show;
    }

    void OnDestroy() => WaveManager.OnWaveStarted -= Show;

    public void Show(int waveNumber)
    {
        label.text = prefix + waveNumber;
        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        yield return Fade(0f, 1f, fadeInDuration);
        yield return new WaitForSeconds(holdDuration);
        yield return Fade(1f, 0f, fadeOutDuration);
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}
