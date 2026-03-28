using UnityEngine;

public static class AttackBehaviorFactory
{
    // 생성 시 - 원색은 근/원거리 랜덤 결정
    public static IAttackBehavior Create(ColorType type) => type switch
    {
        ColorType.Red or ColorType.Green or ColorType.Blue
                          => Random.value > 0.5f ? new MeleeAttack() : new RangedAttack(),
        ColorType.Yellow  => new ChargeAttack(),
        ColorType.Cyan    => new JumpAreaAttack(),
        ColorType.Magenta => new OrbitAttack(),
        ColorType.White   => new IdleBehavior(),
        _ => throw new System.Exception($"알 수 없는 색상 타입: {type}")
    };

    // 색 변환 시 - 혼합색은 항상 고정 행동
    public static IAttackBehavior CreateOnColorChange(ColorType type) => type switch
    {
        ColorType.Yellow  => new ChargeAttack(),
        ColorType.Cyan    => new JumpAreaAttack(),
        ColorType.Magenta => new OrbitAttack(),
        ColorType.White   => new IdleBehavior(),
        _ => throw new System.Exception($"색 변환으로 원색이 될 수 없음: {type}")
    };
}
