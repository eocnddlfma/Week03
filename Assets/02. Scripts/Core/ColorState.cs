using UnityEngine;

[System.Flags]
public enum RGBFlags
{
    None = 0,
    R    = 1,
    G    = 2,
    B    = 4
}

public enum ColorType
{
    Any = -1,   // 대사 조건 전용 - "모든 감정에 해당"
    Red, Green, Blue,
    Yellow, Cyan, Magenta,
    White,
    Gray,       // 적 공 - 혐오
    Black,      // 적 공 - 공포
    DeepBlack   // 적 공 - 붕괴
}

public enum StatType
{
    Attack, Defense, HP, Speed,
    Evasion, Accuracy, Critical, Heal
}

public enum AttackType
{
    Melee, Ranged, Charge, JumpArea, Orbit, Idle
}

public enum GamePhase
{
    Billiard, Exchange, Combat
}

public enum PassionLevel
{
    None  = 0,
    Minor = 1, // 스탯 증가 2배
    Major = 2  // 스탯 증가 5배, 예산 2 소모
}

public struct ColorState
{
    public RGBFlags Channels;

    public static readonly ColorState Red     = new ColorState(RGBFlags.R);
    public static readonly ColorState Green   = new ColorState(RGBFlags.G);
    public static readonly ColorState Blue    = new ColorState(RGBFlags.B);
    public static readonly ColorState Yellow  = new ColorState(RGBFlags.R | RGBFlags.G);
    public static readonly ColorState Cyan    = new ColorState(RGBFlags.G | RGBFlags.B);
    public static readonly ColorState Magenta = new ColorState(RGBFlags.R | RGBFlags.B);
    public static readonly ColorState White   = new ColorState(RGBFlags.R | RGBFlags.G | RGBFlags.B);

    public ColorState(RGBFlags channels) => Channels = channels;

    public ColorType GetColorType() => Channels switch
    {
        RGBFlags.R                          => ColorType.Red,
        RGBFlags.G                          => ColorType.Green,
        RGBFlags.B                          => ColorType.Blue,
        RGBFlags.R | RGBFlags.G             => ColorType.Yellow,
        RGBFlags.G | RGBFlags.B             => ColorType.Cyan,
        RGBFlags.R | RGBFlags.B             => ColorType.Magenta,
        RGBFlags.R | RGBFlags.G | RGBFlags.B => ColorType.White,
        _ => throw new System.Exception($"알 수 없는 색상 채널: {Channels}")
    };

    public ColorState Mix(ColorState other)      => new ColorState(Channels | other.Channels);
    public bool       IsWhite()                  => Channels == (RGBFlags.R | RGBFlags.G | RGBFlags.B);
    public bool       IsSameAs(ColorState other) => Channels == other.Channels;

    // 활성 채널 수 = 색상 티어 (원색:1, 혼합색:2, 흰색:3)
    public int GetTier()
    {
        int count = 0;
        if ((Channels & RGBFlags.R) != 0) count++;
        if ((Channels & RGBFlags.G) != 0) count++;
        if ((Channels & RGBFlags.B) != 0) count++;
        return count;
    }

    public bool IsSameTierAs(ColorState other) => GetTier() == other.GetTier();

    public StatType GetExchangeStat() => GetColorType() switch
    {
        ColorType.Red     => StatType.Attack,
        ColorType.Blue    => StatType.Defense,
        ColorType.Green   => StatType.HP,
        ColorType.Yellow  => StatType.Speed,
        ColorType.Cyan    => StatType.Critical,
        ColorType.Magenta => StatType.Accuracy,
        ColorType.White   => GetWhiteRandomStat(),
        _ => throw new System.Exception("알 수 없는 색상")
    };

    private static StatType GetWhiteRandomStat()
    {
        StatType[] whiteStats = { StatType.Evasion, StatType.Heal };
        return whiteStats[Random.Range(0, whiteStats.Length)];
    }

    public static ColorState From(ColorType type) => type switch
    {
        ColorType.Red     => Red,
        ColorType.Green   => Green,
        ColorType.Blue    => Blue,
        ColorType.Yellow  => Yellow,
        ColorType.Cyan    => Cyan,
        ColorType.Magenta => Magenta,
        ColorType.White   => White,
        _ => throw new System.Exception($"알 수 없는 색상 타입: {type}")
    };

    public Color ToUnityColor() => GetColorType() switch
    {
        ColorType.Red     => Color.red,
        ColorType.Green   => Color.green,
        ColorType.Blue    => Color.blue,
        ColorType.Yellow  => Color.yellow,
        ColorType.Cyan    => Color.cyan,
        ColorType.Magenta => Color.magenta,
        ColorType.White   => Color.white,
        ColorType.Gray      => new Color(0.4f, 0.4f, 0.4f),
        ColorType.Black     => Color.black,
        ColorType.DeepBlack => new Color(0.05f, 0f, 0.1f),
        _ => Color.black
    };
}
