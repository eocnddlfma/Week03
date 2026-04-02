using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct ReportEntry
{
    public BilliardBall Ball;
    public BallStats    Before;
    public BallStats    After;
    public float        TotalDelta;  // 총 스탯 변화량 (정렬 기준)
    public bool         HasSnapshot; // false = 적이거나 이번 블록 이후 생성된 공
}

public class PhaseReportUI : MonoBehaviour
{
    public static PhaseReportUI Instance { get; private set; }
    public static event Action OnClosed;

    [SerializeField] private GameObject      panelRoot;
    [SerializeField] private Transform       cardContainer;
    [SerializeField] private GameObject      cardPrefab;
    [SerializeField] private ScrollRect      scrollRect;

    private readonly List<GameObject> activeCards = new();
    private bool isShowing;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        panelRoot.SetActive(false);
    }

    void Update()
    {
        if (!isShowing) return;
        if (Input.GetMouseButtonDown(0)) Hide();
    }

    public void Show(List<ReportEntry> entries)
    {
        ClearCards();

        foreach (var entry in entries)
        {
            var go   = Instantiate(cardPrefab, cardContainer);
            var card = go.GetComponent<BallReportCard>();
            card.Setup(entry);
            activeCards.Add(go);
        }

        panelRoot.SetActive(true);
        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 1f;
        }
        isShowing = true;
    }

    private void Hide()
    {
        isShowing = false;
        panelRoot.SetActive(false);
        ClearCards();
        OnClosed?.Invoke();
    }

    private void ClearCards()
    {
        foreach (var go in activeCards)
            Destroy(go);
        activeCards.Clear();
    }
}
