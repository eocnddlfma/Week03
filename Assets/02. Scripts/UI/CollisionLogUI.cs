using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CollisionLogUI : MonoBehaviour
{
    public static CollisionLogUI Instance { get; private set; }

    [SerializeField] private ScrollRect   scrollRect;
    [SerializeField] private Transform    contentParent;
    [SerializeField] private GameObject   logLinePrefab;  // TMP_Text가 붙은 단순 GameObject
    [SerializeField] private int          maxLines = 80;
    [SerializeField] private CanvasGroup  canvasGroup;

    private readonly Queue<GameObject> pool    = new();
    private readonly List<GameObject>  active  = new();
    private bool collapsed = false;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }
    void Start()
    {
        foreach (var go in active)
        {
            go.SetActive(false);
            pool.Enqueue(go);
        }
        active.Clear();
    }

    void OnEnable()
    {
        CollisionLogger.OnLogged      += AddLine;
        CollisionLogger.OnWaveCleared += AddWaveDivider;
    }

    void OnDisable()
    {
        CollisionLogger.OnLogged      -= AddLine;
        CollisionLogger.OnWaveCleared -= AddWaveDivider;
    }

    public void AddLine(CollisionLogEntry entry)
    {
        string prefix = entry.Type switch
        {
            CollisionLogType.Combat       => "<color=#FF6B6B>[전투]</color>",
            CollisionLogType.StatExchange => "<color=#6BFF6B>[교환]</color>",
            CollisionLogType.Heal         => "<color=#6BFFFF>[힐]</color>",
            CollisionLogType.ColorMix     => "<color=#FF6BFF>[색상]</color>",
            CollisionLogType.Combo        => "<color=#FFD700>[콤보]</color>",
            CollisionLogType.Death        => "<color=#FF2222>[사망]</color>",
            _                             => ""
        };

        string fullText = $"<size=20><color=#AAAAAA>T{entry.Turn}</color> {prefix} {entry.Message}</size>";
        SpawnLine(fullText);
    }

    private void AddWaveDivider()
    {
        SpawnLine("<size=18><color=#555555>----- 웨이브 -----</color></size>");
    }

    private void SpawnLine(string text)
    {
        GameObject go = pool.Count > 0 ? pool.Dequeue() : Instantiate(logLinePrefab, contentParent);
        go.transform.SetParent(contentParent, false);
        go.SetActive(true);

        var tmp = go.GetComponent<TMP_Text>();
        if (tmp != null) tmp.text = text;

        active.Add(go);

        // 최대 라인 초과 시 가장 오래된 것 반환
        while (active.Count > maxLines)
        {
            var old = active[0];
            active.RemoveAt(0);
            old.SetActive(false);
            pool.Enqueue(old);
        }

        // 다음 프레임에 스크롤 최하단
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public void ToggleCollapse()
    {
        collapsed = !collapsed;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = collapsed ? 0.1f : 0.85f;
            // interactable/blocksRaycasts는 건드리지 않음 - 접힌 상태에서도 버튼 클릭 가능해야 함
        }
    }
}
