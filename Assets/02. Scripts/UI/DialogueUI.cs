using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct EmotionImageEntry
{
    public ColorType emotion;
    public Sprite    sprite;
}

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }

    [Header("표시 시간")]
    [SerializeField] private float displayDuration  = 6f;
    [SerializeField] private float charsPerSecond   = 20f; // 타이핑 속도 (초당 글자 수)

    public float DisplayDuration => displayDuration;

    [Header("참조 (UIBuilder로 자동 연결)")]
    [SerializeField] private GameObject      canvasRoot;
    [SerializeField] private Image           panelImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("감정 이미지")]
    [SerializeField] private Image               emotionImage;
    [SerializeField] private EmotionImageEntry[] emotionImages;

    private Coroutine hideCoroutine;
    private Coroutine typeCoroutine;
    private string    _fullText;

    public bool IsShowing => canvasRoot != null && canvasRoot.activeSelf;
    public bool IsTyping  => typeCoroutine != null;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        SetVisible(false);
    }

    public void Show(string ballName, string text, ColorType emotion)
    {
        _fullText = text;
        var (textCol, bgCol) = EmotionColors(emotion);

        panelImage.color   = bgCol;
        nameText.color     = textCol;
        dialogueText.color = textCol;
        nameText.text     = ballName;
        dialogueText.text = "";

        if (emotionImage != null)
        {
            var entry = System.Array.Find(emotionImages, e => e.emotion == emotion);
            emotionImage.sprite  = entry.sprite;
            emotionImage.enabled = entry.sprite != null;
        }

        SetVisible(true);

        if (typeCoroutine != null) StopCoroutine(typeCoroutine);
        typeCoroutine = StartCoroutine(TypeText(text));

        if (hideCoroutine != null) StopCoroutine(hideCoroutine);
        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    // 타이핑 중일 때: 텍스트 즉시 완성
    public void CompleteText()
    {
        if (typeCoroutine != null) { StopCoroutine(typeCoroutine); typeCoroutine = null; }
        dialogueText.text = _fullText;
    }

    // 대사 즉시 닫기
    public void ForceHide()
    {
        if (typeCoroutine != null) { StopCoroutine(typeCoroutine); typeCoroutine = null; }
        if (hideCoroutine != null) { StopCoroutine(hideCoroutine); hideCoroutine = null; }
        SetVisible(false);
    }

    private IEnumerator TypeText(string text)
    {
        dialogueText.text = "";
        float interval = 1f / Mathf.Max(charsPerSecond, 1f);
        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(interval);
        }
        typeCoroutine = null;
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        SetVisible(false);
    }

    private void SetVisible(bool visible) => canvasRoot.SetActive(visible);

    private static (Color text, Color bg) EmotionColors(ColorType emotion) => emotion switch
    {
        ColorType.Red       => (new Color(1f,    0.40f, 0.40f), new Color(0.22f, 0.04f, 0.04f, 0.90f)),
        ColorType.Blue      => (new Color(0.40f, 0.60f, 1f   ), new Color(0.04f, 0.06f, 0.22f, 0.90f)),
        ColorType.Green     => (new Color(0.40f, 1f,   0.50f ), new Color(0.04f, 0.18f, 0.06f, 0.90f)),
        ColorType.Yellow    => (new Color(1f,    0.95f, 0.30f), new Color(0.20f, 0.16f, 0.02f, 0.90f)),
        ColorType.Cyan      => (new Color(0.30f, 0.95f, 1f   ), new Color(0.02f, 0.16f, 0.20f, 0.90f)),
        ColorType.Magenta   => (new Color(1f,    0.40f, 0.95f), new Color(0.18f, 0.02f, 0.18f, 0.90f)),
        ColorType.White     => (new Color(0.90f, 0.90f, 0.90f), new Color(0.10f, 0.10f, 0.12f, 0.90f)),
        ColorType.Gray      => (new Color(0.72f, 0.72f, 0.72f), new Color(0.06f, 0.06f, 0.06f, 0.92f)),
        ColorType.Black     => (new Color(0.78f, 0.78f, 0.85f), new Color(0.03f, 0.03f, 0.06f, 0.94f)),
        ColorType.DeepBlack => (new Color(0.70f, 0.50f, 0.90f), new Color(0.02f, 0.00f, 0.06f, 0.96f)),
        _                   => (Color.white,                     new Color(0f,    0f,    0f,    0.82f)),
    };
}
