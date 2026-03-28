// 기대: 체력+, 방어+, 치명-
public class CyanEmotionModifier : IEmotionModifier
{
    public ColorType ColorType   => ColorType.Cyan;
    public string    EmotionName => "기대";
    public string    Description => "체력↑  방어↑  치명↓";

    public BallStats Apply(BallStats baseStats)
    {
        var s = baseStats.Clone();
        s.MaxHP    *= 1.2f;
        s.Defense  *= 1.2f;
        s.Critical *= 0.7f;
        return s;
    }
}
