using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class TooltipPrefabBuilder : EditorWindow
{
    // ── 크기 설정 ─────────────────────────────────────────────────
    private const float BarWidth     = 220f;
    private const float BarHeight    = 28f;
    private const float LabelWidth   = 80f;
    private const float InfoFontSize = 26f;
    private const float BarFontSize  = 18f;
    // 정보 섹션 최소 너비 (스탯 바 행 너비에 맞춤)
    private const float InfoMinWidth = BarWidth + LabelWidth + 36f;

    // 색상: 각 스탯을 부여하는 공의 색과 일치
    // Attack=Red / Defense=Blue / HP=Green / Speed=Yellow
    // Evasion=White / Accuracy=Magenta / Critical=Cyan / Heal=White
    private static readonly (string name, Color bright)[] StatDefs =
    {
        ("공격력", new Color(1f,    0.25f, 0.25f)),  // Red
        ("방어력", new Color(0.25f, 0.45f, 1f   )),  // Blue
        ("체력",   new Color(0.2f,  0.85f, 0.2f )),  // Green
        ("속도",   new Color(1f,    1f,    0.2f  )),  // Yellow
        ("회피",   new Color(0.85f, 0.85f, 0.85f)),  // White
        ("명중률", new Color(1f,    0.25f, 1f    )),  // Magenta
        ("치명",   new Color(0.2f,  1f,    1f    )),  // Cyan
        ("힐",     new Color(0.75f, 1f,    0.8f  )),  // White (연두 계열로 구분)
    };

    private TMP_FontAsset font;

    [MenuItem("Tools/툴팁 프리팹 생성")]
    static void Open() => GetWindow<TooltipPrefabBuilder>("툴팁 빌더");

    void OnGUI()
    {
        GUILayout.Space(8);
        font = (TMP_FontAsset)EditorGUILayout.ObjectField(
            "한글 폰트", font, typeof(TMP_FontAsset), false);
        GUILayout.Space(8);

        GUI.enabled = font != null;
        if (GUILayout.Button("프리팹 생성", GUILayout.Height(36)))
            Build(font);
        GUI.enabled = true;

        if (font == null)
            EditorGUILayout.HelpBox("폰트를 먼저 연결해주세요.", MessageType.Warning);
    }

    // ── 생성 ──────────────────────────────────────────────────────
    static void Build(TMP_FontAsset font)
    {
        // Canvas 확보
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            var cgo = new GameObject("Canvas");
            canvas = cgo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            cgo.AddComponent<CanvasScaler>();
            Undo.RegisterCreatedObjectUndo(cgo, "Create Tooltip");
        }

        // 루트
        var root    = CreateRect("Tooltip", canvas.transform).gameObject;
        var tooltip = root.AddComponent<TooltipUI>();
        Undo.RegisterCreatedObjectUndo(root, "Create Tooltip");

        // 패널
        var panelRect = CreateRect("TooltipPanel", root.transform);
        panelRect.pivot     = new Vector2(0f, 1f);
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.zero;

        var panel = panelRect.gameObject;
        panel.AddComponent<Image>().color = new Color(0.08f, 0.08f, 0.08f, 0.92f);

        var vg = panel.AddComponent<VerticalLayoutGroup>();
        vg.padding                = new RectOffset(12, 12, 10, 10);
        vg.spacing                = 5f;
        vg.childForceExpandWidth  = true;
        vg.childForceExpandHeight = false;
        vg.childControlWidth      = true;
        vg.childControlHeight     = true;

        var csf = panel.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;

        // 정보 그룹
        var infoGroup      = CreateVertGroup("InfoGroup",      panel.transform, 2f);
        var infoLE = infoGroup.AddComponent<LayoutElement>();
        infoLE.minWidth = InfoMinWidth;
        var nameTmp        = CreateTMP("NameText",       infoGroup.transform, InfoFontSize + 2f, font);
        nameTmp.fontStyle  = FontStyles.Bold;
        var passionTmp     = CreateTMP("PassionText",    infoGroup.transform, InfoFontSize - 2f, font);
        passionTmp.color   = new Color(1f, 0.85f, 0.3f);
        var colorTmp       = CreateTMP("ColorText",      infoGroup.transform, InfoFontSize, font);
        var emotionTmp     = CreateTMP("EmotionText",    infoGroup.transform, InfoFontSize, font);
        var emotionDescTmp = CreateTMP("EmotionDescText",infoGroup.transform, InfoFontSize - 2f, font);
        emotionDescTmp.color = new Color(0.8f, 0.8f, 0.8f);
        var attackTmp      = CreateTMP("AttackTypeText", infoGroup.transform, InfoFontSize, font);

        // 구분선
        var divGO = CreateRect("Divider", panel.transform).gameObject;
        divGO.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.2f);
        var divLE = divGO.AddComponent<LayoutElement>();
        divLE.preferredHeight = 1f;
        divLE.flexibleWidth   = 1f;

        // 스탯 그룹
        var statsGroup = CreateVertGroup("StatsGroup", panel.transform, 3f);
        var bars = new StatBarUI[StatDefs.Length];
        for (int i = 0; i < StatDefs.Length; i++)
        {
            var (name, bright) = StatDefs[i];
            bars[i] = CreateBarRow(statsGroup.transform, name, bright, font);
        }

        // 참조 연결
        var so = new SerializedObject(tooltip);
        so.FindProperty("tooltipPanel").objectReferenceValue    = panelRect;
        so.FindProperty("koreanFont").objectReferenceValue      = font;
        so.FindProperty("nameText").objectReferenceValue        = nameTmp;
        so.FindProperty("passionText").objectReferenceValue     = passionTmp;
        so.FindProperty("colorText").objectReferenceValue       = colorTmp;
        so.FindProperty("emotionText").objectReferenceValue     = emotionTmp;
        so.FindProperty("emotionDescText").objectReferenceValue = emotionDescTmp;
        so.FindProperty("attackTypeText").objectReferenceValue  = attackTmp;
        var barsProp = so.FindProperty("statBars");
        barsProp.arraySize = bars.Length;
        for (int i = 0; i < bars.Length; i++)
            barsProp.GetArrayElementAtIndex(i).objectReferenceValue = bars[i];
        so.ApplyModifiedProperties();

        // 프리팹 저장
        const string dir  = "Assets/03. Prefab";
        if (!AssetDatabase.IsValidFolder(dir))
            AssetDatabase.CreateFolder("Assets", "03. Prefab");

        const string path = dir + "/Tooltip.prefab";
        PrefabUtility.SaveAsPrefabAssetAndConnect(root, path, InteractionMode.AutomatedAction);

        Selection.activeGameObject = root;
        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<GameObject>(path));
        Debug.Log($"[TooltipPrefabBuilder] 완료 → {path}");
    }

    // ── 헬퍼 ──────────────────────────────────────────────────────
    static RectTransform CreateRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    static GameObject CreateVertGroup(string name, Transform parent, float spacing)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var vg = go.AddComponent<VerticalLayoutGroup>();
        vg.spacing                = spacing;
        vg.childForceExpandWidth  = false;
        vg.childForceExpandHeight = false;
        vg.childControlWidth      = false;
        vg.childControlHeight     = true;

        var csf = go.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;

        return go;
    }

    static TextMeshProUGUI CreateTMP(string name, Transform parent, float size, TMP_FontAsset font)
    {
        var go  = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.color    = Color.white;
        tmp.fontSize = size;
        tmp.font     = font;
        tmp.text     = "";
        return tmp;
    }

    static StatBarUI CreateBarRow(Transform parent, string label, Color bright, TMP_FontAsset font)
    {
        Color dim = new Color(bright.r * 0.4f, bright.g * 0.4f, bright.b * 0.4f);

        var rowGO = new GameObject(label + "Row", typeof(RectTransform));
        rowGO.transform.SetParent(parent, false);

        var hg = rowGO.AddComponent<HorizontalLayoutGroup>();
        hg.spacing                = 6f;
        hg.childForceExpandWidth  = false;
        hg.childForceExpandHeight = false;
        hg.childControlWidth      = true;
        hg.childControlHeight     = true;
        hg.childAlignment         = TextAnchor.MiddleLeft;

        var rowFitter = rowGO.AddComponent<ContentSizeFitter>();
        rowFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        rowFitter.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;

        // 레이블
        var labelGO  = new GameObject("Label", typeof(RectTransform));
        labelGO.transform.SetParent(rowGO.transform, false);
        var labelTmp = labelGO.AddComponent<TextMeshProUGUI>();
        labelTmp.color     = new Color(0.75f, 0.75f, 0.75f);
        labelTmp.fontSize  = BarFontSize;
        labelTmp.alignment = TextAlignmentOptions.Right;
        labelTmp.font      = font;
        labelTmp.text      = label;
        var labelLE = labelGO.AddComponent<LayoutElement>();
        labelLE.minWidth      = LabelWidth;
        labelLE.preferredWidth = LabelWidth;
        labelLE.flexibleWidth  = 0f;
        labelLE.minHeight      = BarHeight;
        labelLE.preferredHeight = BarHeight;

        // 바 배경
        var barGO = new GameObject("Bar", typeof(RectTransform));
        barGO.transform.SetParent(rowGO.transform, false);
        barGO.AddComponent<Image>().color = new Color(0.25f, 0.25f, 0.25f);
        var barRect = barGO.GetComponent<RectTransform>();
        barRect.sizeDelta = new Vector2(BarWidth, BarHeight);
        var barLE = barGO.AddComponent<LayoutElement>();
        barLE.minWidth = BarWidth; barLE.preferredWidth = BarWidth;
        barLE.minHeight = BarHeight; barLE.preferredHeight = BarHeight;

        // DimFill / BrightFill
        var dimRect    = CreateFill("DimFill",    barGO.transform, dim);
        var brightRect = CreateFill("BrightFill", barGO.transform, bright);

        // ValueText
        var valGO = new GameObject("ValueText", typeof(RectTransform));
        valGO.transform.SetParent(barGO.transform, false);
        var valTmp = valGO.AddComponent<TextMeshProUGUI>();
        valTmp.color     = Color.white;
        valTmp.fontSize  = BarFontSize;
        valTmp.alignment = TextAlignmentOptions.Left;
        valTmp.font      = font;
        valTmp.text      = "";
        var valRect = valGO.GetComponent<RectTransform>();
        valRect.anchorMin        = new Vector2(0f, 0f);
        valRect.anchorMax        = new Vector2(0f, 1f);
        valRect.pivot            = new Vector2(0f, 0.5f);
        valRect.anchoredPosition = new Vector2(6f, 0f);
        valRect.sizeDelta        = new Vector2(BarWidth - 10f, 0f);

        // StatBarUI
        var statBar = rowGO.AddComponent<StatBarUI>();
        var sso = new SerializedObject(statBar);
        sso.FindProperty("dimFill").objectReferenceValue    = dimRect;
        sso.FindProperty("brightFill").objectReferenceValue = brightRect;
        sso.FindProperty("valueText").objectReferenceValue  = valTmp;
        sso.ApplyModifiedProperties();

        return statBar;
    }

    static RectTransform CreateFill(string name, Transform parent, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        go.AddComponent<Image>().color = color;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot     = new Vector2(0f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return rect;
    }
}
