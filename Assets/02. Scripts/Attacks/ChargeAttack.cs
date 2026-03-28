using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Yellow - 가장 가까운 적을 향해 돌진, 충돌 시 공격
public class ChargeAttack : IAttackBehavior
{
    public AttackType Type => AttackType.Charge;

    private BilliardBall chargeTarget;
    private bool isCharging;

    public void Execute(BilliardBall self, List<BilliardBall> targets)
    {
        if (isCharging) return;

        var stats = self.GetEffectiveStats();
        chargeTarget = targets
            .Where(t => t != self && !t.Color.IsWhite())
            .OrderBy(t => Vector2.Distance(self.Position, t.Position))
            .FirstOrDefault();

        if (chargeTarget == null) return;

        isCharging = true;
        Vector2 dir = (chargeTarget.Position - self.Position).normalized;
        self.Rigidbody.linearVelocity = dir * stats.Speed * 5f;
    }

    public void OnHit(BilliardBall self, BilliardBall target)
    {
        if (!isCharging) return;
        target.TakeDamage(CombatCalculator.Calculate(self.GetEffectiveStats(), target.GetEffectiveStats()));
        isCharging = false;
        self.Rigidbody.linearVelocity = Vector2.zero;
    }
}
