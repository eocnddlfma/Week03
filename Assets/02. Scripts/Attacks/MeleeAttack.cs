using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeleeAttack : IAttackBehavior
{
    public AttackType Type => AttackType.Melee;

    public void Execute(BilliardBall self, List<BilliardBall> targets)
    {
        var stats   = self.GetEffectiveStats();
        var nearest = targets
            .Where(t => t != self && !t.Color.IsWhite())
            .OrderBy(t => Vector2.Distance(self.Position, t.Position))
            .FirstOrDefault();

        if (nearest == null) return;

        nearest.TakeDamage(CombatCalculator.Calculate(stats, nearest.GetEffectiveStats()));
    }
}
