using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class BilliardBall : MonoBehaviour
{
    [SerializeField] private BallStats stats = new BallStats();

    private static int nextId = 0;

    public int         Id                  { get; private set; }
    public string      MemoryName          { get; private set; }
    public int         MemoryAge           { get; private set; }
    public int         PhaseCollisionCount { get; private set; }
    public ColorState  Color               { get; private set; }
    public ColorState  OriginalColor { get; private set; } // 최초 생성 색상 - 복제 시 사용
    public BallStats   Stats         => stats;
    public Vector2     Position      => Rigidbody.position;
    public bool        IsEnemy       { get; private set; }
    public ColorType   WaveStartEmotionType  { get; private set; } = ColorType.Any; // 웨이브 시작 시 감정
    public bool        ColorChangedThisPhase { get; private set; }                  // 이 페이즈에 색 변화 여부

    public Rigidbody2D Rigidbody { get; private set; }

    private SpriteRenderer  spriteRenderer;
    private IEmotionModifier emotionModifier;
    private IAttackBehavior  attackBehavior;

    [Header("크기 설정")]
    [SerializeField] private float statSizeMin    = 0.5f;  // 스탯 합산 최저일 때 크기
    [SerializeField] private float statSizeMax    = 10.0f; // 스탯 합산 최고일 때 크기
    [SerializeField] private float statSizeRefMax = 240f;  // 크기 최대치 기준 스탯 합산 (8스탯 × 30)

    [Header("물리 설정")]
    [SerializeField] private float stopThreshold      = 0.3f;  // 이 속도 이하면 즉시 정지
    [SerializeField] private float isMovingThreshold  = 0.01f; // IsMoving 판정 기준
    [SerializeField] private float dampingAtLowSpeed  = 0.25f; // Speed 최저 시 linearDamping
    [SerializeField] private float dampingAtMidSpeed  = 0.1f;  // Speed 중간 시 linearDamping
    [SerializeField] private float dampingAtHighSpeed = 0.01f; // Speed 최고 시 linearDamping
    [SerializeField] private float replicateOffset    = 0.5f;  // 분열 시 좌우 간격

    [Header("열정 설정")]
    [SerializeField] private int   passionBudgetMin        = 2;
    [SerializeField] private int   passionBudgetMax        = 12;
    [SerializeField] private float majorPassionChance      = 0.5f; // 2단계 열정 배정 확률
    [SerializeField] private float minorPassionMultiplier  = 2f;
    [SerializeField] private float majorPassionMultiplier  = 5f;

[Header("UI 프리팹")]
    [SerializeField] private BallCombatStatUI statUIPrefab;


    public ColorType EmotionColorType => emotionModifier?.ColorType ?? ColorType.Any;

    public bool  CanReceiveExchange => !Color.IsWhite();
    public bool  IsMoving           => Rigidbody.linearVelocity.magnitude > isMovingThreshold;
    public float CurrentHP          => currentHP;

    // 페이즈마다 초기화되는 전투용 공격/방어량
    public float PhaseAttack  { get; private set; }
    public float PhaseDefense { get; private set; }

    private float currentHP;
    private BallCombatStatUI combatStatUI;

    // 열정 시스템 - 스탯별 열정 레벨
    private readonly System.Collections.Generic.Dictionary<StatType, PassionLevel> passions = new();

    private static readonly StatType[] AllStatTypes =
    {
        StatType.Attack, StatType.Defense, StatType.HP, StatType.Speed,
        StatType.Evasion, StatType.Accuracy, StatType.Critical, StatType.Heal
    };

    public void SetTeam(bool isEnemy) => IsEnemy = isEnemy;

    public PassionLevel GetPassion(StatType stat) =>
        passions.TryGetValue(stat, out var p) ? p : PassionLevel.None;

    public float GetPassionMultiplier(StatType stat) => GetPassion(stat) switch
    {
        PassionLevel.Minor => minorPassionMultiplier,
        PassionLevel.Major => majorPassionMultiplier,
        _                  => 1f
    };

    void Awake()
    {
        Id             = nextId++;
        Rigidbody      = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        Rigidbody.gravityScale = 0f; // 탑다운 2D - 중력 없음
    }

    void FixedUpdate()
    {
        // 다른 콜라이더와 접촉 중이면 물리엔진에 맡김 (벽에 붙어 멈추는 현상 방지)
        if (Rigidbody.linearVelocity.magnitude < stopThreshold && !Rigidbody.IsTouching(ContactFilter2D.noFilter))
            Rigidbody.linearVelocity = Vector2.zero;
    }

    public void Initialize(ColorState colorState, IAttackBehavior initialBehavior)
    {
        attackBehavior  = initialBehavior;
        emotionModifier = EmotionModifierFactory.Create(IsEnemy ? RandomEnemyEmotionType() : colorState.GetColorType());
        Color           = colorState;
        OriginalColor   = ExtractPrimaryColor(colorState); // 항상 원색만 기록
        currentHP       = stats.MaxHP;
        spriteRenderer.color = IsEnemy ? GetEnemySpriteColor() : colorState.ToUnityColor();
        MemoryName = BallMemoryNameGenerator.Next();
        MemoryAge  = int.Parse(MemoryName.Split('살')[0]);
        ApplySpeedToDamping();
        RefreshSizeFromStats();
        AssignRandomPassions();
        InitPhaseStats();

        // 공 자식으로 붙어있는 경우 우선 사용, 없으면 프리팹에서 생성
        combatStatUI = GetComponentInChildren<BallCombatStatUI>(true);
        if (combatStatUI == null && statUIPrefab != null)
            combatStatUI = Instantiate(statUIPrefab);
        combatStatUI?.Setup(this);
    }

    // Exchange 페이즈 시작 시 호출 - 페이즈 전투 스탯 초기화
    public void InitPhaseStats()
    {
        var eff              = GetEffectiveStats();
        PhaseAttack          = eff.Attack;
        PhaseDefense         = eff.Defense;
        PhaseCollisionCount   = 0;
        ColorChangedThisPhase = false;
        WaveStartEmotionType  = EmotionColorType; // 웨이브 시작 시점 감정 기록
        RefreshUI();
    }

    // 방어(보호막)가 먼저 흡수, 남은 피해는 HP에 적용
    public void TakeShieldedDamage(float damage)
    {
        if (damage <= 0f) return;
        float absorbed = Mathf.Min(PhaseDefense, damage);
        PhaseDefense -= absorbed;
        damage       -= absorbed;
        RefreshUI();
        if (damage > 0f) TakeRawDamage(damage);
    }

    private void RefreshUI() =>
        combatStatUI?.Refresh(PhaseAttack, PhaseDefense, currentHP,
                              GetPassion(StatType.Attack), GetPassion(StatType.Defense));

    // 페이즈 스탯 기반 데미지 (방어 중복 적용 없음)
    public void TakeRawDamage(float damage)
    {
        if (damage <= 0f) return;
        currentHP -= damage;
        RefreshUI();
        FloatingStatTextSpawner.Instance?.SpawnText(
            $"-{damage:F0}", UnityEngine.Color.red, Position + Vector2.up * 0.3f);
        if (currentHP <= 0f) OnDeath();
    }

    // 열정 랜덤 배정 - 예산 2~12, 2단계는 2 소모
    private void AssignRandomPassions()
    {
        passions.Clear();
        int budget = Random.Range(passionBudgetMin, passionBudgetMax + 1);

        // Fisher-Yates shuffle
        var shuffled = (StatType[])AllStatTypes.Clone();
        for (int i = shuffled.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }

        foreach (var stat in shuffled)
        {
            if (budget <= 0) break;

            if (budget >= 2 && Random.value < majorPassionChance)
            {
                passions[stat] = PassionLevel.Major;
                budget -= 2;
            }
            else
            {
                passions[stat] = PassionLevel.Minor;
                budget -= 1;
            }
        }
    }

    // 스탯 합산으로 크기를 계산한 뒤 transform에 반영
    public void RefreshSizeFromStats()
    {
        stats.Size = Mathf.Lerp(statSizeMin, statSizeMax,
            Mathf.Clamp01(stats.GetTotalStats() / statSizeRefMax));
        ApplySizeToTransform();
    }

    // 감정 보정 포함한 크기를 transform 스케일에 반영
    private void ApplySizeToTransform()
    {
        float s = GetEffectiveStats().Size;
        transform.localScale = Vector3.one * s;
    }

    // Speed에 비례해 linearDamping 설정 (Speed 낮을수록 빨리 멈춤)
    public void ApplySpeedToDamping()
    {
        float t = Mathf.Clamp01(stats.Speed / Mathf.Max(stats.SpeedMax, 1f));
        Rigidbody.linearDamping = t < 0.5f
            ? Mathf.Lerp(dampingAtLowSpeed, dampingAtMidSpeed,  t * 2f)
            : Mathf.Lerp(dampingAtMidSpeed,  dampingAtHighSpeed, (t - 0.5f) * 2f);
    }

    // 혼합색/흰색이 들어와도 활성 채널 중 하나를 뽑아 원색으로 반환
    private static ColorState ExtractPrimaryColor(ColorState color)
    {
        if (color.GetTier() == 1) return color; // 이미 원색

        var candidates = new System.Collections.Generic.List<ColorState>();
        if ((color.Channels & RGBFlags.R) != 0) candidates.Add(ColorState.Red);
        if ((color.Channels & RGBFlags.G) != 0) candidates.Add(ColorState.Green);
        if ((color.Channels & RGBFlags.B) != 0) candidates.Add(ColorState.Blue);

        return candidates[Random.Range(0, candidates.Count)];
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (!GamePhaseManager.Instance.IsExchangePhase) return;
        if (!col.gameObject.TryGetComponent<BilliardBall>(out var other)) return;

        PhaseCollisionCount++;

        // 낮은 ID 쪽에서만 처리해 중복 방지
        if (Id < other.Id)
        {
            if (ExchangeSystem.Instance == null)
            {
                Debug.LogError("[BilliardBall] ExchangeSystem이 씬에 없습니다!");
                return;
            }
            ExchangeSystem.Instance.HandleCollision(this, other, col.contacts[0].point);
        }
    }

    // 색 변경 - 교류 이후 혼합색으로 전환될 때 호출
    public void OnColorChanged(ColorState newColor)
    {
        ColorChangedThisPhase = true;
        Color                 = newColor;
        emotionModifier = EmotionModifierFactory.Create(IsEnemy ? EmotionColorType : newColor.GetColorType());
        attackBehavior  = AttackBehaviorFactory.CreateOnColorChange(newColor.GetColorType());
        spriteRenderer.color = IsEnemy ? GetEnemySpriteColor() : newColor.ToUnityColor();

        ApplySizeToTransform();

        // 흰색이 된 턴은 계속 이동, 다음 웨이브 시작 시 정지 (InitAllPhaseStats에서 처리)
        if (newColor.IsWhite())
            WaveManager.Instance.RegisterWhiteBall(this);
    }

    // 감정 보정이 적용된 실제 전투 스탯
    public BallStats GetEffectiveStats() => emotionModifier.Apply(stats);

    public void ExecuteAttack(System.Collections.Generic.List<BilliardBall> targets)
    {
        if (Color.IsWhite()) return;
        attackBehavior.Execute(this, targets);
    }

    // damage는 CombatCalculator.Calculate()를 거친 최종값 (miss면 0)
    public void TakeDamage(float damage)
    {
        if (damage <= 0f) return;
        float mitigated = Mathf.Max(0f, damage - GetEffectiveStats().Defense);
        currentHP -= mitigated;
        if (currentHP <= 0f) OnDeath();
    }

    public void HealHP(float amount)
    {
        if (amount <= 0f) return;
        currentHP = Mathf.Min(currentHP + amount, stats.MaxHP);
        RefreshUI();
        FloatingStatTextSpawner.Instance?.SpawnText(
            $"+{amount:F0}", UnityEngine.Color.green, Position + Vector2.up * 0.3f);
    }

    private void OnDeath()
    {
        GamePhaseManager.Instance.RemoveBall(this);
        WaveManager.Instance.OnBallDefeated(this);
        Destroy(gameObject);
    }

    // 흰색이 된 지 2웨이브 후 호출 - 원래색 + 무작위색 두 공으로 분열
    public void Replicate()
    {
        var factory = BallFactory.Instance;
        Vector2 pos = Position;

        BilliardBall ballA, ballB;
        if (IsEnemy)
        {
            ballA = factory.CreateEnemyWithStats(OriginalColor.GetColorType(), pos + Vector2.left  * replicateOffset, stats); // 원본 스탯
            ballB = factory.CreateEnemy(GetRandomNonWhiteColor(),              pos + Vector2.right * replicateOffset);        // 랜덤 스탯
        }
        else
        {
            ballA = factory.CreateWithStats(OriginalColor.GetColorType(), pos + Vector2.left  * replicateOffset, stats); // 원본 스탯
            ballB = factory.Create(GetRandomNonWhiteColor(),              pos + Vector2.right * replicateOffset);        // 랜덤 스탯
        }

        GamePhaseManager.Instance.AddBall(ballA);
        GamePhaseManager.Instance.AddBall(ballB);
        GamePhaseManager.Instance.RemoveBall(this);

        Destroy(gameObject);
    }


    // 스폰 후 감정 타입 강제 지정 (EnemySpawnManager에서 호출)
    public void SetEnemyEmotion(ColorType emotionColorType)
    {
        emotionModifier      = EmotionModifierFactory.Create(emotionColorType);
        spriteRenderer.color = GetEnemySpriteColor();
        ApplySizeToTransform();

        // 어둠 감정이면 "20살 N월의 기억" 이름으로 교체
        if (emotionColorType == ColorType.Gray   ||
            emotionColorType == ColorType.Black  ||
            emotionColorType == ColorType.DeepBlack)
        {
            MemoryName = BallMemoryNameGenerator.NextDark();
            MemoryAge  = 20;
        }
    }

    private static ColorType RandomEnemyEmotionType()
    {
        ColorType[] types = { ColorType.Gray, ColorType.Black, ColorType.DeepBlack };
        return types[Random.Range(0, types.Length)];
    }

    private ColorType GetRandomNonWhiteColor()
    {
        ColorType[] colors = { ColorType.Red, ColorType.Green, ColorType.Blue,
                               ColorType.Yellow, ColorType.Cyan, ColorType.Magenta };
        return colors[Random.Range(0, colors.Length)];
    }

    void OnDestroy()
    {
        if (combatStatUI != null) Destroy(combatStatUI.gameObject);
    }

    public BallTooltipData GetTooltipData() => new BallTooltipData(
        Color.GetColorType(),
        emotionModifier.EmotionName,
        emotionModifier.Description,
        attackBehavior.Type,
        stats,
        GetEffectiveStats(),
        currentHP,
        Color.IsWhite(),
        MemoryName,
        BuildPassionSummary(),
        new System.Collections.Generic.Dictionary<StatType, PassionLevel>(passions)
    );

    private string BuildPassionSummary()
    {
        var parts = new System.Collections.Generic.List<string>();
        foreach (var stat in AllStatTypes)
        {
            var p = GetPassion(stat);
            if (p == PassionLevel.None) continue;
            string symbol = p == PassionLevel.Major ? "★★" : "★";
            parts.Add($"{StatTypeToKorean(stat)}{symbol}");
        }
        return parts.Count > 0 ? string.Join(" ", parts) : "없음";
    }

    // 적 공 스프라이트 색 - 감정 타입에 따라 구분
    private UnityEngine.Color GetEnemySpriteColor() => EmotionColorType switch
    {
        ColorType.Gray      => new UnityEngine.Color(0.45f, 0.45f, 0.45f), // 혐오 - 회색
        ColorType.DeepBlack => new UnityEngine.Color(0.12f, 0.0f,  0.20f), // 붕괴 - 진한 암자색
        _                   => UnityEngine.Color.black                      // 공포 - 검정
    };

    private static string StatTypeToKorean(StatType t) => t switch
    {
        StatType.Attack   => "공격",
        StatType.Defense  => "방어",
        StatType.HP       => "체력",
        StatType.Speed    => "속도",
        StatType.Evasion  => "회피",
        StatType.Accuracy => "명중",
        StatType.Critical => "치명",
        StatType.Heal     => "힐",
        _                 => "?"
    };
}
