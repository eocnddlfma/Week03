// 우울: 방어+, 속도-
public class BlueEmotionModifier : IEmotionModifier
{
    public ColorType ColorType   => ColorType.Blue;
    public string    EmotionName => "우울";
    public string    Description => "방어↑  속도↓";

    public BallStats Apply(BallStats baseStats)
    {
        var s = baseStats.Clone();
        s.Defense *= 1.3f;
        s.Speed   *= 0.7f;
        return s;
    }
}
