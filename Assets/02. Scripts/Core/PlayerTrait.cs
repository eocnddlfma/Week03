public enum PlayerTrait
{
    None,
    Sniper,       // 스나이퍼
    Smasher,      // 스매셔
    Headshot,     // 헤드샷
    SquareBeam,   // 네모네모 빔
    BigBeautiful, // 거대하고 아름다운
    Smooth,       // 매끈매끈하다 매끈매끈한
    SolidBase,    // 충실한 기본기
}

public static class PlayerTraitInfo
{
    private static readonly PlayerTrait[] Pool =
    {
        PlayerTrait.Sniper, PlayerTrait.Smasher, PlayerTrait.Headshot,
        PlayerTrait.SquareBeam, PlayerTrait.BigBeautiful,
        PlayerTrait.Smooth, PlayerTrait.SolidBase,
    };

    public static PlayerTrait Roll() => Pool[UnityEngine.Random.Range(0, Pool.Length)];

    /// 0~2개 중복 없이 랜덤 뽑기
    public static PlayerTrait[] RollMultiple()
    {
        int count = UnityEngine.Random.Range(0, 3); // 0, 1, 2
        if (count == 0) return System.Array.Empty<PlayerTrait>();

        // Fisher-Yates 셔플
        var list = new System.Collections.Generic.List<PlayerTrait>(Pool);
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j  = UnityEngine.Random.Range(0, i + 1);
            var tmp = list[i]; list[i] = list[j]; list[j] = tmp;
        }
        var result = new PlayerTrait[count];
        for (int i = 0; i < count; i++) result[i] = list[i];
        return result;
    }

    public static string GetName(PlayerTrait t) => t switch
    {
        PlayerTrait.Sniper       => "스나이퍼",
        PlayerTrait.Smasher      => "스매셔",
        PlayerTrait.Headshot     => "헤드샷",
        PlayerTrait.SquareBeam   => "네모네모 빔",
        PlayerTrait.BigBeautiful => "거대하고 아름다운",
        PlayerTrait.Smooth       => "매끈매끈하다 매끈매끈한",
        PlayerTrait.SolidBase    => "충실한 기본기",
        _                        => "없음",
    };

    public static string GetDesc(PlayerTrait t) => t switch
    {
        PlayerTrait.Sniper       => "명중 +30%  딜 +20%",
        PlayerTrait.Smasher      => "명중 -30%  속도 +50%",
        PlayerTrait.Headshot     => "치명타 확률 +50%",
        PlayerTrait.SquareBeam   => "모든 기억들이 사각형으로 변함",
        PlayerTrait.BigBeautiful => "공의 크기가 1.2배로 증가",
        PlayerTrait.Smooth       => "마찰력 감소 — 공이 더 오래 구름",
        PlayerTrait.SolidBase    => "생성 시 초기 스탯 범위 3배 증가",
        _                        => "",
    };
}
