using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RangedAttack : IAttackBehavior
{
    public AttackType Type => AttackType.Ranged;

    public void Execute(BilliardBall self, List<BilliardBall> targets)
    {
        var stats  = self.GetEffectiveStats();
        var target = targets
            .Where(t => t != self && !t.Color.IsWhite())
            .OrderBy(t => Vector2.Distance(self.Position, t.Position))
            .FirstOrDefault();

        if (target == null) return;
        target.TakeDamage(CombatCalculator.Calculate(stats, target.GetEffectiveStats()));
    }
}
