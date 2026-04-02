using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public static class StoryUIBuilder
{
    const string PrefabPath = "Assets/03. Prefab/StoryUI.prefab";

    [MenuItem("Tools/스토리 UI 프리팹 생성")]
    public static void Build()
    {
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/98. Font/Pretendard-Light SDF.asset");
        if (!font)
            font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");

        // ── 루트 Canvas ───────────────────────────────────────────
        var root     = new GameObject("StoryUI");
        var canvas   = root.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        root.AddComponent<GraphicRaycaster>();

        // ── 하단 대화 박스 ────────────────────────────────────────
        var box     = MakePanel(root.transform, "DialogueBox",
            new Vector2(0f, 0f), new Vector2(1f, 0f),
            new Color(0.08f, 0.08f, 0.12f, 0.92f));
        var boxRect = box.GetComponent<RectTransform>();
        boxRect.sizeDelta = new Vector2(0f, 220f);
        box.SetActive(false); // Awake에서 Hide() 호출 전 깜빡임 방지

        // ── 화자 이름 ─────────────────────────────────────────────
        var nameplate     = new GameObject("Nameplate", typeof(RectTransform));
        nameplate.transform.SetParent(box.transform, false);
        var nameplateRect = nameplate.GetComponent<RectTransform>();
        nameplateRect.anchorMin        = new Vector2(0f, 1f);
        nameplateRect.anchorMax        = new Vector2(0f, 1f);
        nameplateRect.pivot            = new Vector2(0f, 1f);
        nameplateRect.anchoredPosition = new Vector2(20f, 0f);
        nameplateRect.sizeDelta        = new Vector2(200f, 40f);
        nameplate.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.25f, 1f);

        var speakerGO   = new GameObject("SpeakerName", typeof(RectTransform));
        speakerGO.transform.SetParent(nameplate.transform, false);
        var speakerRect = speakerGO.GetComponent<RectTransform>();
        speakerRect.anchorMin = Vector2.zero;
        speakerRect.anchorMax = Vector2.one;
        speakerRect.offsetMin = new Vector2(10f, 0f);
        speakerRect.offsetMax = new Vector2(-10f, 0f);
        var speakerTMP        = speakerGO.AddComponent<TextMeshProUGUI>();
        speakerTMP.text      = "";
        speakerTMP.fontSize   = 22f;
        speakerTMP.color      = new Color(1f, 0.9f, 0.5f, 1f);
        speakerTMP.alignment  = TextAlignmentOptions.MidlineLeft;
        speakerTMP.raycastTarget = false;
        if (font) speakerTMP.font = font;

        // ── 대사 텍스트 ───────────────────────────────────────────
        var dialogueGO   = new GameObject("DialogueText", typeof(RectTransform));
        dialogueGO.transform.SetParent(box.transform, false);
        var dialogueRect = dialogueGO.GetComponent<RectTransform>();
        dialogueRect.anchorMin = new Vector2(0f, 0f);
        dialogueRect.anchorMax = new Vector2(1f, 1f);
        dialogueRect.offsetMin = new Vector2(20f, 10f);
        dialogueRect.offsetMax = new Vector2(-20f, -50f);
        var dialogueTMP        = dialogueGO.AddComponent<TextMeshProUGUI>();
        dialogueTMP.text      = "";
        dialogueTMP.fontSize   = 24f;
        dialogueTMP.color      = Color.white;
        dialogueTMP.alignment  = TextAlignmentOptions.TopLeft;
        dialogueTMP.raycastTarget      = false;
        dialogueTMP.enableWordWrapping = true;
        if (font) dialogueTMP.font = font;

        // ── ▼ 계속 표시 ───────────────────────────────────────────
        var contGO   = new GameObject("ContinueIndicator", typeof(RectTransform));
        contGO.transform.SetParent(box.transform, false);
        var contRect = contGO.GetComponent<RectTransform>();
        contRect.anchorMin        = new Vector2(1f, 0f);
        contRect.anchorMax        = new Vector2(1f, 0f);
        contRect.pivot            = new Vector2(1f, 0f);
        contRect.anchoredPosition = new Vector2(-20f, 14f);
        contRect.sizeDelta        = new Vector2(30f, 30f);
        var contTMP               = contGO.AddComponent<TextMeshProUGUI>();
        contTMP.text      = "▼";
        contTMP.fontSize   = 24f;
        contTMP.color      = new Color(1f, 0.9f, 0.3f, 1f);
        contTMP.alignment  = TextAlignmentOptions.Center;
        contTMP.raycastTarget = false;
        if (font) contTMP.font = font;
        contGO.SetActive(false);

        // ── 컴포넌트 연결 ─────────────────────────────────────────
        var storyBoxUI = root.AddComponent<StoryBoxUI>();
        SetField(storyBoxUI, "panel",             box);
        SetField(storyBoxUI, "speakerNameText",   speakerTMP);
        SetField(storyBoxUI, "dialogueText",      dialogueTMP);
        SetField(storyBoxUI, "continueIndicator", contGO);

        var storyManager = root.AddComponent<StoryManager>();
        SetField(storyManager, "ui", storyBoxUI);

        // ── 저장 ──────────────────────────────────────────────────
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        Debug.Log($"[StoryUIBuilder] 저장 완료: {PrefabPath}");
        EditorGUIUtility.PingObject(prefab);
    }

    static GameObject MakePanel(Transform parent, string name,
        Vector2 anchor00, Vector2 anchor11, Color color)
    {
        var go   = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchor00; rect.anchorMax = anchor11;
        rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        go.AddComponent<Image>().color = color;
        return go;
    }

    static void SetField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);
        if (field != null) field.SetValue(obj, value);
        else Debug.LogWarning($"[StoryUIBuilder] 필드 없음: {fieldName}");
    }
}
