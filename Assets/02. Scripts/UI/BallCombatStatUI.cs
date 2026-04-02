using TMPro;
using UnityEngine;

public class BallCombatStatUI : MonoBehaviour
{
    // 기존 Y값들을 간격 유닛(Unit)으로 활용합니다.
    private const float hpUnit  =  1.0f; 
    private const float atkUnit =  0.0f; 
    private const float defUnit = -1.0f; 

    [Header("참조 (UIBuilder로 자동 연결)")]
    [SerializeField] private TextMeshPro atkLabel;
    [SerializeField] private TextMeshPro hpLabel;
    [SerializeField] private TextMeshPro defLabel;

    [SerializeField] private float referenceOrtho = 6f;
    [SerializeField] private float baseFontSize = 0.5f; 
    
    [Header("간격 설정")]
    [SerializeField] private float spacingMultiplier = 0.7f; // 텍스트 간의 기본 간격 조절

    public static Transform Container { get; set; }

    private Transform ownerTransform;
    private float     maxHP;
    private Camera    mainCamera;

    void Awake()
    {
        mainCamera = Camera.main;
    }

    public void Setup(BilliardBall owner)
    {
        transform.SetParent(Container, worldPositionStays: true);
        ownerTransform = owner.transform;
        maxHP          = owner.Stats.MaxHP;
        transform.localScale = Vector3.one;

        bool isEnemy = owner.IsEnemy;
        if (atkLabel) atkLabel.color = isEnemy ? Color.red : new Color(1f, 0.6f, 0f);
        if (hpLabel)  hpLabel.color  = isEnemy ? new Color(1f, 0.6f, 0.6f) : Color.green;
        if (defLabel) defLabel.color = isEnemy ? Color.cyan : new Color(0.4f, 0.8f, 1f);

        Refresh(owner.PhaseAttack, owner.PhaseDefense, owner.CurrentHP, maxHP);
    }

    void LateUpdate()
    {
        if (ownerTransform == null) { Destroy(gameObject); return; }
        if (mainCamera == null) return;

        // 1. 카메라 줌에 따른 스케일 팩터 계산
        float scaleFactor = mainCamera.orthographicSize / referenceOrtho;
        
        // 2. 폰트 크기 업데이트
        float currentFontSize = baseFontSize * scaleFactor;
        if (atkLabel) atkLabel.fontSize = currentFontSize;
        if (hpLabel)  hpLabel.fontSize  = currentFontSize;
        if (defLabel) defLabel.fontSize = currentFontSize;

        // 3. 간격 계산 (공의 반지름 + 폰트 크기에 비례한 추가 간격)
        float ballRadius = ownerTransform.localScale.x * 0.5f;
        
        // 글자들 사이의 간격 또한 카메라 줌(scaleFactor)에 맞춰서 늘어나야 겹치지 않습니다.
        float verticalGap = spacingMultiplier * scaleFactor;

        Vector3 basePos = ownerTransform.position;

        // 중앙(ATK)을 기준으로 위(HP), 아래(DEF) 배치
        // 만약 공이 커졌을 때 글자가 공에 가려진다면 ballRadius를 더해주는 로직을 추가할 수 있습니다.
        // 여기서는 ATK를 공 중심에 두고 나머지를 위아래로 벌립니다.
        
        // 공의 크기가 커져도 글자가 겹치지 않게 하려면 offset에 ballRadius 영향을 줍니다.
        float totalOffset = verticalGap + (ballRadius * 0.2f); 

        SetLabelPosition(hpLabel,  basePos + Vector3.up * totalOffset);
        SetLabelPosition(atkLabel, basePos);
        SetLabelPosition(defLabel, basePos + Vector3.down * totalOffset);
    }

    private static void SetLabelPosition(TextMeshPro label, Vector3 pos)
    {
        if (label == null) return;
        label.transform.position = pos;
    }

    public void Refresh(float atk, float def, float hp, float maxHp)
    {
        if (atkLabel) atkLabel.text = $"ATK {atk:F0}";
        if (hpLabel)  hpLabel.text  = $"HP {hp:F0}/{maxHp:F0}"; 
        if (defLabel) defLabel.text = $"DEF {def:F0}";
    }
}