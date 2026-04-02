using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public static class TutorialUIBuilder
{
    const string PrefabPath = "Assets/03. Prefab/TutorialUI.prefab";

    [MenuItem("Tools/튜토리얼 UI 프리팹 생성")]
    public static void Build()
    {
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/98. Font/Pretendard-Light SDF.asset");

        // ── 루트 Canvas ───────────────────────────────────────────
        var root   = new GameObject("TutorialUI");
        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;
        var scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        root.AddComponent<GraphicRaycaster>();

        // ── 패널 (하단 중앙 플로팅) ────────────────────────────────
        var panel     = new GameObject("Panel", typeof(RectTransform));
        panel.transform.SetParent(root.transform, false);
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin        = new Vector2(0.05f, 0f);
        panelRect.anchorMax        = new Vector2(0.95f, 0f);
        panelRect.pivot            = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = new Vector2(0f, 20f);
        panelRect.sizeDelta        = new Vector2(0f, 150f);
        panel.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.1f, 0.93f);

        // ── 단계 레이블 (좌상단) ──────────────────────────────────
        var stepGO   = new GameObject("StepLabel", typeof(RectTransform));
        stepGO.transform.SetParent(panel.transform, false);
        var stepRect = stepGO.GetComponent<RectTransform>();
        stepRect.anchorMin        = new Vector2(0f, 1f);
        stepRect.anchorMax        = new Vector2(0f, 1f);
        stepRect.pivot            = new Vector2(0f, 1f);
        stepRect.anchoredPosition = new Vector2(20f, -10f);
        stepRect.sizeDelta        = new Vector2(220f, 28f);
        var stepTMP               = stepGO.AddComponent<TextMeshProUGUI>();
        stepTMP.text      = "튜토리얼 1 / 4";
        stepTMP.fontSize   = 17f;
        stepTMP.color      = new Color(0.6f, 0.6f, 0.6f, 1f);
        stepTMP.alignment  = TextAlignmentOptions.MidlineLeft;
        stepTMP.raycastTarget = false;
        if (font) stepTMP.font = font;

        // ── 지시 텍스트 ───────────────────────────────────────────
        var instrGO   = new GameObject("InstructionText", typeof(RectTransform));
        instrGO.transform.SetParent(panel.transform, false);
        var instrRect = instrGO.GetComponent<RectTransform>();
        instrRect.anchorMin = new Vector2(0f, 0f);
        instrRect.anchorMax = new Vector2(1f, 1f);
        instrRect.offsetMin = new Vector2(20f, 10f);
        instrRect.offsetMax = new Vector2(-170f, -42f);
        var instrTMP        = instrGO.AddComponent<TextMeshProUGUI>();
        instrTMP.text      = "";
        instrTMP.fontSize   = 23f;
        instrTMP.color      = Color.white;
        instrTMP.alignment  = TextAlignmentOptions.MidlineLeft;
        instrTMP.enableWordWrapping = true;
        instrTMP.raycastTarget = false;
        if (font) instrTMP.font = font;

        // ── 확인 버튼 (우측, 기본 비활성) ────────────────────────
        var btnGO   = new GameObject("ConfirmButton", typeof(RectTransform));
        btnGO.transform.SetParent(panel.transform, false);
        var btnRect = btnGO.GetComponent<RectTransform>();
        btnRect.anchorMin        = new Vector2(1f, 0.5f);
        btnRect.anchorMax        = new Vector2(1f, 0.5f);
        btnRect.pivot            = new Vector2(1f, 0.5f);
        btnRect.anchoredPosition = new Vector2(-20f, 0f);
        btnRect.sizeDelta        = new Vector2(140f, 64f);
        var btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.2f, 0.6f, 0.3f, 1f);
        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = btnImg;

        var lblGO   = new GameObject("Label", typeof(RectTransform));
        lblGO.transform.SetParent(btnGO.transform, false);
        var lblRect = lblGO.GetComponent<RectTransform>();
        lblRect.anchorMin = Vector2.zero;
        lblRect.anchorMax = Vector2.one;
        lblRect.offsetMin = Vector2.zero;
        lblRect.offsetMax = Vector2.zero;
        var lblTMP  = lblGO.AddComponent<TextMeshProUGUI>();
        lblTMP.text           = "완료";
        lblTMP.fontSize        = 22f;
        lblTMP.color           = Color.white;
        lblTMP.alignment       = TextAlignmentOptions.Center;
        lblTMP.raycastTarget   = false;
        if (font) lblTMP.font = font;

        btnGO.SetActive(false);

        // ── TutorialManager 연결 ──────────────────────────────────
        var manager = root.AddComponent<TutorialManager>();
        SetField(manager, "panel",           panel);
        SetField(manager, "stepLabel",       stepTMP);
        SetField(manager, "instructionText", instrTMP);
        SetField(manager, "confirmButton",   btn);

        // ── 저장 ─────────────────────────────────────────────────
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        Debug.Log($"[TutorialUIBuilder] 저장: {PrefabPath}");
        EditorGUIUtility.PingObject(prefab);
    }

    static void SetField(object obj, string name, object value)
    {
        var f = obj.GetType().GetField(name,
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);
        f?.SetValue(obj, value);
    }
}
