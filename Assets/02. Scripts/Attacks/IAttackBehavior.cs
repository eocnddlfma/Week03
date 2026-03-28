using System.Collections.Generic;

public interface IAttackBehavior
{
    AttackType Type { get; }
    void Execute(BilliardBall self, List<BilliardBall> targets);
}
