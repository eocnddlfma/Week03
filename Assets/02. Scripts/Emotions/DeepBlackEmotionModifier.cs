// 붕괴: 모든 것이 무너진다. 제어 불가능한 파괴.
public class DeepBlackEmotionModifier : IEmotionModifier
{
    public ColorType ColorType   => ColorType.DeepBlack;
    public string    EmotionName => "붕괴";
    public string    Description => "공격↑↑  방어↑↑  속도↓↓";

    public BallStats Apply(BallStats baseStats)
    {
        var s = baseStats.Clone();
        s.Size    *= 1.4f;
        s.Attack  *= 1.6f;
        s.Defense *= 1.5f;
        s.Speed   *= 0.5f;
        s.Evasion *= 0.6f;
        return s;
    }
}
