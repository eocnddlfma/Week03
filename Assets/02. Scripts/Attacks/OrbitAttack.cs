using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Magenta - 중점을 기준으로 공전하며 나선형으로 접근, 충돌 시 공격
public class OrbitAttack : IAttackBehavior
{
    public AttackType Type => AttackType.Orbit;

    private BilliardBall orbitTarget;
    private float orbitAngle;
    private float orbitRadius;
    private const float AngularSpeed = 180f;
    private const float SpiralSpeed  = 0.5f;

    public void Execute(BilliardBall self, List<BilliardBall> targets)
    {
        if (orbitTarget == null || orbitTarget.Color.IsWhite())
        {
            orbitTarget = targets
                .Where(t => t != self && !t.Color.IsWhite())
                .OrderBy(t => Vector2.Distance(self.Position, t.Position))
                .FirstOrDefault();

            if (orbitTarget == null) return;

            orbitRadius = Vector2.Distance(self.Position, orbitTarget.Position);
            orbitAngle  = Mathf.Atan2(
                self.Position.y - orbitTarget.Position.y,
                self.Position.x - orbitTarget.Position.x) * Mathf.Rad2Deg;
        }

        orbitAngle  += AngularSpeed * Time.deltaTime;
        orbitRadius -= SpiralSpeed  * Time.deltaTime;

        if (orbitRadius <= 0.3f)
        {
            orbitTarget.TakeDamage(CombatCalculator.Calculate(self.GetEffectiveStats(), orbitTarget.GetEffectiveStats()));
            orbitTarget = null;
            return;
        }

        float rad    = orbitAngle * Mathf.Deg2Rad;
        Vector2 newPos = orbitTarget.Position + new Vector2(
            Mathf.Cos(rad) * orbitRadius,
            Mathf.Sin(rad) * orbitRadius);

        self.Rigidbody.MovePosition(newPos);
    }
}
