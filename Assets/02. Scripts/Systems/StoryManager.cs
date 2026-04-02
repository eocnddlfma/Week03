using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StoryManager : MonoBehaviour
{
    public static StoryManager Instance { get; private set; }

    [SerializeField] private StoryBoxUI ui;
    [SerializeField] private StoryGraph autoPlayOnStart;
    [SerializeField] private float typingSpeed = 40f; // 초당 글자 수

    public static event Action OnDialogueEnd;
    public bool IsPlaying { get; private set; }

    private StoryGraph  graph;
    private StoryNode   currentNode;
    private int         lineIndex;
    private bool        isTyping;
    private bool        waitingForInput;
    private Coroutine   activeCoroutine;
    private Action      onCompleteCallback;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        ui?.Hide();
    }

    void Start()
    {
        if (autoPlayOnStart) Play(autoPlayOnStart);
    }

    void Update()
    {
        if (!IsPlaying) return;
        if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
            Advance();
    }

    public void Play(StoryGraph storyGraph, Action onComplete = null)
    {
        if (storyGraph == null) { Debug.LogError("[StoryManager] storyGraph가 null"); return; }
        if (ui == null)         { Debug.LogError("[StoryManager] ui가 null — Inspector에서 StoryBoxUI 연결 필요"); return; }
        graph              = storyGraph;
        onCompleteCallback = onComplete;
        IsPlaying          = true;
        ui.Show();
        EnterNode(graph.StartNodeId);
    }

    public void Advance()
    {
        if (isTyping)      { SkipTyping(); return; }
        if (waitingForInput) { waitingForInput = false; NextLine(); }
    }

    void EnterNode(string nodeId)
    {
        if (string.IsNullOrEmpty(nodeId)) { EndDialogue(); return; }
        currentNode = graph.GetNode(nodeId);
        if (currentNode == null) { Debug.LogError($"[StoryManager] 노드 '{nodeId}' 없음 — StoryGraph에 노드 추가 필요"); EndDialogue(); return; }
        lineIndex = 0;
        ShowCurrentLine();
    }

    void NextLine()
    {
        lineIndex++;
        if (lineIndex < currentNode.Lines.Count) ShowCurrentLine();
        else                                      FinishNode();
    }

    void FinishNode()
    {
        if (!string.IsNullOrEmpty(currentNode.LoadScene))
        {
            EndDialogue(skipCallback: true);
            SceneManager.LoadScene(currentNode.LoadScene);
            return;
        }
        EnterNode(currentNode.NextNodeId);
    }

    void ShowCurrentLine()
    {
        if (currentNode.Lines == null || currentNode.Lines.Count == 0)
        { FinishNode(); return; }

        var line = currentNode.Lines[lineIndex];
        ui?.ShowContinueIndicator(false);
        ui?.SetSpeaker(line.SpeakerName);

        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        activeCoroutine = StartCoroutine(TypeLine(line.Text));
    }

    IEnumerator TypeLine(string text)
    {
        isTyping        = true;
        waitingForInput = false;

        ui?.SetText(text, 0);
        float elapsed = 0f;
        while (elapsed * typingSpeed < text.Length)
        {
            elapsed += Time.deltaTime;
            ui?.SetText(text, Mathf.FloorToInt(elapsed * typingSpeed));
            yield return null;
        }
        ui?.SetText(text, int.MaxValue);
        isTyping = false;

        ui?.ShowContinueIndicator(true);
        waitingForInput = true;
    }

    void SkipTyping()
    {
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        var line = currentNode.Lines[lineIndex];
        ui?.SetText(line.Text, int.MaxValue);
        isTyping = false;
        ui?.ShowContinueIndicator(true);
        waitingForInput = true;
    }

    void EndDialogue(bool skipCallback = false)
    {
        IsPlaying = false;
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        activeCoroutine = null;
        ui?.Hide();

        OnDialogueEnd?.Invoke();
        if (!skipCallback) onCompleteCallback?.Invoke();
        onCompleteCallback = null;
    }
}
