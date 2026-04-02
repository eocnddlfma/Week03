using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public static class TooltipPrefabModifier
{
    const string PrefabPath = "Assets/03. Prefab/Tooltip.prefab";

    // ── 크기 설정 ──────────────────────────────────────────────────
    const float BarWidth     = 220f;
    const float BarHeight    = 28f;
    const float LabelWidth   = 80f;
    const float InfoFontSize = 26f;
    const float BarFontSize  = 18f;
    const float InfoMinWidth = BarWidth + LabelWidth + 36f;

    // 스탯 행 이름 + bright 색 (공 색과 일치)
    // Attack=Red / Defense=Blue / HP=Green / Speed=Yellow
    // Evasion=White / Accuracy=Magenta / Critical=Cyan / Heal=White(연두)
    static readonly (string row, Color bright)[] StatRows =
    {
        ("공격력Row", new Color(1f,    0.25f, 0.25f)),
        ("방어력Row", new Color(0.25f, 0.45f, 1f   )),
        ("체력Row",   new Color(0.2f,  0.85f, 0.2f )),
        ("속도Row",   new Color(1f,    1f,    0.2f  )),
        ("회피Row",   new Color(0.85f, 0.85f, 0.85f)),
        ("명중률Row", new Color(1f,    0.25f, 1f    )),
        ("치명Row",   new Color(0.2f,  1f,    1f    )),
        ("힐Row",     new Color(0.75f, 1f,    0.8f  )),
    };

    [MenuItem("Tools/툴팁 프리팹 수정")]
    static void Modify()
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (asset == null) { Debug.LogError($"[TooltipModifier] {PrefabPath} 없음"); return; }

        using var scope = new PrefabUtility.EditPrefabContentsScope(PrefabPath);
        var root = scope.prefabContentsRoot;

        // ── 정보 텍스트 폰트 크기 ──────────────────────────────────
        SetFontSize(root, "NameText",        28f);
        SetFontSize(root, "EmotionText",     InfoFontSize);
        SetFontSize(root, "ColorText",       InfoFontSize);
        SetFontSize(root, "AttackTypeText",  InfoFontSize);
        SetFontSize(root, "EmotionDescText", InfoFontSize - 4f);
        SetFontSize(root, "PassionText",     InfoFontSize - 4f);

        // ── InfoGroup 최소 너비 ────────────────────────────────────
        var infoGroup = FindDeep(root.transform, "InfoGroup");
        if (infoGroup != null)
        {
            var le = infoGroup.GetComponent<LayoutElement>();
            if (le == null) le = infoGroup.gameObject.AddComponent<LayoutElement>();
            le.minWidth = InfoMinWidth;
        }

        // ── 스탯 바 행 수정 ────────────────────────────────────────
        foreach (var (rowName, bright) in StatRows)
        {
            var row = FindDeep(root.transform, rowName);
            if (row == null) { Debug.LogWarning($"[TooltipModifier] 행 없음: {rowName}"); continue; }

            Color dim = new Color(bright.r * 0.35f, bright.g * 0.35f, bright.b * 0.35f);

            // Label
            var labelT = row.Find("Label");
            if (labelT != null)
            {
                var le = labelT.GetComponent<LayoutElement>();
                if (le != null) { le.minWidth = LabelWidth; le.preferredWidth = LabelWidth; }
                var tmp = labelT.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null) tmp.fontSize = BarFontSize;
            }

            // Bar 컨테이너
            var barT = row.Find("Bar");
            if (barT != null)
            {
                var le = barT.GetComponent<LayoutElement>();
                if (le != null) { le.minWidth = BarWidth; le.preferredWidth = BarWidth; }
                var rect = barT.GetComponent<RectTransform>();
                if (rect != null) rect.sizeDelta = new Vector2(BarWidth, BarHeight);

                // Fill 색상
                SetChildImageColor(barT, "DimFill",    dim);
                SetChildImageColor(barT, "BrightFill", bright);

                // ValueText
                var valT = barT.Find("ValueText");
                if (valT != null)
                {
                    var tmp  = valT.GetComponent<TextMeshProUGUI>();
                    if (tmp  != null) tmp.fontSize = BarFontSize;
                    var vrect = valT.GetComponent<RectTransform>();
                    if (vrect != null) vrect.sizeDelta = new Vector2(BarWidth - 10f, vrect.sizeDelta.y);
                }
            }
        }

        Debug.Log("[TooltipModifier] 완료");
    }

    // ── 헬퍼 ──────────────────────────────────────────────────────
    static void SetFontSize(GameObject root, string goName, float size)
    {
        var t = FindDeep(root.transform, goName);
        if (t == null) { Debug.LogWarning($"[TooltipModifier] 없음: {goName}"); return; }
        var tmp = t.GetComponent<TextMeshProUGUI>();
        if (tmp != null) tmp.fontSize = size;
    }

    static void SetChildImageColor(Transform parent, string childName, Color color)
    {
        var child = parent.Find(childName);
        if (child == null) return;
        var img = child.GetComponent<Image>();
        if (img != null) img.color = color;
    }

    static Transform FindDeep(Transform t, string name)
    {
        if (t.name == name) return t;
        foreach (Transform child in t)
        {
            var found = FindDeep(child, name);
            if (found != null) return found;
        }
        return null;
    }
}
