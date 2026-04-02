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
            TutorialManager.NotifyBallHovered(ball);
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

        if (nameText)  nameText.text = data.MemoryName;
        if (colorText) colorText.gameObject.SetActive(false);
        emotionText.text = $"감정: {data.EmotionName}";

        var ct = data.ColorType;
        bool isEnemy = ct == ColorType.Gray || ct == ColorType.Black || ct == ColorType.DeepBlack;

        if (passionText) passionText.gameObject.SetActive(!isEnemy);
        if (!isEnemy && passionText) passionText.text = $"열정: {data.PassionSummary}";

        emotionDescText.text = isEnemy ? data.EmotionDesc : $"{data.EmotionDesc}\n{GetExchangeInfo(data)}";

        UpdateBars(data);
        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipPanel);
    }

    private string GetExchangeInfo(BallTooltipData data)
    {
        var    ct  = data.ColorType;
        string hex = ColorTypeToHex(ct);

        if (ct == ColorType.Gray || ct == ColorType.Black || ct == ColorType.DeepBlack)
            return "<color=#FF6666>충돌 시 전투 발생</color>";

        if (ct == ColorType.White)
        {
            float ev   = Mathf.Max(1f, data.BaseStats.Get(StatType.Evasion) / 100f);
            float heal = Mathf.Max(1f, data.BaseStats.Get(StatType.Heal)    / 100f);
            return $"<color={hex}>충돌 부여: 회피 +{ev:F1} / 힐 +{heal:F1} (랜덤)</color>";
        }

        StatType st     = ColorState.From(ct).GetExchangeStat();
        float    amount = Mathf.Max(1f, data.BaseStats.Get(st) / 100f);
        return $"<color={hex}>충돌 부여: {StatName(st)} +{amount:F1}</color>";
    }

    private static string ColorTypeToHex(ColorType t) => t switch
    {
        ColorType.Red     => "#FF4444",
        ColorType.Green   => "#44DD44",
        ColorType.Blue    => "#4488FF",
        ColorType.Yellow  => "#FFDD00",
        ColorType.Cyan    => "#00DDDD",
        ColorType.Magenta => "#FF44FF",
        ColorType.White   => "#FFFFFF",
        _                 => "#AAAAAA"
    };

    private static string StatName(StatType t) => t switch
    {
        StatType.Attack   => "공격",
        StatType.Defense  => "방어",
        StatType.HP       => "체력",
        StatType.Speed    => "속도",
        StatType.Evasion  => "회피",
        StatType.Accuracy => "명중",
        StatType.Critical => "치명",
        StatType.Heal     => "힐",
        _                 => "?"
    };

    private void UpdateBars(BallTooltipData data)
    {
        var b = data.BaseStats;
        var e = data.EffectiveStats;

        SetStatBar(statBars[0], b.Attack,   e.Attack,   BallStats.AttackMax,   data.GetPassion(StatType.Attack));
        SetStatBar(statBars[1], b.Defense,  e.Defense,  BallStats.DefenseMax,  data.GetPassion(StatType.Defense));

        float maxHP = Mathf.Max(b.MaxHP, 1f);
        SetBar(statBars[2], maxHP, data.CurrentHP, BallStats.MaxHPMax,
               $"{data.CurrentHP:F0} / {b.MaxHP:F0}{PassionSymbol(data.GetPassion(StatType.HP))}");

        SetStatBar(statBars[3], b.Speed,    e.Speed,    BallStats.SpeedMax,    data.GetPassion(StatType.Speed));
        SetPercentBar(statBars[4], b.Evasion,  e.Evasion,  BallStats.EvasionMax,  data.GetPassion(StatType.Evasion));
        SetPercentBar(statBars[5], b.Accuracy, e.Accuracy, BallStats.AccuracyMax, data.GetPassion(StatType.Accuracy));
        SetPercentBar(statBars[6], b.Critical, e.Critical, BallStats.CriticalMax, data.GetPassion(StatType.Critical));
        SetStatBar(statBars[7], b.Heal,     e.Heal,     BallStats.HealMax,     data.GetPassion(StatType.Heal));
    }

    private void SetStatBar(StatBarUI bar, float baseVal, float effectiveVal, float maxVal,
                            PassionLevel passion = PassionLevel.None)
    {
        float diff = effectiveVal - baseVal;
        string diffStr = Mathf.Approximately(diff, 0f) ? ""
            : $" ({(diff > 0 ? "+" : "")}{diff:F1})";
        SetBar(bar, baseVal, effectiveVal, maxVal, $"{effectiveVal:F1}{diffStr}{PassionSymbol(passion)}");
    }

    // 회피·명중률·치명처럼 값 자체가 % 단위인 스탯용
    private void SetPercentBar(StatBarUI bar, float baseVal, float effectiveVal, float maxVal,
                               PassionLevel passion = PassionLevel.None)
    {
        float diff = effectiveVal - baseVal;
        string diffStr = Mathf.Approximately(diff, 0f) ? ""
            : $" ({(diff > 0 ? "+" : "")}{diff:F0}%)";
        SetBar(bar, baseVal, effectiveVal, maxVal, $"{effectiveVal:F0}%{diffStr}{PassionSymbol(passion)}");
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

}
