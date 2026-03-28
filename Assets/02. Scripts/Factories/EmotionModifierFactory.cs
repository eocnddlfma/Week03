public static class EmotionModifierFactory
{
    public static IEmotionModifier Create(ColorType type) => type switch
    {
        ColorType.Red       => new RedEmotionModifier(),
        ColorType.Green     => new GreenEmotionModifier(),
        ColorType.Blue      => new BlueEmotionModifier(),
        ColorType.Yellow    => new YellowEmotionModifier(),
        ColorType.Cyan      => new CyanEmotionModifier(),
        ColorType.Magenta   => new MagentaEmotionModifier(),
        ColorType.White     => new WhiteEmotionModifier(),
        ColorType.Gray      => new GrayEmotionModifier(),
        ColorType.Black     => new BlackEmotionModifier(),
        ColorType.DeepBlack => new DeepBlackEmotionModifier(),
        _ => throw new System.Exception($"알 수 없는 색상 타입: {type}")
    };
}
