using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;      // New Input System 사용을 위해 추가
using UnityEngine.SceneManagement; // 씬 로드를 위해 추가

public class GamePhaseManager : MonoBehaviour
{
    public static GamePhaseManager Instance { get; private set; }

    public GamePhase CurrentPhase    { get; private set; } = GamePhase.Billiard;
    public bool      IsExchangePhase => CurrentPhase == GamePhase.Exchange;

    // 아군 공 색상 풀 (7색) — 없는 색을 채워줌
    private static readonly ColorType[] AllyColorPool =
    {
        ColorType.Red, ColorType.Green, ColorType.Blue,
        ColorType.Yellow, ColorType.Cyan, ColorType.Magenta, ColorType.White
    };

    private List<BilliardBall> balls = new List<BilliardBall>(); // 'balls' 리스트 사용
    private bool anyBallMoved;

    // 3웨이브 리포트용 스냅샷 (아군 공만)
    private readonly Dictionary<BilliardBall, BallStats> phaseStartSnapshots = new();

    private PolygonBoundary boundary;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        WaveManager.OnWaveStarted += OnWaveStarted;
    }

    void OnDestroy() => WaveManager.OnWaveStarted -= OnWaveStarted;

    private void OnWaveStarted(int wave)
    {
        InitAllPhaseStats();
        // 3웨이브 블록 시작(1, 4, 7...) 시 스냅샷 저장
        if ((wave - 1) % 3 == 0)
            TakePhaseSnapshot();
    }

    private void TakePhaseSnapshot()
    {
        phaseStartSnapshots.Clear();
        foreach (var b in balls)
        {
            if (b == null || !b.gameObject.activeSelf || b.IsEnemy) continue;
            phaseStartSnapshots[b] = b.Stats.Clone();
        }
    }

    public void RegisterBalls(List<BilliardBall> registeredBalls) => balls = registeredBalls;

    // AddBall 메서드: 여기에 EnemyPresenceManager 호출을 추가합니다.
    public void AddBall(BilliardBall ball)
    {
        balls.Add(ball);
        // 블록 진행 중 합류한 아군 공은 합류 시점 스탯을 before로 등록
        if (!ball.IsEnemy && phaseStartSnapshots.Count > 0 && !phaseStartSnapshots.ContainsKey(ball))
            phaseStartSnapshots[ball] = ball.Stats.Clone();
        
        // EnemyPresenceManager에 공 추가 알림
        EnemyPresenceManager.Instance?.OnBallAdded(ball); // null 체크 추가
    }

    // RemoveBall 메서드: 여기에 EnemyPresenceManager 호출을 추가합니다.
    public void RemoveBall(BilliardBall ball)
    {
        balls.Remove(ball);
        // EnemyPresenceManager에 공 제거 알림
        EnemyPresenceManager.Instance?.OnBallRemoved(ball); // null 체크 추가
    }

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

        ExchangeSystem.Instance?.ResetPhase();
        CollisionLogger.NextTurn();
        CurrentPhase = GamePhase.Exchange;
        anyBallMoved = false;
    }

    void Update()
    {
         if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            Debug.Log("Restarting Game...");
            CollisionLogger.ResetAll(); 
            // 현재 활성화된 씬의 이름을 가져와서 다시 로드합니다.
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

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

        int wave = WaveManager.Instance.CurrentWave;
        Debug.Log($"[GPM] TransitionToBilliard wave={wave} snapshots={phaseStartSnapshots.Count}");

        // 3웨이브마다 리포트 표시
        if (wave % 3 == 0 && phaseStartSnapshots.Count > 0)
        {
            ShowPhaseReport();
            return;
        }

        ContinueAfterReport();
    }

    private void ShowPhaseReport()
    {
        var entries = new List<ReportEntry>();
        foreach (var b in balls)
        {
            if (b == null || !b.gameObject.activeSelf) continue;
            var after = b.Stats.Clone();
            if (!b.IsEnemy && phaseStartSnapshots.TryGetValue(b, out var before))
            {
                entries.Add(new ReportEntry
                {
                    Ball        = b,
                    Before      = before,
                    After       = after,
                    TotalDelta  = after.GetTotalStats() - before.GetTotalStats(),
                    HasSnapshot = true
                });
            }
            else
            {
                // 적이거나 이번 블록 이후 생성된 공 — 현재 스탯만 표시
                entries.Add(new ReportEntry
                {
                    Ball        = b,
                    Before      = after,
                    After       = after,
                    TotalDelta  = 0f,
                    HasSnapshot = false
                });
            }
        }
        // 아군(스냅샷 있음) 먼저, 그 다음 적; 각 그룹 내에선 TotalDelta 내림차순
        entries.Sort((a, x) =>
        {
            if (a.HasSnapshot != x.HasSnapshot) return x.HasSnapshot.CompareTo(a.HasSnapshot);
            return x.TotalDelta.CompareTo(a.TotalDelta);
        });

        Debug.Log($"[GPM] ShowPhaseReport entries={entries.Count} UI={PhaseReportUI.Instance != null}");

        if (PhaseReportUI.Instance != null && entries.Count > 0)
        {
            PhaseReportUI.OnClosed += OnReportClosed;
            PhaseReportUI.Instance.Show(entries);
        }
        else
        {
            Debug.LogWarning($"[GPM] 리포트 스킵 - UI:{PhaseReportUI.Instance != null} entries:{entries.Count}");
            ContinueAfterReport();
        }
    }

    private void OnReportClosed()
    {
        PhaseReportUI.OnClosed -= OnReportClosed;
        AddMissingAllyBall();
        ContinueAfterReport();
    }

    // 없는 색의 아군 공 최대 3개 추가 (이미 7색 모두 있으면 스킵)
    private void AddMissingAllyBall()
    {
        var existing = new HashSet<ColorType>();
        foreach (var b in balls)
        {
            if (b == null || !b.gameObject.activeSelf || b.IsEnemy) continue;
            existing.Add(b.Color.GetColorType());
        }

        if (boundary == null) boundary = FindAnyObjectByType<PolygonBoundary>();
        if (boundary == null || BallFactory.Instance == null) return;

        var used = GetAllBallPositions();
        float r  = boundary.Inradius - 0.5f - 0.3f;
        int added = 0;

        foreach (var c in AllyColorPool)
        {
            if (added >= 3) break;
            if (existing.Contains(c)) continue;

            Vector2 pos = FindFreePosition(boundary.Center, r, used);
            used.Add(pos);

            var newBall = BallFactory.Instance.Create(c, pos);
            AddBall(newBall);
            existing.Add(c);
            added++;
        }

        boundary.GrowStep(); // 공 추가 여부와 무관하게 경계 확장
    }

    private static Vector2 FindFreePosition(Vector2 center, float radius, List<Vector2> used)
    {
        const float minDist = 1.1f;
        for (int i = 0; i < 50; i++)
        {
            Vector2 p   = center + Random.insideUnitCircle * radius;
            bool    ok  = true;
            foreach (var u in used) if (Vector2.Distance(p, u) < minDist) { ok = false; break; }
            if (ok) return p;
        }
        return center + Random.insideUnitCircle * radius;
    }

    private void ContinueAfterReport()
    {
        bool dialogueStarted = TriggerMostActiveBallDialogue();
        if (dialogueStarted)
        {
            DialogueSystem.OnDialogueFinished += OnAfterDialogue;
        }
        else
        {
            ApplyAllPendingColors();
            WaveManager.Instance.OnBallsAllStopped();
        }
    }

    private void OnAfterDialogue()
    {
        DialogueSystem.OnDialogueFinished -= OnAfterDialogue;
        ApplyAllPendingColors();
        WaveManager.Instance.OnBallsAllStopped();
    }

    private void ApplyAllPendingColors()
    {
        foreach (var b in balls)
        {
            if (b == null || !b.gameObject.activeSelf) continue;
            b.ApplyPendingColor();
        }
    }

    private bool TriggerMostActiveBallDialogue()
    {
        if (DialogueSystem.Instance == null) return false;

        // 적이 맵에 있으면 조건 무관하게 가장 많이 충돌한 적 감정 우선
        BilliardBall mostActiveEnemy = null;
        int maxEnemyCollisions = -1;
        foreach (var b in balls)
        {
            if (b == null || !b.gameObject.activeSelf || !b.IsEnemy) continue;
            if (b.PhaseCollisionCount > maxEnemyCollisions)
            {
                maxEnemyCollisions = b.PhaseCollisionCount;
                mostActiveEnemy    = b;
            }
        }
        if (mostActiveEnemy != null)
            return DialogueSystem.Instance.TriggerDialogue(mostActiveEnemy);

        int minRequired = Mathf.FloorToInt(Mathf.Log(WaveManager.Instance.CurrentWave + 1, 4f));

        BilliardBall thresholdBall  = null; // 기준 이상 충돌한 공
        BilliardBall mostActiveBall = null; // 충돌 횟수 최다 공 (기준 무관)
        int maxAboveThreshold = minRequired - 1;
        int maxAny            = -1;

        foreach (var b in balls)
        {
            if (b == null || !b.gameObject.activeSelf) continue;
            if (b.PhaseCollisionCount > maxAny)
            {
                maxAny          = b.PhaseCollisionCount;
                mostActiveBall  = b;
            }
            if (b.PhaseCollisionCount > maxAboveThreshold)
            {
                maxAboveThreshold = b.PhaseCollisionCount;
                thresholdBall     = b;
            }
        }

        if (thresholdBall != null)
            return DialogueSystem.Instance.TriggerDialogue(thresholdBall);

        // 충돌 횟수 기준 미달 → Any 대사 (가장 많이 충돌한 공 포커스)
        if (mostActiveBall != null)
            return DialogueSystem.Instance.TriggerAnyDialogue(mostActiveBall);

        return false;
    }
}