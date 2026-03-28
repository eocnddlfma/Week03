public class BallTooltipData
{
    public ColorType  ColorType        { get; }
    public string     EmotionName      { get; }
    public string     EmotionDesc      { get; }
    public AttackType AttackType       { get; }
    public BallStats  BaseStats        { get; }
    public BallStats  EffectiveStats   { get; }
    public float      CurrentHP        { get; }
    public bool       IsWhite          { get; }
    public string     MemoryName       { get; }
    public string     PassionSummary   { get; }
    public System.Collections.Generic.Dictionary<StatType, PassionLevel> Passions { get; }

    public BallTooltipData(
        ColorType  colorType,
        string     emotionName,
        string     emotionDesc,
        AttackType attackType,
        BallStats  baseStats,
        BallStats  effectiveStats,
        float      currentHP,
        bool       isWhite,
        string     memoryName    = "",
        string     passionSummary = "",
        System.Collections.Generic.Dictionary<StatType, PassionLevel> passions = null)
    {
        ColorType      = colorType;
        EmotionName    = emotionName;
        EmotionDesc    = emotionDesc;
        AttackType     = attackType;
        BaseStats      = baseStats;
        EffectiveStats = effectiveStats;
        CurrentHP      = currentHP;
        IsWhite        = isWhite;
        MemoryName     = memoryName;
        PassionSummary = passionSummary;
        Passions       = passions ?? new System.Collections.Generic.Dictionary<StatType, PassionLevel>();
    }

    public PassionLevel GetPassion(StatType stat) =>
        Passions.TryGetValue(stat, out var p) ? p : PassionLevel.None;
}
