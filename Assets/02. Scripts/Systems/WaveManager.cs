using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    // 구독자: EnemySpawnManager, GamePhaseManager, CueController, WaveTransitionUI
    public static event Action<int> OnWaveStarted;

    [Header("흰색 공 분열")]
    [SerializeField] private int whiteBallSplitDelay = 2;

    [Header("경계 설정")]
    [SerializeField] private PolygonBoundary boundary;

    public int CurrentWave { get; private set; } = 0;

    // 흰색이 된 공 → 등록된 웨이브 번호 (분열 타이밍 추적용)
    private readonly Dictionary<BilliardBall, int> whiteBallWaveMap = new();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // GameInitializer 및 OnBallsAllStopped에서 호출
    public void StartWave(int wave)
    {
        CurrentWave = wave;

        // 경계 확장은 스폰 반경 계산보다 먼저 (순서 의존성)
        if (wave > 1)
        {
            if (boundary == null) boundary = FindAnyObjectByType<PolygonBoundary>();
            boundary?.GrowRadius();
        }

        SplitMatureWhiteBalls();
        OnWaveStarted?.Invoke(wave);
    }

    // GamePhaseManager에서 모든 공이 멈췄을 때 호출
    public void OnBallsAllStopped() => StartWave(CurrentWave + 1);

    // BilliardBall.OnColorChanged에서 흰색이 됐을 때 호출
    public void RegisterWhiteBall(BilliardBall ball)
    {
        if (!whiteBallWaveMap.ContainsKey(ball))
            whiteBallWaveMap[ball] = CurrentWave;
    }

    // BilliardBall.OnDeath에서 호출
    public void OnBallDefeated(BilliardBall ball) => whiteBallWaveMap.Remove(ball);

    private void SplitMatureWhiteBalls()
    {
        var toSplit = whiteBallWaveMap
            .Where(kv => kv.Value + whiteBallSplitDelay <= CurrentWave
                      && kv.Key != null
                      && kv.Key.gameObject.activeSelf)
            .Select(kv => kv.Key)
            .ToList();

        foreach (var white in toSplit)
        {
            whiteBallWaveMap.Remove(white);
            white.Replicate();
        }
    }
}
