using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TooltipUI : MonoBehaviour
{
    [Header("연결 필수")]
    [SerializeField] private RectTransform tooltipPanel;
    [SerializeField] private TMP_FontAsset koreanFont;

    [Header("정보 텍스트")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI passionText;
    [SerializeField] private TextMeshProUGUI colorText;
    [SerializeField] private TextMeshProUGUI emotionText;
    [SerializeField] private TextMeshProUGUI emotionDescText;
    [SerializeField] private TextMeshProUGUI attackTypeText;

    [Header("스탯 바 (순서: 공격/방어/체력/속도/회피/명중률/치명/힐)")]
    [SerializeField] private StatBarUI[] statBars; // 8개


    private Camera mainCamera;

    void Awake()
    {
        mainCamera = Camera.main;
        if (koreanFont != null)
            foreach (var tmp in tooltipPanel.GetComponentsInChildren<TextMeshProUGUI>(true))
                tmp.font = koreanFont;
        tooltipPanel.gameObject.SetActive(false);
    }

    void Update()
    {
        Vector2 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        var hit = Physics2D.OverlapPoint(mouseWorld);

        if (hit != null && hit.TryGetComponent<BilliardBall>(out var ball))
        {
            ShowTooltip(ball.GetTooltipData());
            FollowMouse();
        }
        else
        {
            tooltipPanel.gameObject.SetActive(false);
        }
    }

    private void ShowTooltip(BallTooltipData data)
    {
        tooltipPanel.gameObject.SetActive(true);

        if (nameText)    nameText.text    = data.MemoryName;
        if (passionText) passionText.text = $"열정: {data.PassionSummary}";
        colorText.text       = $"색상: {ColorTypeToKorean(data.ColorType)}";
        emotionText.text     = $"감정: {data.EmotionName}";
        emotionDescText.text = data.EmotionDesc;
        attackTypeText.text  = $"공격: {AttackTypeToKorean(data.AttackType)}";

        UpdateBars(data);
        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipPanel);
    }

    private void UpdateBars(BallTooltipData data)
    {
        var b = data.BaseStats;
        var e = data.EffectiveStats;

        SetStatBar(statBars[0], b.Attack,   e.Attack,   b.AttackMax,  data.GetPassion(StatType.Attack));
        SetStatBar(statBars[1], b.Defense,  e.Defense,  b.DefenseMax, data.GetPassion(StatType.Defense));

        float maxHP = Mathf.Max(b.MaxHP, 1f);
        SetBar(statBars[2], maxHP, data.CurrentHP, b.MaxHPMax,
               $"{data.CurrentHP:F0} / {b.MaxHP:F0}{PassionSymbol(data.GetPassion(StatType.HP))}");

        SetStatBar(statBars[3], b.Speed,    e.Speed,    b.SpeedMax,    data.GetPassion(StatType.Speed));
        SetStatBar(statBars[4], b.Evasion,  e.Evasion,  b.EvasionMax,  data.GetPassion(StatType.Evasion));
        SetStatBar(statBars[5], b.Accuracy, e.Accuracy, b.AccuracyMax, data.GetPassion(StatType.Accuracy));
        SetStatBar(statBars[6], b.Critical, e.Critical, b.CriticalMax, data.GetPassion(StatType.Critical));
        SetStatBar(statBars[7], b.Heal,     e.Heal,     b.HealMax,     data.GetPassion(StatType.Heal));
    }

    private void SetStatBar(StatBarUI bar, float baseVal, float effectiveVal, float maxVal,
                            PassionLevel passion = PassionLevel.None)
    {
        float diff = effectiveVal - baseVal;
        string diffStr = Mathf.Approximately(diff, 0f) ? ""
            : $" ({(diff > 0 ? "+" : "")}{diff:F1})";
        SetBar(bar, baseVal, effectiveVal, maxVal, $"{effectiveVal:F1}{diffStr}{PassionSymbol(passion)}");
    }

    private static string PassionSymbol(PassionLevel p) => p switch
    {
        PassionLevel.Minor => " ★",
        PassionLevel.Major => " ★★",
        _                  => ""
    };

    private void SetBar(StatBarUI bar, float dimVal, float brightVal, float refMax, string label)
    {
        bar.dimFill.anchorMax    = new Vector2(Mathf.Clamp01(dimVal    / refMax), 1f);
        bar.brightFill.anchorMax = new Vector2(Mathf.Clamp01(brightVal / refMax), 1f);
        bar.valueText.text       = label;
    }

    private void FollowMouse()
    {
        Vector3 pos = Input.mousePosition + new Vector3(15f, -15f, 0f);
        pos.x = Mathf.Clamp(pos.x, 0f, Screen.width  - tooltipPanel.rect.width);
        pos.y = Mathf.Clamp(pos.y, tooltipPanel.rect.height, Screen.height);
        tooltipPanel.position = pos;
    }

    private string ColorTypeToKorean(ColorType t) => t switch
    {
        ColorType.Red     => "빨강",  ColorType.Green   => "초록",
        ColorType.Blue    => "파랑",  ColorType.Yellow  => "노랑",
        ColorType.Cyan    => "청록",  ColorType.Magenta => "자홍",
        ColorType.White   => "흰색",  _ => "?"
    };

    private string AttackTypeToKorean(AttackType t) => t switch
    {
        AttackType.Melee    => "근거리", AttackType.Ranged   => "원거리",
        AttackType.Charge   => "돌진",   AttackType.JumpArea => "점프 착지",
        AttackType.Orbit    => "공전",   AttackType.Idle     => "없음",
        _ => "?"
    };
}
