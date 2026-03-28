using UnityEngine;

public static class CombatCalculator
{
    /// <summary>
    /// 명중/크리티컬/사거리 배율을 적용한 최종 데미지 반환. 빗나가면 0.
    /// </summary>
    public static float Calculate(BallStats attacker, BallStats defender)
    {
        // ── 명중 판정 ──────────────────────────────────────────────
        // 기본 100 + 내 Accuracy - 상대 Evasion (0~100 클램프)
        float hitChance = Mathf.Clamp(100f + attacker.Accuracy - defender.Evasion, 0f, 100f);
        if (Random.value * 100f > hitChance) return 0f; // miss

        float dmg = attacker.Attack;

        // ── 크리티컬 ───────────────────────────────────────────────
        // Critical 100마다 확정 1크리 + 나머지로 추가 크리 롤
        // 크리 1회마다 데미지 2배 (1크리=2배, 2크리=4배, 3크리=8배 ...)
        int   guaranteed = Mathf.FloorToInt(attacker.Critical / 100f);
        float remainder  = attacker.Critical % 100f;
        int   rolled     = Random.value * 100f < remainder ? 1 : 0;
        int   totalCrits = guaranteed + rolled;

        dmg *= Mathf.Pow(2f, totalCrits);

        return dmg;
    }
}
