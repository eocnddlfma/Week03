// 분노: 공격+, 방어-, 명중-
public class RedEmotionModifier : IEmotionModifier
{
    public ColorType ColorType   => ColorType.Red;
    public string    EmotionName => "분노";
    public string    Description => "공격↑  방어↓  명중↓";

    public BallStats Apply(BallStats baseStats)
    {
        var s = baseStats.Clone();
        s.Size     *= 1.1f;
        s.Attack   *= 1.3f;
        s.Defense  *= 0.7f;
        s.Accuracy *= 0.8f;
        return s;
    }
}
