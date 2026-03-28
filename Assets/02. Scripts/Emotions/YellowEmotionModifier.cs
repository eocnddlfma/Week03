// 행복: 명중-, 속도+, 회피+, 치명+
public class YellowEmotionModifier : IEmotionModifier
{
    public ColorType ColorType   => ColorType.Yellow;
    public string    EmotionName => "행복";
    public string    Description => "속도↑  회피↑  치명↑  명중↓";

    public BallStats Apply(BallStats baseStats)
    {
        var s = baseStats.Clone();
        s.Accuracy *= 0.8f;
        s.Speed    *= 1.3f;
        s.Evasion  *= 1.3f;
        s.Critical *= 1.3f;
        return s;
    }
}
