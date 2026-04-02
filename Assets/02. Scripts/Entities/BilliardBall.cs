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
    public ColorState? PendingColor          { get; private set; }                  // 대사 이후 적용될 예약 색상

    public Rigidbody2D Rigidbody { get; private set; }

    private SpriteRenderer  spriteRenderer;
    private IEmotionModifier emotionModifier;

    [Header("크기 설정")]
    [SerializeField] private float statSizeMin = 1.0f;  // 스폰 시 기본 크기
    [SerializeField] private float statSizeMax = 10.0f; // 최대 크기
    [SerializeField] private AnimationCurve sizeGrowthCurve; // 크기 증가 곡선

    // 스탯 최댓값 총합 (BallStats 상수 기준, 자동 계산)
    private static readonly float StatTotalMax =
        BallStats.AttackMax + BallStats.DefenseMax + BallStats.MaxHPMax +
        BallStats.SpeedMax  + BallStats.EvasionMax  + BallStats.AccuracyMax +
        BallStats.CriticalMax + BallStats.HealMax;

    [Header("물리 설정")]
    private const float stopThreshold     = 0.3f;  // 이 속도 이하면 즉시 정지
    private const float isMovingThreshold = 0.01f; // IsMoving 판정 기준
    private const float dampingAtLowSpeed  = 1.0f;  // Speed 최저 시 linearDamping
    private const float dampingAtMidSpeed  = 0.6f;  // Speed 중간 시 linearDamping
    private const float dampingAtHighSpeed = 0.2f;  // Speed 최고 시 linearDamping
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

    public bool  CanReceiveExchange => true;
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
        spriteRenderer = GetComponent<SpriteRenderer>(); // <-- 여기서 초기화됩니다.
        Rigidbody.gravityScale = 0f;

        // 완전 탄성 반사 (마찰 없음) - 인스펙터에서 별도 설정이 없을 때만 적용
        var col = GetComponent<CircleCollider2D>();
        if (col.sharedMaterial == null)
            col.sharedMaterial = new PhysicsMaterial2D("BilliardBall") { bounciness = 1f, friction = 0f };
    }

    // 버텍스 컬러 + 머테리얼 색상 동시 적용 — 셰이더 방식에 무관하게 색상 반영
    private void SetSpriteColor(UnityEngine.Color c)
    {
        spriteRenderer.color = c;
        var mat = spriteRenderer.material;
        if (mat.HasProperty("_OutlineColor"))
            mat.SetColor("_OutlineColor", c);
    }

    public void SetMaterial(Material newMaterial)
    {
        if (spriteRenderer == null) return;
        UnityEngine.Color saved = spriteRenderer.color;
        spriteRenderer.material = newMaterial;
        SetSpriteColor(saved);
    }

    void FixedUpdate()
    {
        // 다른 콜라이더와 접촉 중이면 물리엔진에 맡김 (벽에 붙어 멈추는 현상 방지)
        if (Rigidbody.linearVelocity.magnitude < stopThreshold)
            Rigidbody.linearVelocity = Vector2.zero;
    }

    public void Initialize(ColorState colorState)
    {
         ColorType myEmotion = IsEnemy ? RandomEnemyEmotionType() : colorState.GetColorType();
        emotionModifier = EmotionModifierFactory.Create(myEmotion);
        
        Color         = colorState;
        OriginalColor = ExtractPrimaryColor(colorState);
        currentHP     = stats.MaxHP;
        SetSpriteColor(IsEnemy ? GetEnemySpriteColor() : colorState.ToUnityColor());

        if (PlayerData.Instance?.HasTrait(PlayerTrait.SquareBeam) == true)
        {
            spriteRenderer.sprite = GetSquareSprite();

            // CircleCollider2D → BoxCollider2D 교체
            var circle = GetComponent<CircleCollider2D>();
            var mat    = circle.sharedMaterial;
            circle.enabled = false;

            var box        = gameObject.AddComponent<BoxCollider2D>();
            box.size           = Vector2.one;   // 스프라이트 1×1 유닛에 맞춤
            box.sharedMaterial = mat;

            // 회전 저항 (사각형이 계속 회전하면 어색함)
            Rigidbody.angularDamping = 1f;
        }
        SetMemoryNameByEmotion(myEmotion); // 아래에 새로 만들 함수 호출
        if (MemoryName.Contains("살"))
        {
            MemoryAge = int.Parse(MemoryName.Split('살')[0]);
        }

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
    private void SetMemoryNameByEmotion(ColorType myColor)
    {
        var db = DialogueSystem.Instance?.Database;
        
        // 어둠의 감정 (Gray, Black, DeepBlack) 처리
        if (myColor == ColorType.Gray || myColor == ColorType.Black || myColor == ColorType.DeepBlack)
        {
            MemoryName = BallMemoryNameGenerator.NextDark();
            return;
        }

        // 일반 감정 DB에서 고정 나이 찾기
        if (db != null)
        {
            var group = db.groups.Find(g => g.emotion == myColor);
            if (group != null && group.fixedAge != -1)
            {
                MemoryName = BallMemoryNameGenerator.GetNameByAge(group.fixedAge);
                return;
            }
        }

        // DB에 없거나 고정 나이가 없는 경우 (Any 등) 기존처럼 랜덤
        MemoryName = BallMemoryNameGenerator.Next();
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
        RefreshUI(); // 변경된 방어력으로 UI 갱신
        if (damage > 0f) TakeRawDamage(damage);
    }

    private void RefreshUI() =>
        combatStatUI?.Refresh(PhaseAttack, PhaseDefense, currentHP, Stats.MaxHP);

    // 페이즈 스탯 기반 데미지 (방어 중복 적용 없음)
    public void TakeRawDamage(float damage)
    {
        if (damage <= 0f) return;
        currentHP -= damage;
        RefreshUI(); // 변경된 HP로 UI 갱신
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
        float t = Mathf.Clamp01(stats.GetTotalStats() / StatTotalMax);
        
        // sizeGrowthCurve를 사용하여 t 값을 변환
        float curvedT = sizeGrowthCurve.Evaluate(t); 

        stats.Size = Mathf.Lerp(statSizeMin, statSizeMax, curvedT); // 변환된 t 값 사용

        // 거대하고 아름다운: 계산된 크기에 1.2배 배율
        if (!IsEnemy && PlayerData.Instance?.HasTrait(PlayerTrait.BigBeautiful) == true)
            stats.Size *= 1.2f;
        ApplySizeToTransform();
    }

    // stats.Size를 그대로 반영 (감정 배율 미적용 - 스폰 시 크기 1 보장)
    private void ApplySizeToTransform()
    {
        transform.localScale = Vector3.one * stats.Size;
    }

    // Speed에 비례해 linearDamping 설정 (Speed 낮을수록 빨리 멈춤)
    public void ApplySpeedToDamping() => Rigidbody.linearDamping = GetBaseDamping();

    // FrictionRampSystem에서 참조
    public float GetBaseDamping()
    {
        float t    = Mathf.Clamp01(stats.Speed / Mathf.Max(BallStats.SpeedMax, 1f));
        float base_ = t < 0.5f
            ? Mathf.Lerp(dampingAtLowSpeed, dampingAtMidSpeed,  t * 2f)
            : Mathf.Lerp(dampingAtMidSpeed,  dampingAtHighSpeed, (t - 0.5f) * 2f);
        // Smooth 특성: 기본 damping 60% 감소
        if (PlayerData.Instance?.HasTrait(PlayerTrait.Smooth) == true) base_ *= 0.4f;
        return base_;
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

    // 색 예약 - Exchange 페이즈 중 충돌 시 호출, 실제 적용은 대사 이후
    public void SetPendingColor(ColorState newColor)
    {
        PendingColor          = newColor;
        ColorChangedThisPhase = true; // 이번 페이즈 추가 변경 차단
    }

    // 예약된 색상 적용 - 대사 종료 후 GamePhaseManager에서 호출
    public void ApplyPendingColor()
    {
        if (PendingColor == null) return;
        OnColorChanged(PendingColor.Value);
        PendingColor = null;
    }

    // 색 변경 - 교류 이후 혼합색으로 전환될 때 호출
    public void OnColorChanged(ColorState newColor)
    {
        ColorChangedThisPhase = true;
        Color                 = newColor;
        emotionModifier = EmotionModifierFactory.Create(IsEnemy ? EmotionColorType : newColor.GetColorType());
        SetSpriteColor(IsEnemy ? GetEnemySpriteColor() : newColor.ToUnityColor());

        ApplySizeToTransform();

    }

    // 감정 보정이 적용된 실제 전투 스탯
    public BallStats GetEffectiveStats() => emotionModifier.Apply(stats);

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
        float actual = Mathf.Min(currentHP + amount, stats.MaxHP) - currentHP;
        if (actual <= 0f) return; // 이미 최대체력 — 힐 없음
        currentHP += actual;
        RefreshUI();
        FloatingStatTextSpawner.Instance?.SpawnText(
            $"+{actual:F0}", UnityEngine.Color.green, Position + Vector2.up * 0.3f);
    }

    public static event System.Action OnLastDeepBlackDefeated;

    private void OnDeath()
    {
        CollisionLogger.Log(CollisionLogType.Death,
            $"{MemoryName} 사망", UnityEngine.Color.red);

        // 1. [추가] 적이 죽었음을 즉시 알림 (음악 변경 트리거)
        if (IsEnemy) 
        {
            EnemyPresenceManager.Instance?.OnBallRemoved(this);
        }

        GamePhaseManager.Instance.RemoveBall(this);

        // DeepBlack 관련 로직 (FindObjectsByType은 죽어가는 나 자신을 포함할 수 있음)
        if (IsEnemy && EmotionColorType == ColorType.DeepBlack)
        {
            bool anyLeft = false;
            foreach (var b in FindObjectsByType<BilliardBall>(FindObjectsSortMode.None))
            {
                if (b != this && b.IsEnemy && b.EmotionColorType == ColorType.DeepBlack)
                { anyLeft = true; break; }
            }
            if (!anyLeft) OnLastDeepBlackDefeated?.Invoke();
        }

        Destroy(gameObject);
    }



    // 스폰 후 감정 타입 강제 지정 (EnemySpawnManager에서 호출)
    public void SetEnemyEmotion(ColorType emotionColorType)
    {
        emotionModifier      = EmotionModifierFactory.Create(emotionColorType);
        SetSpriteColor(GetEnemySpriteColor());
        ApplySizeToTransform();

        SetMemoryNameByEmotion(emotionColorType);
        if (MemoryName.Contains("살"))
        {
            MemoryAge = int.Parse(MemoryName.Split('살')[0]);
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

    // ── SquareBeam 특성용 정사각형 스프라이트 ─────────────────────
    private static Sprite _squareSprite;
    private static Sprite GetSquareSprite()
    {
        if (_squareSprite != null) return _squareSprite;
        var tex     = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        var pixels  = new Color32[32 * 32];
        var white   = new Color32(255, 255, 255, 255);
        for (int i = 0; i < pixels.Length; i++) pixels[i] = white;
        tex.SetPixels32(pixels);
        tex.Apply();
        _squareSprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
        return _squareSprite;
    }
}