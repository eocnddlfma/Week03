using UnityEngine;

[System.Serializable]
public class BallStats
{
    public const float DefaultMinValue = 1f;
    public const float DefaultMaxValue = 1f;

    [Header("크기 (성장 불가 - 감정으로만 변동)")]
    public float Size = 1f;

    [Header("스탯 값")]
    public float Attack   = 1f;
    public float Defense  = 1f;
    public float MaxHP    = 5f;
    public float Speed    = 1f;
    public float Evasion  = 1f;
    public float Accuracy = 1f;
    public float Critical = 1f;
    public float Heal     = 0f;

    // 스탯 최대치 - 모든 공 동일 고정값 (게이지 기준)
    public const float AttackMax   = 500f;
    public const float DefenseMax  = 500f;
    public const float MaxHPMax    = 50000f;
    public const float SpeedMax    = 520f;
    public const float EvasionMax  = 520f;
    public const float AccuracyMax = 520f;
    public const float CriticalMax = 520f;
    public const float HealMax     = 500f;

    public float Get(StatType type) => type switch
    {
        StatType.Attack   => Attack,
        StatType.Defense  => Defense,
        StatType.HP       => MaxHP,
        StatType.Speed    => Speed,
        StatType.Evasion  => Evasion,
        StatType.Accuracy => Accuracy,
        StatType.Critical => Critical,
        StatType.Heal     => Heal,
        _ => 0f
    };

    public float GetMax(StatType type) => type switch
    {
        StatType.Attack   => AttackMax,
        StatType.Defense  => DefenseMax,
        StatType.HP       => MaxHPMax,
        StatType.Speed    => SpeedMax,
        StatType.Evasion  => EvasionMax,
        StatType.Accuracy => AccuracyMax,
        StatType.Critical => CriticalMax,
        StatType.Heal     => HealMax,
        _ => 100f
    };

    public void Add(StatType type, float amount)
    {
        switch (type)
        {
            case StatType.Attack:   Attack   += amount; break;
            case StatType.Defense:  Defense  += amount; break;
            case StatType.HP:       MaxHP    += amount; break;
            case StatType.Speed:    Speed    += amount; break;
            case StatType.Evasion:  Evasion  += amount; break;
            case StatType.Accuracy: Accuracy += amount; break;
            case StatType.Critical: Critical += amount; break;
            case StatType.Heal:     Heal     += amount; break;
        }
    }

    public BallStats Clone() => new BallStats
    {
        Size     = Size,
        Attack   = Attack,
        Defense  = Defense,
        MaxHP    = MaxHP,
        Speed    = Speed,
        Evasion  = Evasion,
        Accuracy = Accuracy,
        Critical = Critical,
        Heal     = Heal,
    };

    public float GetTotalStats() =>
        Attack + Defense + MaxHP + Speed + Evasion + Accuracy + Critical + Heal;

    // 각 스탯을 minValue~maxValue 범위에서 랜덤 생성 (크기는 스탯 합산으로 결정)
    public void Randomize(float minValue, float maxValue)
    {
        int min = Mathf.RoundToInt(minValue);
        int max = Mathf.RoundToInt(maxValue) + 1;
        Attack   = Random.Range(min, max);
        Defense  = Random.Range(min, max);
        Speed    = Random.Range(min, max);
        Evasion  = Random.Range(min, max);
        Accuracy = Random.Range(min, max);
        Critical = Random.Range(min, max);
        Heal     = Random.Range(min, max);
        MaxHP    = 5f;
    }

    public void CopyFrom(BallStats other)
    {
        Size     = other.Size;
        Attack   = other.Attack;
        Defense  = other.Defense;
        MaxHP    = other.MaxHP;
        Speed    = other.Speed;
        Evasion  = other.Evasion;
        Accuracy = other.Accuracy;
        Critical = other.Critical;
        Heal     = other.Heal;
    }
}
