using UnityEngine;

// 턴제 전투 시스템은 제거됨 - 충돌 기반 전투로 전환 (ExchangeSystem 참조)
// 씬의 GameObject 참조 유지를 위해 빈 싱글톤으로 보존
public class CombatSystem : MonoBehaviour
{
    public static CombatSystem Instance { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }
}
