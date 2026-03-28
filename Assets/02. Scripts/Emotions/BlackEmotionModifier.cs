// 공포: 도망칠 수도, 맞설 수도 없는 압도감.
public class BlackEmotionModifier : IEmotionModifier
{
    public ColorType ColorType   => ColorType.Black;
    public string    EmotionName => "공포";
    public string    Description => "공격↑  속도↑  회피↑  방어↓";

    public BallStats Apply(BallStats baseStats)
    {
        var s = baseStats.Clone();
        s.Size    *= 1.15f;
        s.Attack  *= 1.3f;
        s.Speed   *= 1.3f;
        s.Evasion *= 1.2f;
        s.Defense *= 0.7f;
        return s;
    }
}
