using System.Collections.Generic;
using UnityEngine;

public class ExchangeSystem : MonoBehaviour
{
    public static ExchangeSystem Instance { get; private set; }

    [Header("스탯 교환 설정")]
    [SerializeField] private float statGrowthDivisor = 100f; // 스탯 증가량 = 현재값 / divisor
    [SerializeField] private float minStatGrowth     = 1f;   // 최소 증가량

    // 연속 충돌 콤보 배수 임계값
    private static readonly int[] ComboThresholds = { 5, 10 };

    private readonly HashSet<(int, int)>  processedThisFrame = new HashSet<(int, int)>();
    private readonly Dictionary<int, int> ballComboTier      = new Dictionary<int, int>(); // 공 ID → 현재 달성 티어

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void LateUpdate() => processedThisFrame.Clear();

    // 큐 타격 시 호출 - 콤보 티어 초기화
    public void ResetPhase() => ballComboTier.Clear();

    public void HandleCollision(BilliardBall a, BilliardBall b, Vector2 contactPoint)
    {
        var key = (Mathf.Min(a.Id, b.Id), Mathf.Max(a.Id, b.Id));
        if (!processedThisFrame.Add(key)) return;

        // 각 공의 콤보 티어 갱신 및 알림 (각자 위치에 표시)
        CheckAndAnnounceCombo(a, a.Position);
        CheckAndAnnounceCombo(b, b.Position);

        if (a.IsEnemy != b.IsEnemy)
        {
            AttackEachOther(a, b);

            // 아군은 자신의 색상에 해당하는 스탯 획득
            var ally = a.IsEnemy ? b : a;
            GiveStat(ally, ally, ally.Position, GetComboMultiplier(ally.PhaseCollisionCount));
        }
        else
        {
            HealEachOther(a, b);

            // 아군끼리 - 스탯 교환 (콤보 배수 적용)
            if (!a.IsEnemy && !b.IsEnemy)
            {
                GiveStat(a, b, contactPoint, GetComboMultiplier(b.PhaseCollisionCount));
                GiveStat(b, a, contactPoint, GetComboMultiplier(a.PhaseCollisionCount));
            }
        }

    }

    // ── 콤보 ────────────────────────────────────────────────────────────
    // 5회↑ : 2배 / 10회↑ : 3배
    private static int GetComboTier(int collisionCount)
    {
        if (collisionCount >= 10) return 3;
        if (collisionCount >= 5)  return 2;
        return 1;
    }

    private static float GetComboMultiplier(int collisionCount) => GetComboTier(collisionCount);

    private void CheckAndAnnounceCombo(BilliardBall ball, Vector2 pos)
    {
        if (ball.IsEnemy) return;

        int newTier = GetComboTier(ball.PhaseCollisionCount);
        ballComboTier.TryGetValue(ball.Id, out int oldTier);

        if (newTier <= oldTier) return;

        ballComboTier[ball.Id] = newTier;
        if (newTier > 1)
        {
            int threshold = ComboThresholds[newTier - 2];
            FloatingStatTextSpawner.Instance?.SpawnText(
                $"연속 충돌 {threshold}회!\n스탯 획득량 {newTier}배 적용!", Color.yellow, pos);
            CollisionLogger.Log(CollisionLogType.Combo,
                $"{ball.MemoryName} {threshold}회 돌파 → ×{newTier}!", Color.yellow);
        }
    }

    // ── 전투 ────────────────────────────────────────────────────────────
    private void AttackEachOther(BilliardBall a, BilliardBall b)
    {
        float aSpeed    = a.GetEffectiveStats().Speed;
        float bSpeed    = b.GetEffectiveStats().Speed;
        bool  aIsFaster = aSpeed >= bSpeed;

        BilliardBall first  = aIsFaster ? a : b;
        BilliardBall second = aIsFaster ? b : a;
        float fasterSpeed   = aIsFaster ? aSpeed : bSpeed;
        float slowerSpeed   = aIsFaster ? bSpeed : aSpeed;

        int hits = Mathf.Max(1, Mathf.FloorToInt(fasterSpeed / Mathf.Max(slowerSpeed, 0.001f)));

        float hpBefore_first  = first.CurrentHP;
        float hpBefore_second = second.CurrentHP;

        var firstStats  = first.GetEffectiveStats();
        var secondStats = second.GetEffectiveStats();

        bool anyEvadeA = false, anyCritA = false;
        for (int i = 0; i < hits; i++)
        {
            if (second.CurrentHP <= 0f) break;
            float dmg = CombatCalculator.Calculate(firstStats, secondStats, out bool ev, out bool cr);
            anyEvadeA |= ev; anyCritA |= cr;
            if (dmg > 0f) second.TakeShieldedDamage(dmg);
        }

        float counterBase = second.CurrentHP <= 0f ? secondStats.Attack * 0.5f : secondStats.Attack;
        float counterDmg  = CombatCalculator.Calculate(secondStats, firstStats, out bool evadeB, out bool critB);
        first.TakeShieldedDamage(counterDmg > 0f ? counterDmg : counterBase * 0.5f);

        float dmgFirst  = hpBefore_first  - first.CurrentHP;
        float dmgSecond = hpBefore_second - second.CurrentHP;

        string tagA = anyCritA ? " <크리>" : anyEvadeA ? " <회피>" : "";
        string tagB = critB    ? " <크리>" : evadeB    ? " <회피>" : "";
        string logA = dmgSecond > 0f ? $"{second.MemoryName} -{dmgSecond:F0}{tagA}" : $"{second.MemoryName} 회피";
        string logB = dmgFirst  > 0f ? $"{first.MemoryName} -{dmgFirst:F0}{tagB}"   : $"{first.MemoryName} 회피";
        CollisionLogger.Log(CollisionLogType.Combat, $"{logA}  /  {logB}", Color.red);
    }

    private void HealEachOther(BilliardBall a, BilliardBall b)
    {
        ApplyHeal(a);
        ApplyHeal(b);
    }

    private static void ApplyHeal(BilliardBall ball)
    {
        if (ball.Stats.Heal <= 0f) return;

        bool isCrit = Random.value * 100f < ball.Stats.Critical;
        float amount = isCrit ? ball.Stats.Heal * 2f : ball.Stats.Heal;
        ball.HealHP(amount);

        string tag = isCrit ? " <크리>" : "";
        CollisionLogger.Log(CollisionLogType.Heal,
            $"{ball.MemoryName} HP +{amount:F0}{tag}", Color.cyan);
    }

    // ── 스탯 교환 ────────────────────────────────────────────────────────
    private void GiveStat(BilliardBall giver, BilliardBall receiver, Vector2 textPos,
                          float comboMultiplier = 1f)
    {
        if (!receiver.CanReceiveExchange) return;

        StatType statType      = giver.Color.GetExchangeStat();
        float    current       = giver.Stats.Get(statType);
        float    baseAmount    = Mathf.Max(minStatGrowth, current / statGrowthDivisor);
        float    passionMult   = receiver.GetPassionMultiplier(statType);
        float    amount        = baseAmount * passionMult * comboMultiplier;

        receiver.Stats.Add(statType, amount);
        if (statType == StatType.Speed) receiver.ApplySpeedToDamping();
        receiver.RefreshSizeFromStats();

        FloatingStatTextSpawner.Instance.Spawn(statType, amount, giver.Color.ToUnityColor(), textPos);

        string comboStr = comboMultiplier > 1f ? $" ×{comboMultiplier:F0}" : "";
        CollisionLogger.Log(CollisionLogType.StatExchange,
            $"{receiver.MemoryName} {StatTypeToKorean(statType)} +{amount:F1}{comboStr}",
            giver.Color.ToUnityColor());
    }

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
