using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Cyan - 적 위로 점프 후 착지, 범위 내 모든 적 공격
public class JumpAreaAttack : IAttackBehavior
{
    public AttackType Type => AttackType.JumpArea;

    private const float LandRadius = 2f;

    public void Execute(BilliardBall self, List<BilliardBall> targets)
    {
        var jumpTarget = targets
            .Where(t => t != self && !t.Color.IsWhite())
            .OrderBy(t => Vector2.Distance(self.Position, t.Position))
            .FirstOrDefault();

        if (jumpTarget == null) return;

        // TODO: 점프 연출 (포물선 이동 코루틴)
        LandAt(self, jumpTarget.Position, targets);
    }

    private void LandAt(BilliardBall self, Vector2 landPos, List<BilliardBall> targets)
    {
        var stats    = self.GetEffectiveStats();
        var inRadius = targets
            .Where(t => t != self && !t.Color.IsWhite())
            .Where(t => Vector2.Distance(landPos, t.Position) <= LandRadius);

        foreach (var target in inRadius)
            target.TakeDamage(CombatCalculator.Calculate(stats, target.GetEffectiveStats()));
    }
}
