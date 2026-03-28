using System.Collections.Generic;

// White - 아무것도 안함
public class IdleBehavior : IAttackBehavior
{
    public AttackType Type => AttackType.Idle;

    public void Execute(BilliardBall self, List<BilliardBall> targets) { }
}
