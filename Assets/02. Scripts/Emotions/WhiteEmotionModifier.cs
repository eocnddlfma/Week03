// 현타옴: 스탯 보정 없음
public class WhiteEmotionModifier : IEmotionModifier
{
    public ColorType ColorType   => ColorType.White;
    public string    EmotionName => "현타옴";
    public string    Description => "-";

    public BallStats Apply(BallStats baseStats)
    {
        var s  = baseStats.Clone();
        s.Size *= 0.85f;
        return s;
    }
}
