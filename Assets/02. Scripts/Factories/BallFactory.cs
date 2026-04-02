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

    // 적 공에 적용할 기본 Material (옵션)
    [Header("적 공 머테리얼")]
    [SerializeField] private Material defaultEnemyMaterial; 

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        BallCombatStatUI.Container = statUIContainer;
    }

    // 플레이어 공 생성
    public BilliardBall Create(ColorType colorType, Vector2 position)
        => CreateInternal(colorType, position, playerStatMinValue, playerStatMaxValue, null); // 플레이어 공은 Material 없음

    // 스탯을 직접 지정해 생성 (분열 원본 공에 사용)
    public BilliardBall CreateWithStats(ColorType colorType, Vector2 position, BallStats inheritedStats)
    {
        var go   = Instantiate(ballPrefab, position, Quaternion.identity, ballContainer);
        var ball = go.GetComponent<BilliardBall>();
        ball.Stats.CopyFrom(inheritedStats);
        ball.Initialize(ColorState.From(colorType));
        // 이 메서드는 SetMaterial을 직접 호출하지 않음 (CreateInternal에서 처리)
        return ball;
    }

    // 적 공 생성 - 분열 시 기본 범위 사용 (Material 인자 추가)
    public BilliardBall CreateEnemy(ColorType colorType, Vector2 position, Material enemyMaterial = null)
    {
        // Material이 지정되지 않았다면 defaultEnemyMaterial 사용
        if (enemyMaterial == null) enemyMaterial = defaultEnemyMaterial;

        var ball = CreateInternal(colorType, position, replicateEnemyStatMin, replicateEnemyStatMax, enemyMaterial);
        ball.SetTeam(true);
        return ball;
    }

    // 적 공 - 스탯 직접 지정 (분열 원본 적 공에 사용) (Material 인자 추가)
    public BilliardBall CreateEnemyWithStats(ColorType colorType, Vector2 position, BallStats inheritedStats, Material enemyMaterial = null)
    {
        // Material이 지정되지 않았다면 defaultEnemyMaterial 사용
        if (enemyMaterial == null) enemyMaterial = defaultEnemyMaterial;

        // CreateInternal 대신 CreateWithStats를 호출하여 스탯을 복사하지만, Material은 여기서 따로 설정
        var ball = CreateWithStats(colorType, position, inheritedStats);
        ball.SetTeam(true);
        // CreateWithStats는 SetMaterial을 호출하지 않으므로 여기서 직접 호출
        if (enemyMaterial != null) ball.SetMaterial(enemyMaterial); 
        return ball;
    }

    private BilliardBall CreateInternal(ColorType colorType, Vector2 position,
                                        float minValue, float maxValue, Material materialToSet) // Material 인자 추가
    {
        // 충실한 기본기: 초기 스탯 범위 3배
        if (PlayerData.Instance?.HasTrait(PlayerTrait.SolidBase) == true)
        {
            minValue *= 3f;
            maxValue *= 3f;
        }

        var go   = Instantiate(ballPrefab, position, Quaternion.identity, ballContainer);
        var ball = go.GetComponent<BilliardBall>();
        ball.Stats.Randomize(minValue, maxValue);

        // 특성 스탯 수정 (적 공에는 적용 안 함 — SetTeam 전이라 IsEnemy=false이므로 조건 없이 적용)
        ApplyTraitStats(ball.Stats);

        ball.Initialize(ColorState.From(colorType));
        
        // Material이 있다면 설정
        if (materialToSet != null)
        {
            ball.SetMaterial(materialToSet);
        }
        
        return ball;
    }

    private static void ApplyTraitStats(BallStats s)
    {
        var pd = PlayerData.Instance;
        if (pd == null) return;

        if (pd.HasTrait(PlayerTrait.Sniper))   { s.Accuracy *= 1.3f; s.Attack  *= 1.2f; }
        if (pd.HasTrait(PlayerTrait.Smasher))  { s.Accuracy *= 0.7f; s.Speed   *= 1.5f; }
        if (pd.HasTrait(PlayerTrait.Headshot))   s.Critical *= 1.5f;
    }
}