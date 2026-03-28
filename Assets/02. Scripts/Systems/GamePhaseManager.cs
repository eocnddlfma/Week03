using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GamePhaseManager : MonoBehaviour
{
    public static GamePhaseManager Instance { get; private set; }

    public GamePhase CurrentPhase    { get; private set; } = GamePhase.Billiard;
    public bool      IsExchangePhase => CurrentPhase == GamePhase.Exchange;

    private List<BilliardBall> balls = new List<BilliardBall>();
    private bool anyBallMoved; // 타구 후 실제로 움직인 공이 있어야만 Exchange → Billiard 전환

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        WaveManager.OnWaveStarted += OnWaveStarted;
    }

    void OnDestroy() => WaveManager.OnWaveStarted -= OnWaveStarted;

    private void OnWaveStarted(int _) => InitAllPhaseStats();

    public void RegisterBalls(List<BilliardBall> registeredBalls) => balls = registeredBalls;

    public void AddBall(BilliardBall ball)    => balls.Add(ball);
    public void RemoveBall(BilliardBall ball) => balls.Remove(ball);

    // 웨이브 시작 시 모든 공의 페이즈 스탯 초기화
    public void InitAllPhaseStats()
    {
        foreach (var b in balls)
        {
            if (b == null || !b.gameObject.activeSelf) continue;
            b.InitPhaseStats();
        }
    }

    // 현재 살아있는 모든 공의 위치 반환 (WaveManager 스폰 겹침 방지용)
    public List<Vector2> GetAllBallPositions() =>
        balls.Where(b => b != null && b.gameObject.activeSelf)
             .Select(b => b.Position)
             .ToList();

    // 타구 시 호출
    public void OnCueShot()
    {
        // 페이즈 전투 스탯 초기화 (모든 공의 PhaseAttack/PhaseDefense 리셋)
        foreach (var b in balls)
            if (b != null && b.gameObject.activeSelf)
                b.InitPhaseStats();

        CurrentPhase = GamePhase.Exchange;
        anyBallMoved = false;
    }

    void Update()
    {
        if (CurrentPhase != GamePhase.Exchange) return;

        // 한 번이라도 움직인 공이 생길 때까지 대기
        if (!anyBallMoved)
        {
            anyBallMoved = balls.Any(b => b != null && b.IsMoving);
            return;
        }

        // 살아있는 공이 모두 멈추면 Billiard 페이즈로
        var alive = balls.Where(b => b != null && b.gameObject.activeSelf).ToList();
        if (alive.All(b => !b.IsMoving))
            TransitionToBilliard();
    }

    private void TransitionToBilliard()
    {
        CurrentPhase = GamePhase.Billiard;
        TriggerMostActiveBallDialogue();
        WaveManager.Instance.OnBallsAllStopped();
    }

    private void TriggerMostActiveBallDialogue()
    {
        if (DialogueSystem.Instance == null) return;

        int minRequired = WaveManager.Instance.CurrentWave / 3;

        BilliardBall mostActive = null;
        int maxCollisions = minRequired - 1; // 최소 기준 미만이면 선택 안 됨
        foreach (var b in balls)
        {
            if (b == null || !b.gameObject.activeSelf) continue;
            if (b.PhaseCollisionCount > maxCollisions)
            {
                maxCollisions = b.PhaseCollisionCount;
                mostActive    = b;
            }
        }

        if (mostActive != null)
            DialogueSystem.Instance.TriggerDialogue(mostActive);
    }
}
