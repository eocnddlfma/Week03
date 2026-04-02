using System;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    // 구독자: EnemySpawnManager, GamePhaseManager, CueController, WaveTransitionUI
    public static event Action<int> OnWaveStarted;

    public int CurrentWave { get; private set; } = 0;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // 여기에서 웨이브를 초기화합니다.
        // 예를 들어, 첫 웨이브부터 시작하려면 0 또는 1을 넣어주세요.
        StartWave(1); // 1부터 시작하는 웨이브
    }

    // GameInitializer 및 OnBallsAllStopped에서 호출
    public void StartWave(int wave)
    {
        CurrentWave = wave;

        OnWaveStarted?.Invoke(wave);
    }

    // GamePhaseManager에서 모든 공이 멈췄을 때 호출
    public void OnBallsAllStopped() => StartWave(CurrentWave + 1);
}