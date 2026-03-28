// 불안: 공격-, 회피++, 속도-
public class MagentaEmotionModifier : IEmotionModifier
{
    public ColorType ColorType   => ColorType.Magenta;
    public string    EmotionName => "불안";
    public string    Description => "회피↑↑  공격↓  속도↓";

    public BallStats Apply(BallStats baseStats)
    {
        var s = baseStats.Clone();
        s.Attack  *= 0.8f;
        s.Evasion *= 1.5f;
        s.Speed   *= 0.8f;
        return s;
    }
}
