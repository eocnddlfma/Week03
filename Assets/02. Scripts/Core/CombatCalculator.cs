using UnityEngine;

public static class CombatCalculator
{
    /// <summary>
    /// 명중/크리티컬/사거리 배율을 적용한 최종 데미지 반환. 빗나가면 0.
    /// </summary>
    /// <summary>
    /// 명중률 = 100 + Accuracy - Evasion (1스탯 = 1%).
    /// 100% 초과분은 크리티컬 확률로 전환.
    /// 크리티컬 시 데미지 ×2.
    /// </summary>
    public static float Calculate(BallStats attacker, BallStats defender, out bool evaded, out bool crit)
    {
        evaded = false;
        crit   = false;

        // ── 명중 판정 ──────────────────────────────────────────────
        float hitChance    = 100f + attacker.Accuracy - defender.Evasion;
        float overflowCrit = 0f;

        if (hitChance <= 0f) { evaded = true; return 0f; }

        if (hitChance >= 100f)
        {
            overflowCrit = hitChance - 100f;
        }
        else if (Random.value * 100f >= hitChance)
        {
            evaded = true;
            return 0f;
        }

        // ── 크리티컬 판정 ──────────────────────────────────────────
        float critChance = attacker.Critical + overflowCrit;
        float dmg        = attacker.Attack;
        if (Random.value * 100f < critChance)
        {
            dmg  *= 2f;
            crit  = true;
        }

        return dmg;
    }
}
