using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class PhaseReportPrefabBuilder : EditorWindow
{
    private const float BottomOffset = 138f; // 대사창 130px + 여백 8px
    private const float RightOffset  = 558f; // 로그창 550px + 여백 8px

    private TMP_FontAsset font;

    [MenuItem("Tools/페이즈 리포트 프리팹 생성")]
    static void Open() => GetWindow<PhaseReportPrefabBuilder>("페이즈 리포트 빌더");

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
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            var cgo = new GameObject("Canvas");
            canvas  = cgo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            cgo.AddComponent<CanvasScaler>();
            cgo.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(cgo, "Create PhaseReport Canvas");
        }

        // ── 루트 패널 ─────────────────────────────────────────────────
        // 대사창(하단)과 로그창(우측)을 제외한 나머지 영역을 채움
        var rootRect          = CreateRect("PhaseReportPanel", canvas.transform);
        rootRect.anchorMin    = Vector2.zero;
        rootRect.anchorMax    = Vector2.one;
        rootRect.offsetMin    = new Vector2(8f, BottomOffset);
        rootRect.offsetMax    = new Vector2(-RightOffset, -8f);

        var rootGO = rootRect.gameObject;
        var bg     = rootGO.AddComponent<Image>();
        bg.color   = new Color(0.04f, 0.04f, 0.08f, 0.92f);

        var rootVg                    = rootGO.AddComponent<VerticalLayoutGroup>();
        rootVg.padding                = new RectOffset(0, 0, 0, 0);
        rootVg.spacing                = 0f;
        rootVg.childForceExpandWidth  = true;
        rootVg.childForceExpandHeight = false;
        rootVg.childControlWidth      = true;
        rootVg.childControlHeight     = true;

        Undo.RegisterCreatedObjectUndo(rootGO, "Create PhaseReportPanel");

        // ── 헤더 ──────────────────────────────────────────────────────
        var headerGO = new GameObject("Header", typeof(RectTransform));
        headerGO.transform.SetParent(rootRect, false);

        var headerBg   = headerGO.AddComponent<Image>();
        headerBg.color = new Color(0.10f, 0.10f, 0.18f, 1f);

        var headerLE             = headerGO.AddComponent<LayoutElement>();
        headerLE.minHeight       = 44f;
        headerLE.preferredHeight = 44f;
        headerLE.flexibleHeight  = 0f;

        var headerHg                    = headerGO.AddComponent<HorizontalLayoutGroup>();
        headerHg.padding                = new RectOffset(12, 12, 6, 6);
        headerHg.childForceExpandHeight = true;
        headerHg.childControlWidth      = true;
        headerHg.childControlHeight     = true;
        headerHg.childAlignment         = TextAnchor.MiddleCenter;

        var titleTmp       = CreateTMP("TitleText", headerGO.transform, 22f, font);
        titleTmp.text      = "페이즈 리포트  <size=70%><color=#AAAAAA>클릭하면 닫힙니다</color></size>";
        titleTmp.fontStyle = FontStyles.Bold;
        titleTmp.alignment = TextAlignmentOptions.Center;
        titleTmp.color     = new Color(0.9f, 0.9f, 1.0f);
        var titleLE        = titleTmp.gameObject.AddComponent<LayoutElement>();
        titleLE.flexibleWidth = 1f;

        // ── 구분선 ────────────────────────────────────────────────────
        var divGO  = new GameObject("Divider", typeof(RectTransform));
        divGO.transform.SetParent(rootRect, false);
        var divImg = divGO.AddComponent<Image>();
        divImg.color = new Color(0.3f, 0.3f, 0.5f, 0.8f);
        var divLE  = divGO.AddComponent<LayoutElement>();
        divLE.minHeight       = 1f;
        divLE.preferredHeight = 1f;
        divLE.flexibleHeight  = 0f;

        // ── ScrollView ────────────────────────────────────────────────
        var scrollGO = new GameObject("ScrollView", typeof(RectTransform));
        scrollGO.transform.SetParent(rootRect, false);

        var scrollLE            = scrollGO.AddComponent<LayoutElement>();
        scrollLE.flexibleHeight = 1f;

        var scrollRect               = scrollGO.AddComponent<ScrollRect>();
        scrollRect.horizontal        = false;
        scrollRect.vertical          = true;
        scrollRect.scrollSensitivity = 30f;
        scrollRect.movementType      = ScrollRect.MovementType.Clamped;

        // Viewport
        var vpGO = new GameObject("Viewport", typeof(RectTransform));
        vpGO.transform.SetParent(scrollGO.transform, false);
        vpGO.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.01f);
        vpGO.AddComponent<Mask>().showMaskGraphic = false;
        var vpRect       = vpGO.GetComponent<RectTransform>();
        vpRect.anchorMin = Vector2.zero;
        vpRect.anchorMax = Vector2.one;
        vpRect.offsetMin = Vector2.zero;
        vpRect.offsetMax = Vector2.zero;
        scrollRect.viewport = vpRect;

        // Content — GridLayoutGroup으로 카드를 격자 배치
        var contentGO = new GameObject("Content", typeof(RectTransform));
        contentGO.transform.SetParent(vpGO.transform, false);

        var grid             = contentGO.AddComponent<GridLayoutGroup>();
        grid.padding         = new RectOffset(8, 8, 8, 8);
        grid.spacing         = new Vector2(8f, 8f);
        grid.cellSize        = new Vector2(240f, 340f); // 세로형 카드
        grid.constraint      = GridLayoutGroup.Constraint.Flexible;
        grid.childAlignment  = TextAnchor.UpperLeft;

        var contentCsf           = contentGO.AddComponent<ContentSizeFitter>();
        contentCsf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        contentCsf.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;

        var contentRect       = contentGO.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot     = new Vector2(0.5f, 1f);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;
        scrollRect.content    = contentRect;

        // ── PhaseReportUI 컴포넌트 연결 ───────────────────────────────
        var reportUI = rootGO.AddComponent<PhaseReportUI>();
        var uiSO     = new SerializedObject(reportUI);
        uiSO.FindProperty("panelRoot").objectReferenceValue      = rootGO;
        uiSO.FindProperty("cardContainer").objectReferenceValue  = contentRect;
        uiSO.FindProperty("scrollRect").objectReferenceValue     = scrollRect;
        uiSO.ApplyModifiedProperties();

        // ── 카드 프리팹 ───────────────────────────────────────────────
        var cardGO  = new GameObject("BallReportCard", typeof(RectTransform));
        var cardBg  = cardGO.AddComponent<Image>();
        cardBg.color = new Color(0.08f, 0.08f, 0.14f, 1f);

        // GridLayoutGroup이 카드 크기를 결정하므로 ContentSizeFitter 불필요
        var cardVg                    = cardGO.AddComponent<VerticalLayoutGroup>();
        cardVg.padding                = new RectOffset(10, 10, 10, 10);
        cardVg.spacing                = 4f;
        cardVg.childForceExpandWidth  = true;
        cardVg.childForceExpandHeight = false;
        cardVg.childControlWidth      = true;
        cardVg.childControlHeight     = true;

        // 카드 이름 텍스트
        var nameTmp        = CreateTMP("NameText", cardGO.transform, 19f, font);
        nameTmp.color      = new Color(1f, 0.95f, 0.75f);
        nameTmp.fontStyle  = FontStyles.Bold;
        nameTmp.enableWordWrapping = false;
        var nameLE         = nameTmp.gameObject.AddComponent<LayoutElement>();
        nameLE.flexibleWidth = 1f;

        // 카드 스탯 텍스트
        var statsTmp       = CreateTMP("StatsText", cardGO.transform, 16f, font);
        statsTmp.color     = new Color(0.85f, 0.85f, 0.85f);
        statsTmp.enableWordWrapping = false;
        var statsLE        = statsTmp.gameObject.AddComponent<LayoutElement>();
        statsLE.flexibleWidth = 1f;

        // BallReportCard 컴포넌트 연결
        var reportCard = cardGO.AddComponent<BallReportCard>();
        var cardSO     = new SerializedObject(reportCard);
        cardSO.FindProperty("nameText").objectReferenceValue  = nameTmp;
        cardSO.FindProperty("statsText").objectReferenceValue = statsTmp;
        cardSO.ApplyModifiedProperties();

        // ── 저장 ──────────────────────────────────────────────────────
        const string dir = "Assets/03. Prefab";
        if (!AssetDatabase.IsValidFolder(dir))
            AssetDatabase.CreateFolder("Assets", "03. Prefab");

        const string cardPath  = dir + "/BallReportCard.prefab";
        const string panelPath = dir + "/PhaseReportPanel.prefab";

        var cardPrefab = PrefabUtility.SaveAsPrefabAsset(cardGO, cardPath);
        Object.DestroyImmediate(cardGO);

        // cardPrefab 연결
        uiSO.Update();
        uiSO.FindProperty("cardPrefab").objectReferenceValue = cardPrefab;
        uiSO.ApplyModifiedProperties();

        PrefabUtility.SaveAsPrefabAssetAndConnect(rootGO, panelPath, InteractionMode.AutomatedAction);

        Selection.activeGameObject = rootGO;
        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<GameObject>(panelPath));
        Debug.Log($"[PhaseReportPrefabBuilder] 완료 → {panelPath}");
    }

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
        var tmp      = go.AddComponent<TextMeshProUGUI>();
        tmp.color    = Color.white;
        tmp.fontSize = size;
        tmp.font     = font;
        tmp.text     = "";
        tmp.richText = true;
        return tmp;
    }
}
