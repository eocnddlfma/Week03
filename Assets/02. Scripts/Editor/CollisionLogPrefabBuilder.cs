using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class CollisionLogPrefabBuilder : EditorWindow
{
    private const float PanelWidth  = 550f;
    private const float HeaderHeight = 44f;

    private TMP_FontAsset font;

    [MenuItem("Tools/충돌 로그 프리팹 생성")]
    static void Open() => GetWindow<CollisionLogPrefabBuilder>("충돌 로그 빌더");

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

    static void Build(TMP_FontAsset font)
    {
        // ── Canvas 확보 ───────────────────────────────────────────────
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            var cgo = new GameObject("Canvas");
            canvas  = cgo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            cgo.AddComponent<CanvasScaler>();
            cgo.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(cgo, "Create CollisionLog Canvas");
        }

        // ── 루트 패널 ─────────────────────────────────────────────────
        // 화면 오른쪽 전체 높이에 고정
        var rootRect = CreateRect("CollisionLogPanel", canvas.transform);
        rootRect.anchorMin        = new Vector2(1f, 0f);
        rootRect.anchorMax        = new Vector2(1f, 1f);
        rootRect.pivot            = new Vector2(1f, 0.5f);
        rootRect.offsetMin        = new Vector2(-PanelWidth - 8f, 8f);
        rootRect.offsetMax        = new Vector2(-8f, -8f);

        var rootGO   = rootRect.gameObject;
        var cg       = rootGO.AddComponent<CanvasGroup>();
        cg.alpha     = 0.85f;

        // 반투명 배경
        var bg      = rootGO.AddComponent<Image>();
        bg.color    = new Color(0.05f, 0.05f, 0.05f, 0.82f);

        // VerticalLayoutGroup: Header + ScrollView 세로 배치
        var rootVg                    = rootGO.AddComponent<VerticalLayoutGroup>();
        rootVg.padding                = new RectOffset(0, 0, 0, 0);
        rootVg.spacing                = 0f;
        rootVg.childForceExpandWidth  = true;
        rootVg.childForceExpandHeight = false;
        rootVg.childControlWidth      = true;
        rootVg.childControlHeight     = true;

        Undo.RegisterCreatedObjectUndo(rootGO, "Create CollisionLogPanel");

        // ── 헤더 ──────────────────────────────────────────────────────
        var headerGO = new GameObject("Header", typeof(RectTransform));
        headerGO.transform.SetParent(rootRect, false);

        var headerBg   = headerGO.AddComponent<Image>();
        headerBg.color = new Color(0.12f, 0.12f, 0.12f, 1f);

        var headerHg                    = headerGO.AddComponent<HorizontalLayoutGroup>();
        headerHg.padding                = new RectOffset(8, 4, 4, 4);
        headerHg.spacing                = 4f;
        headerHg.childForceExpandWidth  = false;
        headerHg.childForceExpandHeight = true;
        headerHg.childControlWidth      = true;
        headerHg.childControlHeight     = true;
        headerHg.childAlignment         = TextAnchor.MiddleLeft;

        var headerLE             = headerGO.AddComponent<LayoutElement>();
        headerLE.minHeight       = HeaderHeight;
        headerLE.preferredHeight = HeaderHeight;
        headerLE.flexibleHeight  = 0f;

        // 제목 텍스트
        var titleTmp        = CreateTMP("TitleText", headerGO.transform, 20f, font);
        titleTmp.text       = "충돌 로그";
        titleTmp.fontStyle  = FontStyles.Bold;
        titleTmp.color      = new Color(0.85f, 0.85f, 0.85f);
        var titleLE         = titleTmp.gameObject.AddComponent<LayoutElement>();
        titleLE.flexibleWidth = 1f;

        // 접기 버튼
        var btnGO  = new GameObject("CollapseButton", typeof(RectTransform));
        btnGO.transform.SetParent(headerGO.transform, false);
        var btnImg  = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.25f, 0.25f, 0.25f);
        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        var btnLE             = btnGO.AddComponent<LayoutElement>();
        btnLE.minWidth        = 48f;
        btnLE.preferredWidth  = 48f;
        btnLE.flexibleWidth   = 0f;

        var btnLabelTmp = CreateTMP("BtnLabel", btnGO.transform, 16f, null); // 기본 폰트 - 특수문자 렌더링용
        btnLabelTmp.text      = "-";
        btnLabelTmp.alignment = TextAlignmentOptions.Center;
        var btnLabelRect      = btnLabelTmp.GetComponent<RectTransform>();
        btnLabelRect.anchorMin = Vector2.zero;
        btnLabelRect.anchorMax = Vector2.one;
        btnLabelRect.offsetMin = Vector2.zero;
        btnLabelRect.offsetMax = Vector2.zero;

        // ── ScrollView ────────────────────────────────────────────────
        var scrollGO = new GameObject("ScrollView", typeof(RectTransform));
        scrollGO.transform.SetParent(rootRect, false);

        var scrollLE            = scrollGO.AddComponent<LayoutElement>();
        scrollLE.flexibleHeight = 1f;

        var scrollRect    = scrollGO.AddComponent<ScrollRect>();
        scrollRect.horizontal        = false;
        scrollRect.vertical          = true;
        scrollRect.scrollSensitivity = 30f;
        scrollRect.movementType      = ScrollRect.MovementType.Clamped;

        // Viewport
        var vpGO   = new GameObject("Viewport", typeof(RectTransform));
        vpGO.transform.SetParent(scrollGO.transform, false);
        vpGO.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.01f); // Mask용 더미 이미지
        vpGO.AddComponent<Mask>().showMaskGraphic = false;
        var vpRect            = vpGO.GetComponent<RectTransform>();
        vpRect.anchorMin      = Vector2.zero;
        vpRect.anchorMax      = Vector2.one;
        vpRect.offsetMin      = Vector2.zero;
        vpRect.offsetMax      = Vector2.zero;
        scrollRect.viewport   = vpRect;

        // Content
        var contentGO = new GameObject("Content", typeof(RectTransform));
        contentGO.transform.SetParent(vpGO.transform, false);

        var contentVg                    = contentGO.AddComponent<VerticalLayoutGroup>();
        contentVg.padding                = new RectOffset(6, 6, 4, 4);
        contentVg.spacing                = 1f;
        contentVg.childForceExpandWidth  = true;
        contentVg.childForceExpandHeight = false;
        contentVg.childControlWidth      = true;
        contentVg.childControlHeight     = true;

        var contentCsf            = contentGO.AddComponent<ContentSizeFitter>();
        contentCsf.horizontalFit  = ContentSizeFitter.FitMode.Unconstrained;
        contentCsf.verticalFit    = ContentSizeFitter.FitMode.PreferredSize;

        var contentRect           = contentGO.GetComponent<RectTransform>();
        contentRect.anchorMin     = new Vector2(0f, 1f);
        contentRect.anchorMax     = new Vector2(1f, 1f);
        contentRect.pivot         = new Vector2(0.5f, 1f);
        contentRect.offsetMin     = Vector2.zero;
        contentRect.offsetMax     = Vector2.zero;
        scrollRect.content        = contentRect;

        // ── CollisionLogUI 컴포넌트 연결 ──────────────────────────────
        var logUI    = rootGO.AddComponent<CollisionLogUI>();
        var uiSO     = new SerializedObject(logUI);
        uiSO.FindProperty("scrollRect").objectReferenceValue    = scrollRect;
        uiSO.FindProperty("contentParent").objectReferenceValue = contentRect;
        uiSO.FindProperty("canvasGroup").objectReferenceValue   = cg;
        uiSO.ApplyModifiedProperties();

        // 버튼 → ToggleCollapse 연결
        var entry = new UnityEngine.Events.UnityAction(logUI.ToggleCollapse);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, entry);

        // ── LogLine 프리팹 ────────────────────────────────────────────
        var lineGO   = new GameObject("LogLine", typeof(RectTransform));
        var lineTmp  = lineGO.AddComponent<TextMeshProUGUI>();
        lineTmp.font      = font;
        lineTmp.fontSize  = 20f;
        lineTmp.color     = Color.white;
        lineTmp.richText  = true;
        lineTmp.enableWordWrapping = true;
        lineTmp.text      = "";

        var lineLE            = lineGO.AddComponent<LayoutElement>();
        lineLE.flexibleWidth  = 1f;

        // ── 저장 ──────────────────────────────────────────────────────
        const string dir = "Assets/03. Prefab";
        if (!AssetDatabase.IsValidFolder(dir))
            AssetDatabase.CreateFolder("Assets", "03. Prefab");

        const string linePath  = dir + "/LogLine.prefab";
        const string panelPath = dir + "/CollisionLogPanel.prefab";

        // LogLine 프리팹 먼저 저장
        var linePrefab = PrefabUtility.SaveAsPrefabAsset(lineGO, linePath);
        Object.DestroyImmediate(lineGO);

        // logLinePrefab 연결
        uiSO.Update();
        uiSO.FindProperty("logLinePrefab").objectReferenceValue = linePrefab;
        uiSO.ApplyModifiedProperties();

        // CollisionLogPanel 프리팹 저장
        PrefabUtility.SaveAsPrefabAssetAndConnect(rootGO, panelPath, InteractionMode.AutomatedAction);

        Selection.activeGameObject = rootGO;
        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<GameObject>(panelPath));
        Debug.Log($"[CollisionLogPrefabBuilder] 완료 → {panelPath}");
    }

    // ── 헬퍼 ──────────────────────────────────────────────────────────
    static RectTransform CreateRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
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
}
