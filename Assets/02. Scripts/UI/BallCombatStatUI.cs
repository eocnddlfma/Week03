using TMPro;
using UnityEngine;

public class BallCombatStatUI : MonoBehaviour
{
    [Header("위치 (월드 단위)")]
    [SerializeField] private float hpLabelY     =  0.55f; // HP: 위 중앙
    [SerializeField] private float combatLabelY =  0.0f;  // ATK/DEF: 아래 좌우
    [SerializeField] private float combatLabelX =  0.55f; // ATK 왼쪽 / DEF 오른쪽 간격

    [Header("참조 (UIBuilder로 자동 연결)")]
    [SerializeField] private TextMeshPro atkLabel;
    [SerializeField] private TextMeshPro hpLabel;
    [SerializeField] private TextMeshPro defLabel;

    public static Transform Container { get; set; } // BallFactory에서 주입

    private Transform ownerTransform;
    private float     maxHP;

    private static string PassionSymbol(PassionLevel p) => p switch
    {
        PassionLevel.Minor => "★",
        PassionLevel.Major => "★★",
        _                  => ""
    };

    public void Setup(BilliardBall owner)
    {
        // UI 컨테이너 아래로 이동 (없으면 루트로 분리)
        transform.SetParent(Container, worldPositionStays: true);

        ownerTransform = owner.transform;
        maxHP          = owner.Stats.MaxHP;
        bool isEnemy   = owner.IsEnemy;

        if (atkLabel) atkLabel.color = isEnemy ? Color.red                  : new Color(1f, 0.6f, 0f);
        if (hpLabel)  hpLabel.color  = isEnemy ? new Color(1f, 0.6f, 0.6f) : Color.green;
        if (defLabel) defLabel.color = isEnemy ? Color.cyan                 : new Color(0.4f, 0.8f, 1f);

        Refresh(owner.PhaseAttack, owner.PhaseDefense, owner.CurrentHP,
                owner.GetPassion(StatType.Attack), owner.GetPassion(StatType.Defense));
    }

    void LateUpdate()
    {
        if (ownerTransform == null) { Destroy(gameObject); return; }
        Vector3 base_ = ownerTransform.position;
        if (hpLabel)  hpLabel.transform.position  = base_ + Vector3.up * hpLabelY;
        if (atkLabel) atkLabel.transform.position = base_ + Vector3.up * combatLabelY - Vector3.right * combatLabelX;
        if (defLabel) defLabel.transform.position = base_ + Vector3.up * combatLabelY + Vector3.right * combatLabelX;
    }

    public void Refresh(float atk, float def, float hp,
                        PassionLevel atkPassion = PassionLevel.None,
                        PassionLevel defPassion = PassionLevel.None)
    {
        if (atkLabel) atkLabel.text = $"ATK {atk:F0}";
        if (hpLabel)  hpLabel.text  = $"HP {hp:F0}/{maxHP:F0}";
        if (defLabel) defLabel.text = $"DEF {def:F0}";
    }
}
