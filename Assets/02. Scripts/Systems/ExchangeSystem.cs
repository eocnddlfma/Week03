using UnityEngine;

public class ExchangeSystem : MonoBehaviour
{
    public static ExchangeSystem Instance { get; private set; }

    [Header("스탯 교환 설정")]
    [SerializeField] private float statGrowthDivisor = 100f; // 스탯 증가량 = 현재값 / divisor
    [SerializeField] private float minStatGrowth     = 1f;   // 최소 증가량

    private readonly System.Collections.Generic.HashSet<(int, int)> processedThisFrame
        = new System.Collections.Generic.HashSet<(int, int)>();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void LateUpdate() => processedThisFrame.Clear();

    public void HandleCollision(BilliardBall a, BilliardBall b, Vector2 contactPoint)
    {
        var key = (Mathf.Min(a.Id, b.Id), Mathf.Max(a.Id, b.Id));
        if (!processedThisFrame.Add(key)) return;

        if (a.IsEnemy != b.IsEnemy)
        {
            // 적군 vs 아군: 서로 공격
            AttackEachOther(a, b);
        }
        else
        {
            // 같은 팀: 힐 + 스탯 교환
            HealEachOther(a, b);
            GiveStat(giver: a, receiver: b, textPos: b.Position + Vector2.up * 0.5f);
            GiveStat(giver: b, receiver: a, textPos: a.Position + Vector2.up * 0.5f);
        }

        MixColors(a, b);
    }

    // 속도가 빠른 쪽이 선공. 선공에 상대가 죽으면 반격은 절반.
    private void AttackEachOther(BilliardBall a, BilliardBall b)
    {
        bool aIsFaster = a.GetEffectiveStats().Speed >= b.GetEffectiveStats().Speed;
        BilliardBall first  = aIsFaster ? a : b;
        BilliardBall second = aIsFaster ? b : a;

        // 방어 = 추가 체력(보호막) - 공격력 그대로 들어오고 보호막이 먼저 흡수
        second.TakeShieldedDamage(first.PhaseAttack);

        float damageToFirst = second.CurrentHP <= 0f
            ? second.PhaseAttack * 0.5f
            : second.PhaseAttack;

        first.TakeShieldedDamage(damageToFirst);
    }

    // 같은 팀 충돌: 각자 Heal만큼 HP 회복
    private void HealEachOther(BilliardBall a, BilliardBall b)
    {
        a.HealHP(a.Stats.Heal);
        b.HealHP(b.Stats.Heal);
    }

    private void GiveStat(BilliardBall giver, BilliardBall receiver, Vector2 textPos)
    {
        if (!receiver.CanReceiveExchange) return;

        StatType statType   = giver.Color.GetExchangeStat();
        float    current    = giver.Stats.Get(statType);
        float    baseAmount = Mathf.Max(minStatGrowth, current / statGrowthDivisor);
        float    amount     = baseAmount * receiver.GetPassionMultiplier(statType);

        receiver.Stats.Add(statType, amount);
        if (statType == StatType.Speed) receiver.ApplySpeedToDamping();
        receiver.RefreshSizeFromStats();

        FloatingStatTextSpawner.Instance.Spawn(statType, amount, giver.Color.ToUnityColor(), textPos);
    }

    private void MixColors(BilliardBall a, BilliardBall b)
    {
        if (a.IsEnemy != b.IsEnemy)                          return; // 적-아군 간 색 혼합 없음
        if (a.Color.IsWhite() || b.Color.IsWhite())          return; // 흰색 관여 시 색 혼합 없음
        if (a.ColorChangedThisPhase || b.ColorChangedThisPhase) return; // 페이즈당 1회 제한
        if (a.Color.IsSameAs(b.Color))                       return; // 동색: 스탯 성장만

        ColorState mixed = a.Color.Mix(b.Color);
        a.OnColorChanged(mixed);
        b.OnColorChanged(mixed);
    }
}
