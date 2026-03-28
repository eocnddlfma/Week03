// 안정: 치명+, 체력+, 명중+, 힐+
public class GreenEmotionModifier : IEmotionModifier
{
    public ColorType ColorType   => ColorType.Green;
    public string    EmotionName => "안정";
    public string    Description => "체력↑  치명↑  명중↑  힐↑";

    public BallStats Apply(BallStats baseStats)
    {
        var s = baseStats.Clone();
        s.Critical *= 1.2f;
        s.MaxHP    *= 1.2f;
        s.Accuracy *= 1.2f;
        s.Heal     *= 1.3f;
        return s;
    }
}
