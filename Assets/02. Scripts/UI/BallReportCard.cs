using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BallReportCard : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text statsText;

    private static readonly StatType[] StatOrder =
    {
        StatType.Attack, StatType.Defense, StatType.HP, StatType.Speed,
        StatType.Evasion, StatType.Accuracy, StatType.Critical, StatType.Heal
    };

    public void Setup(ReportEntry entry)
    {
        string tag      = entry.Ball.IsEnemy ? "<color=#FF8888>[적]</color> " : "";
        string colorName = entry.Ball.Color.GetColorType().ToString();
        nameText.text = $"{tag}<b>{entry.Ball.MemoryName}</b>  <size=80%>{colorName}</size>";

        var sb = new StringBuilder();
        foreach (var st in StatOrder)
        {
            float after = entry.After.Get(st);
            if (entry.HasSnapshot)
            {
                float before = entry.Before.Get(st);
                float delta  = after - before;
                string dColor = delta > 0f ? "#6BFF6B" : delta < 0f ? "#FF6B6B" : "#888888";
                string sign   = delta >= 0f ? "+" : "";
                sb.AppendLine($"<mspace=0.85em>{StatName(st),2}</mspace>  {before,5:F1} → {after,5:F1}  <color={dColor}>{sign}{delta:F1}</color>");
            }
            else
            {
                sb.AppendLine($"<mspace=0.85em>{StatName(st),2}</mspace>  <color=#AAAAAA>{after,5:F1}</color>");
            }
        }
        statsText.text = sb.ToString();
    }

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
}
