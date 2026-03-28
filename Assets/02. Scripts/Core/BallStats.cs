using UnityEngine;

[System.Serializable]
public class BallStats
{
    public const float DefaultMinValue = 5f;
    public const float DefaultMaxValue = 30f;

    [Header("크기 (성장 불가 - 감정으로만 변동)")]
    public float Size = 1f;

    [Header("스탯 값")]
    public float Attack   = 10f;
    public float Defense  = 10f;
    public float MaxHP    = 10f;
    public float Speed    = 10f;
    public float Evasion  = 10f;
    public float Accuracy = 10f;
    public float Critical = 10f;
    public float Heal     = 10f;

    [Header("스탯 최대치 (게이지 기준)")]
    public float AttackMax   = 100f;
    public float DefenseMax  = 100f;
    public float MaxHPMax    = 1000f;
    public float SpeedMax    = 100f;
    public float EvasionMax  = 500f;
    public float AccuracyMax = 500f;
    public float CriticalMax = 100f;
    public float HealMax     = 100f;

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
        Attack   = Attack,   AttackMax   = AttackMax,
        Defense  = Defense,  DefenseMax  = DefenseMax,
        MaxHP    = MaxHP,    MaxHPMax    = MaxHPMax,
        Speed    = Speed,    SpeedMax    = SpeedMax,
        Evasion  = Evasion,  EvasionMax  = EvasionMax,
        Accuracy = Accuracy, AccuracyMax = AccuracyMax,
        Critical = Critical, CriticalMax = CriticalMax,
        Heal     = Heal,     HealMax     = HealMax,
    };

    public float GetTotalStats() =>
        Attack + Defense + MaxHP + Speed + Evasion + Accuracy + Critical + Heal;

    // 각 스탯을 minValue~maxValue 범위에서 랜덤 생성 (크기는 스탯 합산으로 결정)
    public void Randomize(float minValue, float maxValue)
    {
        Attack   = Random.Range(Mathf.RoundToInt(minValue), Mathf.RoundToInt(maxValue) + 1);
        Defense  = Random.Range(Mathf.RoundToInt(minValue), Mathf.RoundToInt(maxValue) + 1);
        MaxHP    = Random.Range(Mathf.RoundToInt(minValue), Mathf.RoundToInt(maxValue) + 1);
        Speed    = Random.Range(Mathf.RoundToInt(minValue), Mathf.RoundToInt(maxValue) + 1);
        Evasion  = Random.Range(Mathf.RoundToInt(minValue), Mathf.RoundToInt(maxValue) + 1);
        Accuracy = Random.Range(Mathf.RoundToInt(minValue), Mathf.RoundToInt(maxValue) + 1);
        Critical = Random.Range(Mathf.RoundToInt(minValue), Mathf.RoundToInt(maxValue) + 1);
        Heal     = Random.Range(Mathf.RoundToInt(minValue), Mathf.RoundToInt(maxValue) + 1);
    }

    public void CopyFrom(BallStats other)
    {
        Size     = other.Size;
        Attack   = other.Attack;   AttackMax   = other.AttackMax;
        Defense  = other.Defense;  DefenseMax  = other.DefenseMax;
        MaxHP    = other.MaxHP;    MaxHPMax    = other.MaxHPMax;
        Speed    = other.Speed;    SpeedMax    = other.SpeedMax;
        Evasion  = other.Evasion;  EvasionMax  = other.EvasionMax;
        Accuracy = other.Accuracy; AccuracyMax = other.AccuracyMax;
        Critical = other.Critical; CriticalMax = other.CriticalMax;
        Heal     = other.Heal;     HealMax     = other.HealMax;
    }
}
