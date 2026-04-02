using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    public static EnemySpawnManager Instance { get; private set; }

    [Header("웨이브 설정 (인덱스 0 = 1웨이브)")]
    [SerializeField] private List<WaveConfigSO> waveConfigs = new();

    [Header("설정된 웨이브를 초과했을 때")]
    [Tooltip("마지막 웨이브 설정 기준으로 웨이브마다 적을 이만큼 추가 (감정 순환)")]
    [SerializeField] private int overflowExtraEnemies = 1;

    [Header("스폰 영역")]
    [SerializeField] private PolygonBoundary boundary;

    [Header("겹침 방지")]
    [SerializeField] private float ballRadius  = 0.5f;
    [SerializeField] private float edgeMargin  = 0.3f;
    [SerializeField] private int   maxAttempts = 50;

    private static readonly EnemyEmotionType[] OverflowEmotionPool =
    {
        EnemyEmotionType.Gray, EnemyEmotionType.Black, EnemyEmotionType.DeepBlack
    };

    private static readonly ColorType[] PrimaryColorPool =
    {
        ColorType.Red, ColorType.Green, ColorType.Blue
    };

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        WaveManager.OnWaveStarted += SpawnWave;
    }

    void OnDestroy() => WaveManager.OnWaveStarted -= SpawnWave;

    public void SpawnWave(int wave)
    {
        if (boundary == null) boundary = FindAnyObjectByType<PolygonBoundary>();
        
        // 1. [수정] 설정된 웨이브 설정을 초과하면 즉시 리턴하여 스폰하지 않음
        if (waveConfigs == null || wave > waveConfigs.Count) 
        {
            Debug.Log($"[EnemySpawnManager] 웨이브 {wave}는 설정 범위를 벗어났으므로 적을 생성하지 않습니다.");
            return;
        }

        var config = GetWaveConfig(wave);
        if (config == null || !config.spawnEnemies) return;

        var enemies = GetEnemyList(wave, config);
        float spawnRadius = boundary.Inradius - ballRadius - edgeMargin;
        float minDist     = ballRadius * 2f + 0.1f;

        var usedPositions = GamePhaseManager.Instance.GetAllBallPositions();
        int ballCount     = usedPositions.Count; // 현재 살아있는 공 수

        foreach (var entry in enemies)
        {
            var     baseStats = entry.useFixedStats ? entry.stats : config.defaultStats;
            var     stats     = ScaleStats(baseStats, ballCount, config);
            Vector2 pos       = FindPosition(boundary.Center, spawnRadius, usedPositions, minDist);
            usedPositions.Add(pos);

            // 항상 어둠 색상으로 생성 — RandomPrimaryColor 사용 시 Color 필드가 원색으로 남아 색 혼합 버그 발생
            ColorType emotionColor = entry.emotionType == EnemyEmotionType.Random
                ? ToColorType(OverflowEmotionPool[Random.Range(0, OverflowEmotionPool.Length)])
                : ToColorType(entry.emotionType);

            var ball = BallFactory.Instance.CreateEnemyWithStats(emotionColor, pos, stats);
            ball.SetEnemyEmotion(emotionColor);

            GamePhaseManager.Instance.AddBall(ball);
        }
    }

    private WaveConfigSO GetWaveConfig(int wave)
    {
        // 2. [수정] Clamp를 제거하고 정확한 인덱스만 반환
        int idx = wave - 1;
        if (idx >= 0 && idx < waveConfigs.Count)
            return waveConfigs[idx];
        
        return null;
    }

    private List<EnemyEntry> GetEnemyList(int wave, WaveConfigSO config)
    {
        // 3. [수정] Overflow 로직을 완전히 제거
        // 설정 범위 내라면 config에 정의된 적 리스트만 반환합니다.
        return config.enemies;
    }

    // 기준 공 수 초과분에만 보너스 적용 (SO 원본 수정 방지를 위해 Clone 사용)
    private static BallStats ScaleStats(BallStats baseStats, int ballCount, WaveConfigSO config)
    {
        var   scaled    = baseStats.Clone();
        int   excess    = Mathf.Max(0, ballCount - config.bonusMinBallCount);
        float bonus     = excess * config.statBonusPerBall;
        if (bonus <= 0f) return scaled;

        scaled.Attack   += bonus;
        scaled.Defense  += bonus;
        scaled.MaxHP    += bonus;
        scaled.Speed    += bonus;
        scaled.Evasion  += bonus;
        scaled.Accuracy += bonus;
        scaled.Critical += bonus;
        scaled.Heal     += bonus;
        return scaled;
    }

    private static ColorType RandomPrimaryColor() =>
        PrimaryColorPool[Random.Range(0, PrimaryColorPool.Length)];

    private static ColorType ToColorType(EnemyEmotionType t) => t switch
    {
        EnemyEmotionType.Gray      => ColorType.Gray,
        EnemyEmotionType.Black     => ColorType.Black,
        EnemyEmotionType.DeepBlack => ColorType.DeepBlack,
        _                          => ColorType.Gray
    };

    private Vector2 FindPosition(Vector2 center, float spawnRadius,
                                  List<Vector2> used, float minDist)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 candidate = center + Random.insideUnitCircle * spawnRadius;
            if (IsFarEnough(candidate, used, minDist)) return candidate;
        }
        return center + Random.insideUnitCircle * spawnRadius;
    }

    private static bool IsFarEnough(Vector2 p, List<Vector2> others, float minDist)
    {
        foreach (var o in others)
            if (Vector2.Distance(p, o) < minDist) return false;
        return true;
    }
}
