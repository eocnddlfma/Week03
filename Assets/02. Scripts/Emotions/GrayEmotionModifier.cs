// 혐오: 가까이 오지 마. 방어적이고 회피적인 공포.
public class GrayEmotionModifier : IEmotionModifier
{
    public ColorType ColorType   => ColorType.Gray;
    public string    EmotionName => "혐오";
    public string    Description => "방어↑  회피↑  공격↓  명중↓";

    public BallStats Apply(BallStats baseStats)
    {
        var s = baseStats.Clone();
        s.Defense  *= 1.3f;
        s.Evasion  *= 1.4f;
        s.Attack   *= 0.8f;
        s.Accuracy *= 0.75f;
        return s;
    }
}
