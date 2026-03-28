public interface IEmotionModifier
{
    ColorType ColorType   { get; }
    string    EmotionName { get; }
    string    Description { get; } // 보정 내용 요약
    BallStats Apply(BallStats baseStats);
}
