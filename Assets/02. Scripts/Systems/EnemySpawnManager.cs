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

    [Header("스탯 스케일링")]
    [Tooltip("현재 공 1개당 적의 각 스탯에 더해지는 보너스")]
    [SerializeField] private float statBonusPerBall = 1f;

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
        if (waveConfigs.Count == 0) return;

        var config = GetWaveConfig(wave);
        if (config == null || !config.spawnEnemies) return;

        var   enemies     = GetEnemyList(wave, config);
        float spawnRadius = boundary.Inradius - ballRadius - edgeMargin;
        float minDist     = ballRadius * 2f + 0.1f;

        var usedPositions = GamePhaseManager.Instance.GetAllBallPositions();
        int ballCount     = usedPositions.Count; // 현재 살아있는 공 수

        foreach (var entry in enemies)
        {
            var     baseStats = entry.useFixedStats ? entry.stats : config.defaultStats;
            var     stats     = ScaleStats(baseStats, ballCount);
            Vector2 pos       = FindPosition(boundary.Center, spawnRadius, usedPositions, minDist);
            usedPositions.Add(pos);

            var ball = BallFactory.Instance.CreateEnemyWithStats(RandomPrimaryColor(), pos, stats);

            if (entry.emotionType != EnemyEmotionType.Random)
                ball.SetEnemyEmotion(ToColorType(entry.emotionType));

            GamePhaseManager.Instance.AddBall(ball);
        }
    }

    private WaveConfigSO GetWaveConfig(int wave)
    {
        int idx = Mathf.Clamp(wave - 1, 0, waveConfigs.Count - 1);
        return waveConfigs[idx];
    }

    private List<EnemyEntry> GetEnemyList(int wave, WaveConfigSO config)
    {
        int idx = wave - 1;
        if (idx < waveConfigs.Count) return config.enemies;

        // 설정된 웨이브를 초과하면 마지막 config에 추가 적을 붙임
        int overflow = idx - (waveConfigs.Count - 1);
        var list     = new List<EnemyEntry>(config.enemies);
        for (int i = 0; i < overflow * overflowExtraEnemies; i++)
            list.Add(new EnemyEntry
            {
                emotionType   = OverflowEmotionPool[i % OverflowEmotionPool.Length],
                useFixedStats = false,
            });
        return list;
    }

    // 현재 공 수에 비례해 각 스탯에 보너스 추가 (SO 원본 수정 방지를 위해 Clone 사용)
    private BallStats ScaleStats(BallStats baseStats, int ballCount)
    {
        var scaled = baseStats.Clone();
        float bonus = ballCount * statBonusPerBall;
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
