using UnityEngine;

public class FrictionRampSystem : MonoBehaviour
{
    [SerializeField] private float rampRate   = 0.2f; // 초당 추가 damping
    [SerializeField] private float maxExtra   = 1f;   // 최대 추가 damping
    [SerializeField] private float increasePerWave = 0.04f; // [추가] 웨이브당 증가할 SustainTime

    private float timer;
    private bool  wasExchange;

    void Update()
    {
        if (GamePhaseManager.Instance == null) return;

        bool nowExchange = GamePhaseManager.Instance.IsExchangePhase;

        // Exchange → Billiard 전환 시 damping 복구 및 타이머 리셋
        if (wasExchange && !nowExchange)
        {
            ResetDamping();
            timer = 0f;
        }

        wasExchange = nowExchange;
        if (!nowExchange) return;

        timer += Time.deltaTime;

        // [수정] 기본 SustainTime + (웨이브 번호 - 1) * 증가량
        float baseSustain = PlayerData.Instance?.Stats.SustainTime ?? 6f;
        int currentWave = WaveManager.Instance?.CurrentWave ?? 1;
        float totalSustain = baseSustain + ((currentWave - 1) * increasePerWave);

        if (timer <= totalSustain) return;

        float extra = Mathf.Min((timer - totalSustain) * rampRate, maxExtra);

        foreach (var b in FindObjectsByType<BilliardBall>(FindObjectsSortMode.None))
        {
            if (b == null || !b.gameObject.activeSelf || !b.IsMoving) continue;
            b.Rigidbody.linearDamping = b.GetBaseDamping() + extra;
        }
    }

    // 재시작 시 호출하거나 페이즈 종료 시 호출할 댐핑 초기화 함수
    private void ResetDamping()
    {
        foreach (var b in FindObjectsByType<BilliardBall>(FindObjectsSortMode.None))
            if (b != null && b.gameObject.activeSelf)
                b.ApplySpeedToDamping();
    }
}