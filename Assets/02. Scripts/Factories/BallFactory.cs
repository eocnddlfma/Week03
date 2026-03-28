using UnityEngine;

public class BallFactory : MonoBehaviour
{
    public static BallFactory Instance { get; private set; }

    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Transform  ballContainer;   // 공들을 묶을 부모 오브젝트
    [SerializeField] private Transform  statUIContainer; // CombatStatUI를 묶을 UI 부모 오브젝트

    [Header("플레이어 공 스탯 랜덤 범위")]
    [SerializeField] private float playerStatMinValue = BallStats.DefaultMinValue;
    [SerializeField] private float playerStatMaxValue = BallStats.DefaultMaxValue;

    [Header("분열 적 공 기본 랜덤 범위 (Replicate용)")]
    [SerializeField] private float replicateEnemyStatMin = BallStats.DefaultMinValue;
    [SerializeField] private float replicateEnemyStatMax = BallStats.DefaultMaxValue;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        BallCombatStatUI.Container = statUIContainer;
    }

    // 플레이어 공 생성
    public BilliardBall Create(ColorType colorType, Vector2 position)
        => CreateInternal(colorType, position, playerStatMinValue, playerStatMaxValue);

    // 스탯을 직접 지정해 생성 (분열 원본 공에 사용)
    public BilliardBall CreateWithStats(ColorType colorType, Vector2 position, BallStats inheritedStats)
    {
        var go   = Instantiate(ballPrefab, position, Quaternion.identity, ballContainer);
        var ball = go.GetComponent<BilliardBall>();
        ball.Stats.CopyFrom(inheritedStats);
        ball.Initialize(ColorState.From(colorType), AttackBehaviorFactory.Create(colorType));
        return ball;
    }

    // 적 공 생성 - 분열 시 기본 범위 사용
    public BilliardBall CreateEnemy(ColorType colorType, Vector2 position)
    {
        var ball = CreateInternal(colorType, position, replicateEnemyStatMin, replicateEnemyStatMax);
        ball.SetTeam(true);
        return ball;
    }

    // 적 공 - 스탯 직접 지정 (분열 원본 적 공에 사용)
    public BilliardBall CreateEnemyWithStats(ColorType colorType, Vector2 position, BallStats inheritedStats)
    {
        var ball = CreateWithStats(colorType, position, inheritedStats);
        ball.SetTeam(true);
        return ball;
    }

    private BilliardBall CreateInternal(ColorType colorType, Vector2 position,
                                        float minValue, float maxValue)
    {
        var go   = Instantiate(ballPrefab, position, Quaternion.identity, ballContainer);
        var ball = go.GetComponent<BilliardBall>();
        ball.Stats.Randomize(minValue, maxValue);
        ball.Initialize(ColorState.From(colorType), AttackBehaviorFactory.Create(colorType));
        return ball;
    }
}
