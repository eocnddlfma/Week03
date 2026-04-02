using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEditor;

public static class CharacterSelectUIBuilder
{
    const string PrefabPath = "Assets/03. Prefab/CharacterSelectUI.prefab";

    // ── 색상 (명도 대비 명확하게) ──────────────────────────────────
    static readonly Color BgColor      = new Color(0.05f, 0.05f, 0.05f, 1f);   // 거의 검정
    static readonly Color PanelColor   = new Color(0.22f, 0.22f, 0.22f, 1f);   // 회색
    static readonly Color StatRowBg    = new Color(0.15f, 0.15f, 0.15f, 1f);   // 어두운 회색 (행 배경)
    static readonly Color SliderBg     = new Color(0.10f, 0.10f, 0.10f, 1f);   // 슬라이더 배경
    static readonly Color SliderFill   = new Color(0.25f, 0.65f, 1.00f, 1f);   // 파랑
    static readonly Color TraitBg      = new Color(0.75f, 0.35f, 0.00f, 1f);   // 주황
    static readonly Color TooltipBg    = new Color(0.08f, 0.08f, 0.08f, 0.96f);
    static readonly Color RerollColor  = new Color(0.10f, 0.80f, 0.30f, 1f);   // 밝은 초록
    static readonly Color StartColor   = new Color(0.75f, 0.20f, 1.00f, 1f);   // 밝은 보라

    [MenuItem("Tools/캐릭터 선택 UI 프리팹 생성")]
    static void Build()
    {
        var font = AssetDatabase.FindAssets("t:TMP_FontAsset Pretendard") is { Length: > 0 } guids
            ? AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guids[0]))
            : null;

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            var cgo = new GameObject("Canvas");
            canvas  = cgo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            cgo.AddComponent<CanvasScaler>();
            cgo.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(cgo, "CharSelectUI");
        }

        // EventSystem 없으면 생성 (없으면 버튼 클릭이 전혀 안 됨)
        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<StandaloneInputModule>();
            Undo.RegisterCreatedObjectUndo(esGO, "CharSelectUI EventSystem");
        }

        // ── 루트 ──────────────────────────────────────────────────
        var root = new GameObject("CharacterSelectUI", typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);
        Stretch(root.GetComponent<RectTransform>());
        root.AddComponent<Image>().color = BgColor;
        var manager = root.AddComponent<CharacterSelectManager>();

        // ── 중앙 패널 ──────────────────────────────────────────────
        var panel     = MakeRect("Panel", root.transform);
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.sizeDelta        = new Vector2(540, 700);
        panelRect.anchorMin        = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax        = new Vector2(0.5f, 0.5f);
        panelRect.pivot            = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panel.AddComponent<Image>().color = PanelColor;
        var vg = panel.AddComponent<VerticalLayoutGroup>();
        vg.padding                = new RectOffset(20, 20, 18, 18);
        vg.spacing                = 12f;
        vg.childForceExpandWidth  = true;
        vg.childForceExpandHeight = false;
        vg.childControlWidth      = true;
        vg.childControlHeight     = true;

        // 제목
        var title = MakeTMP("Title", panel.transform, "캐릭터 선택", 34f, font);
        title.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        title.GetComponent<TextMeshProUGUI>().color     = Color.white;
        AddLE(title, preferredHeight: 46f);

        MakeDivider(panel.transform);

        // ── 스탯 섹션 ──────────────────────────────────────────────
        var statsHdr = MakeTMP("StatsHeader", panel.transform, "플레이어 스탯", 18f, font);
        statsHdr.GetComponent<TextMeshProUGUI>().color = new Color(0.65f, 0.85f, 1f);
        AddLE(statsHdr, preferredHeight: 26f);

        var (sld0, val0) = MakeStatRow("심리학 지식(치는 힘)",  panel.transform, font);
        var (sld1, val1) = MakeStatRow("공감능력(유지시간)",    panel.transform, font);
        var (sld2, val2) = MakeStatRow("인내심(바운더리 크기)", panel.transform, font);
        var (sld3, val3) = MakeStatRow("성향(바운더리 각)",     panel.transform, font);

        foreach (var s in new[] { sld0, sld1, sld2, sld3 })
        {
            var fill = s.fillRect?.GetComponent<Image>();
            if (fill) fill.color = SliderFill;
        }

        MakeDivider(panel.transform);

        // ── 특성 섹션 ──────────────────────────────────────────────
        var traitSection = MakeRect("TraitSection", panel.transform);
        traitSection.AddComponent<Image>().color = TraitBg;
        var tvg = traitSection.AddComponent<VerticalLayoutGroup>();
        tvg.padding                = new RectOffset(14, 14, 10, 12);
        tvg.spacing                = 8f;
        tvg.childForceExpandWidth  = true;
        tvg.childForceExpandHeight = false;
        tvg.childControlWidth      = true;
        tvg.childControlHeight     = true;
        traitSection.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var traitHdr = MakeTMP("TraitHeader", traitSection.transform, "특성 (0~2개)", 17f, font);
        traitHdr.GetComponent<TextMeshProUGUI>().color = new Color(1f, 0.92f, 0.7f);
        AddLE(traitHdr, preferredHeight: 24f);

        // 뱃지 컨테이너 (런타임에 뱃지를 여기에 생성)
        var traitContainer = MakeRect("TraitContainer", traitSection.transform);
        var thg = traitContainer.AddComponent<HorizontalLayoutGroup>();
        thg.spacing                = 10f;
        thg.childForceExpandWidth  = false;
        thg.childForceExpandHeight = false;
        thg.childControlWidth      = true;
        thg.childControlHeight     = true;
        thg.padding                = new RectOffset(0, 0, 0, 0);
        traitContainer.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        AddLE(traitContainer, preferredHeight: 44f);

        // ── 버튼 행 ────────────────────────────────────────────────
        var btnRow = MakeRect("ButtonRow", panel.transform);
        var bhg = btnRow.AddComponent<HorizontalLayoutGroup>();
        bhg.spacing                = 14f;
        bhg.childForceExpandWidth  = true;
        bhg.childForceExpandHeight = true;   // ← 버튼이 행 높이를 꽉 채우도록
        bhg.childControlWidth      = true;
        bhg.childControlHeight     = true;
        AddLE(btnRow, preferredHeight: 60f);

        var rerollBtn = MakeButton("RerollButton", btnRow.transform, "다시 굴리기", 22f, RerollColor, font);
        var startBtn  = MakeButton("StartButton",  btnRow.transform, "게임 시작",   22f, StartColor,  font);

        // ── 툴팁 패널 (루트 직속 — 항상 최상위 렌더링) ────────────────
        var tipPanel = MakeRect("TraitTooltip", root.transform);
        var tipRect  = tipPanel.GetComponent<RectTransform>();
        tipRect.sizeDelta        = new Vector2(280, 80);
        tipRect.anchorMin        = new Vector2(0f, 0f);
        tipRect.anchorMax        = new Vector2(0f, 0f);
        tipRect.pivot            = new Vector2(0.5f, 0f);
        tipPanel.AddComponent<Image>().color = TooltipBg;
        var tipVG = tipPanel.AddComponent<VerticalLayoutGroup>();
        tipVG.padding  = new RectOffset(12, 12, 8, 8);
        tipVG.spacing  = 4f;
        tipVG.childForceExpandWidth  = true;
        tipVG.childForceExpandHeight = false;
        tipVG.childControlWidth  = true;
        tipVG.childControlHeight = true;
        tipPanel.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var tipName = MakeTMP("TooltipName", tipPanel.transform, "", 20f, font);
        tipName.GetComponent<TextMeshProUGUI>().color     = new Color(1f, 0.9f, 0.5f);
        tipName.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        AddLE(tipName, preferredHeight: 28f);

        var tipDesc = MakeTMP("TooltipDesc", tipPanel.transform, "", 16f, font);
        tipDesc.GetComponent<TextMeshProUGUI>().color = Color.white;
        AddLE(tipDesc, preferredHeight: 22f);

        tipPanel.SetActive(false);

        // ── 참조 연결 ──────────────────────────────────────────────
        var so = new SerializedObject(manager);
        so.FindProperty("shootPowerSlider")    .objectReferenceValue = sld0;
        so.FindProperty("sustainTimeSlider")   .objectReferenceValue = sld1;
        so.FindProperty("boundarySizeSlider")  .objectReferenceValue = sld2;
        so.FindProperty("boundaryCornersSlider").objectReferenceValue = sld3;
        so.FindProperty("shootPowerValue")     .objectReferenceValue = val0.GetComponent<TextMeshProUGUI>();
        so.FindProperty("sustainTimeValue")    .objectReferenceValue = val1.GetComponent<TextMeshProUGUI>();
        so.FindProperty("boundarySizeValue")   .objectReferenceValue = val2.GetComponent<TextMeshProUGUI>();
        so.FindProperty("boundaryCornersValue").objectReferenceValue = val3.GetComponent<TextMeshProUGUI>();
        so.FindProperty("traitContainer")      .objectReferenceValue = traitContainer.transform;
        so.FindProperty("tooltipPanel")        .objectReferenceValue = tipRect;
        so.FindProperty("tooltipNameText")     .objectReferenceValue = tipName.GetComponent<TextMeshProUGUI>();
        so.FindProperty("tooltipDescText")     .objectReferenceValue = tipDesc.GetComponent<TextMeshProUGUI>();
        so.FindProperty("rerollButton")        .objectReferenceValue = rerollBtn;
        so.FindProperty("startButton")         .objectReferenceValue = startBtn;
        so.FindProperty("badgeFont")           .objectReferenceValue = font;
        so.ApplyModifiedProperties();

        // ── 프리팹 저장 ────────────────────────────────────────────
        const string dir = "Assets/03. Prefab";
        if (!AssetDatabase.IsValidFolder(dir))
            AssetDatabase.CreateFolder("Assets", "03. Prefab");
        PrefabUtility.SaveAsPrefabAssetAndConnect(root, PrefabPath, InteractionMode.AutomatedAction);

        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
        Debug.Log($"[CharSelectUIBuilder] 완료 → {PrefabPath}");
    }

    // ── 헬퍼 ──────────────────────────────────────────────────────
    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    static GameObject MakeRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    static GameObject MakeTMP(string name, Transform parent, string text, float size, TMP_FontAsset font)
    {
        var go  = MakeRect(name, parent);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text     = text;
        tmp.fontSize = size;
        tmp.color    = Color.white;
        if (font != null) tmp.font = font;
        return go;
    }

    static void AddLE(GameObject go, float preferredHeight = -1f, float preferredWidth = -1f)
    {
        var le = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
        if (preferredHeight >= 0f) le.preferredHeight = preferredHeight;
        if (preferredWidth  >= 0f) le.preferredWidth  = preferredWidth;
    }

    static void MakeDivider(Transform parent)
    {
        var go = MakeRect("Divider", parent);
        go.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.12f);
        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 1f; le.flexibleWidth = 1f;
    }

    static (Slider, GameObject) MakeStatRow(string label, Transform parent, TMP_FontAsset font)
    {
        // 행 배경
        var row = MakeRect(label + "Row", parent);
        row.AddComponent<Image>().color = StatRowBg;
        var hg = row.AddComponent<HorizontalLayoutGroup>();
        hg.spacing                = 10f;
        hg.padding                = new RectOffset(10, 10, 0, 0);
        hg.childForceExpandWidth  = false;
        hg.childForceExpandHeight = false;
        hg.childControlWidth      = true;
        hg.childControlHeight     = true;
        hg.childAlignment         = TextAnchor.MiddleLeft;
        AddLE(row, preferredHeight: 38f);

        // 레이블 (고정 너비)
        var labelGO = MakeTMP(label + "Label", row.transform, label, 17f, font);
        var lTMP    = labelGO.GetComponent<TextMeshProUGUI>();
        lTMP.color               = new Color(0.85f, 0.85f, 0.85f);
        lTMP.enableWordWrapping  = false;
        lTMP.overflowMode        = TextOverflowModes.Ellipsis;
        var lLE = labelGO.AddComponent<LayoutElement>();
        lLE.minWidth = 170f; lLE.preferredWidth = 170f; lLE.flexibleWidth = 0f;

        // 슬라이더 (남은 공간 전부)
        var sliderGO = MakeRect(label + "Slider", row.transform);
        var slider   = sliderGO.AddComponent<Slider>();
        var sLE = sliderGO.AddComponent<LayoutElement>();
        sLE.flexibleWidth = 1f; sLE.minHeight = 14f; sLE.preferredHeight = 14f;

        var bg = MakeRect("Background", sliderGO.transform);
        bg.AddComponent<Image>().color = SliderBg;
        Stretch(bg.GetComponent<RectTransform>());

        var fillArea = MakeRect("Fill Area", sliderGO.transform);
        Stretch(fillArea.GetComponent<RectTransform>());
        var fill = MakeRect("Fill", fillArea.transform);
        fill.AddComponent<Image>().color = SliderFill;
        var fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero; fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero; fillRect.offsetMax = Vector2.zero;

        slider.fillRect      = fillRect;
        slider.targetGraphic = fill.GetComponent<Image>();
        slider.interactable  = false;

        // 값 텍스트 (고정 너비)
        var valGO  = MakeTMP(label + "Value", row.transform, "", 17f, font);
        var vTMP   = valGO.GetComponent<TextMeshProUGUI>();
        vTMP.alignment        = TextAlignmentOptions.Right;
        vTMP.color            = new Color(0.4f, 0.85f, 1f);
        vTMP.enableWordWrapping = false;
        var vLE = valGO.AddComponent<LayoutElement>();
        vLE.minWidth = 72f; vLE.preferredWidth = 72f; vLE.flexibleWidth = 0f;

        return (slider, valGO);
    }

    static Button MakeButton(string name, Transform parent, string text, float fontSize,
                             Color bgColor, TMP_FontAsset font)
    {
        var go  = MakeRect(name, parent);
        var img = go.AddComponent<Image>();
        img.color = bgColor;
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;  // 명시적 설정 — 없으면 클릭 응답 안 할 수 있음
        var cs = btn.colors;
        cs.normalColor      = bgColor;
        cs.highlightedColor = new Color(
            Mathf.Min(bgColor.r + 0.15f, 1f),
            Mathf.Min(bgColor.g + 0.15f, 1f),
            Mathf.Min(bgColor.b + 0.15f, 1f));
        cs.pressedColor = new Color(bgColor.r * 0.8f, bgColor.g * 0.8f, bgColor.b * 0.8f);
        btn.colors = cs;

        var lbl  = MakeRect(name + "Label", go.transform);
        Stretch(lbl.GetComponent<RectTransform>());
        var tmp  = lbl.AddComponent<TextMeshProUGUI>();
        tmp.text            = text;
        tmp.fontSize        = fontSize;
        tmp.color           = Color.white;
        tmp.fontStyle       = FontStyles.Bold;
        tmp.alignment       = TextAlignmentOptions.Center;
        tmp.raycastTarget   = false;  // 텍스트가 클릭 이벤트 가로채지 않도록
        if (font != null) tmp.font = font;

        return btn;
    }
}
