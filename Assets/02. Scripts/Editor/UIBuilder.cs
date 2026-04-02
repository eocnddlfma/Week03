using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIBuilder : EditorWindow
{
    private const string PrefabDir = "Assets/03. Prefab";

    private TMP_FontAsset font;

    [MenuItem("Tools/UI/UI 프리팹 빌더")]
    static void Open() => GetWindow<UIBuilder>("UI Builder");

    void OnGUI()
    {
        GUILayout.Space(8);
        font = (TMP_FontAsset)EditorGUILayout.ObjectField("한글 폰트", font, typeof(TMP_FontAsset), false);
        GUILayout.Space(8);

        GUI.enabled = font != null;

        if (GUILayout.Button("DialogueUI 프리팹 생성", GUILayout.Height(36)))
            BuildDialogueUI();

        GUILayout.Space(4);

        if (GUILayout.Button("WaveTransitionUI 프리팹 생성", GUILayout.Height(36)))
            BuildWaveTransitionUI();

        GUILayout.Space(4);

        if (GUILayout.Button("BallCombatStatUI 프리팹 생성", GUILayout.Height(36)))
            BuildBallCombatStatUI();

        GUILayout.Space(4);

        if (GUILayout.Button("둘 다 생성 + 씬 배치", GUILayout.Height(44)))
        {
            BuildDialogueUI();
            BuildWaveTransitionUI();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        GUI.enabled = true;

        if (font == null)
            EditorGUILayout.HelpBox("폰트를 먼저 연결해주세요.", MessageType.Warning);
    }

    // ──────────────────────────────────────────────
    // DialogueUI
    // ──────────────────────────────────────────────
    private void BuildDialogueUI()
    {
        EnsurePrefabDir();

        // 루트
        var root = new GameObject("DialogueUI");
        var ui   = root.AddComponent<DialogueUI>();
        Undo.RegisterCreatedObjectUndo(root, "Create DialogueUI");

        // Canvas
        var canvasGo = new GameObject("DialogueCanvas");
        canvasGo.transform.SetParent(root.transform, false);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight  = 0.5f;

        // Panel (상단 고정)
        var panelRect = CreateRect("DialoguePanel", canvasGo.transform);
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot     = new Vector2(0.5f, 1f);
        panelRect.offsetMin = new Vector2(0f, -130f);
        panelRect.offsetMax = Vector2.zero;
        var panelImage = panelRect.gameObject.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.82f);

        // 이름 텍스트
        var nameRect = CreateRect("NameText", panelRect.transform);
        nameRect.anchorMin = new Vector2(0f, 0.72f);
        nameRect.anchorMax = Vector2.one;
        nameRect.offsetMin = new Vector2(16f, 0f);
        nameRect.offsetMax = new Vector2(-16f, -6f);
        var nameTmp = CreateTMP(nameRect.gameObject, 18f, Color.white, TextAlignmentOptions.BottomLeft);

        // 대사 텍스트
        var textRect = CreateRect("DialogueText", panelRect.transform);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = new Vector2(1f, 0.72f);
        textRect.offsetMin = new Vector2(16f, 6f);
        textRect.offsetMax = new Vector2(-16f, 0f);
        var dialogueTmp = CreateTMP(textRect.gameObject, 15f, Color.white, TextAlignmentOptions.TopLeft);

        // 참조 연결
        var so = new SerializedObject(ui);
        so.FindProperty("canvasRoot").objectReferenceValue  = canvasGo;
        so.FindProperty("panelImage").objectReferenceValue  = panelImage;
        so.FindProperty("nameText").objectReferenceValue    = nameTmp;
        so.FindProperty("dialogueText").objectReferenceValue = dialogueTmp;
        so.ApplyModifiedProperties();

        // 프리팹 저장 및 씬 배치
        SaveAndPlace(root, PrefabDir + "/DialogueUI.prefab");
    }

    // ──────────────────────────────────────────────
    // WaveTransitionUI
    // ──────────────────────────────────────────────
    private void BuildWaveTransitionUI()
    {
        EnsurePrefabDir();

        var root = new GameObject("WaveTransitionUI");
        var ui   = root.AddComponent<WaveTransitionUI>();
        Undo.RegisterCreatedObjectUndo(root, "Create WaveTransitionUI");

        // Canvas
        var canvasGo = new GameObject("WaveCanvas");
        canvasGo.transform.SetParent(root.transform, false);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 90;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight  = 0.5f;
        var cg = canvasGo.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;

        // 전체 배경
        var bgRect = CreateRect("BG", canvasGo.transform);
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = bgRect.offsetMax = Vector2.zero;
        bgRect.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.45f);

        // 중앙 레이블
        var labelRect = CreateRect("WaveLabel", canvasGo.transform);
        labelRect.anchorMin = new Vector2(0.2f, 0.35f);
        labelRect.anchorMax = new Vector2(0.8f, 0.65f);
        labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
        var labelTmp = CreateTMP(labelRect.gameObject, 72f, Color.white, TextAlignmentOptions.Center);
        labelTmp.fontStyle       = FontStyles.Bold;
        labelTmp.textWrappingMode = TextWrappingModes.NoWrap;

        // 참조 연결
        var so = new SerializedObject(ui);
        so.FindProperty("canvasGroup").objectReferenceValue = cg;
        so.FindProperty("label").objectReferenceValue       = labelTmp;
        so.ApplyModifiedProperties();

        SaveAndPlace(root, PrefabDir + "/WaveTransitionUI.prefab");
    }

    // ──────────────────────────────────────────────
    // BallCombatStatUI
    // ──────────────────────────────────────────────
    private void BuildBallCombatStatUI()
    {
        EnsurePrefabDir();

        var root = new GameObject("BallCombatStatUI");
        var ui   = root.AddComponent<BallCombatStatUI>();
        Undo.RegisterCreatedObjectUndo(root, "Create BallCombatStatUI");

        var atkTmp = CreateWorldTMP("ATKLabel", root.transform, 0.55f, new Color(1f, 0.6f, 0f));
        var hpTmp  = CreateWorldTMP("HPLabel",  root.transform, 0.55f, Color.green);
        var defTmp = CreateWorldTMP("DEFLabel", root.transform, 0.55f, new Color(0.4f, 0.8f, 1f));

        var so = new SerializedObject(ui);
        so.FindProperty("atkLabel").objectReferenceValue = atkTmp;
        so.FindProperty("hpLabel").objectReferenceValue  = hpTmp;
        so.FindProperty("defLabel").objectReferenceValue = defTmp;
        so.ApplyModifiedProperties();

        SaveAndPlace(root, PrefabDir + "/BallCombatStatUI.prefab");
    }

    // ──────────────────────────────────────────────
    // 헬퍼
    // ──────────────────────────────────────────────
    private static RectTransform CreateRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    private TextMeshProUGUI CreateTMP(GameObject go, float fontSize, Color color, TextAlignmentOptions alignment)
    {
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.font             = font;
        tmp.fontSize         = fontSize;
        tmp.color            = color;
        tmp.alignment        = alignment;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.overflowMode     = TextOverflowModes.Ellipsis;
        return tmp;
    }

    private static void SaveAndPlace(GameObject root, string path)
    {
        var prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(
            root, path, InteractionMode.AutomatedAction);

        EditorGUIUtility.PingObject(prefab);
        Debug.Log($"[UIBuilder] 저장됨: {path}");
    }

    private static void EnsurePrefabDir()
    {
        if (!AssetDatabase.IsValidFolder(PrefabDir))
            AssetDatabase.CreateFolder("Assets", "03. Prefab");
    }

    private TextMeshPro CreateWorldTMP(string name, Transform parent, float fontSize, Color color)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshPro>();
        tmp.font         = font;
        tmp.alignment    = TextAlignmentOptions.Center;
        tmp.fontSize     = fontSize;
        tmp.color        = color;
        tmp.sortingOrder = 10;
        tmp.rectTransform.sizeDelta = new Vector2(2f, 0.4f);
        return tmp;
    }
}
