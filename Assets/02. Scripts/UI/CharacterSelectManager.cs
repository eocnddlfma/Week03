using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CharacterSelectManager : MonoBehaviour
{
    public static CharacterSelectManager Instance { get; private set; }

    [Header("씬 설정")]
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("스탯 슬라이더")]
    [SerializeField] private Slider   shootPowerSlider;
    [SerializeField] private Slider   sustainTimeSlider;
    [SerializeField] private Slider   boundarySizeSlider;
    [SerializeField] private Slider   boundaryCornersSlider;

    [Header("스탯 값 텍스트")]
    [SerializeField] private TMP_Text shootPowerValue;
    [SerializeField] private TMP_Text sustainTimeValue;
    [SerializeField] private TMP_Text boundarySizeValue;
    [SerializeField] private TMP_Text boundaryCornersValue;

    [Header("특성 UI")]
    [SerializeField] private Transform     traitContainer;  // HorizontalLayoutGroup
    [SerializeField] private TMP_FontAsset badgeFont;

    [Header("툴팁")]
    [SerializeField] private RectTransform tooltipPanel;
    [SerializeField] private TMP_Text      tooltipNameText;
    [SerializeField] private TMP_Text      tooltipDescText;

    [Header("버튼")]
    [SerializeField] private Button rerollButton;
    [SerializeField] private Button startButton;

    // 특성 뱃지 색상
    private static readonly Color[] BadgeColors =
    {
        new Color(0.90f, 0.35f, 0.10f),
        new Color(0.20f, 0.55f, 0.90f),
        new Color(0.20f, 0.75f, 0.35f),
        new Color(0.70f, 0.20f, 0.85f),
        new Color(0.85f, 0.75f, 0.10f),
        new Color(0.85f, 0.25f, 0.45f),
        new Color(0.15f, 0.70f, 0.70f),
    };

    private PlayerRollStats  currentStats;
    private PlayerTrait[]    currentTraits;
    private readonly List<GameObject> badges = new();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        SetupSliderRanges();
        rerollButton.onClick.AddListener(Reroll);
        startButton .onClick.AddListener(StartGame);

        if (PlayerData.Instance == null)
            new GameObject("PlayerData").AddComponent<PlayerData>();

        tooltipPanel?.gameObject.SetActive(false);
        Reroll();
    }

    // ── 굴리기 ────────────────────────────────────────────────────
    public void Reroll()
    {
        currentStats  = PlayerRollStats.Roll();
        currentTraits = PlayerTraitInfo.RollMultiple();
        HideTraitTooltip();
        RefreshUI();
    }

    void RefreshUI()
    {
        // 슬라이더
        shootPowerSlider    .value = currentStats.ShootPower;
        sustainTimeSlider   .value = currentStats.SustainTime;
        boundarySizeSlider  .value = currentStats.BoundarySize;
        boundaryCornersSlider.value = currentStats.BoundaryCorners;

        // 값 텍스트
        shootPowerValue    .text = $"{currentStats.ShootPower:F1}x";
        sustainTimeValue   .text = $"{currentStats.SustainTime:F1}s";
        boundarySizeValue  .text = $"{currentStats.BoundarySize:F1}";
        boundaryCornersValue.text = $"{currentStats.BoundaryCorners}각";

        // 특성 뱃지 재생성
        foreach (var b in badges) Destroy(b);
        badges.Clear();

        if (currentTraits.Length == 0)
        {
            var none = CreateBadge("없음", new Color(0.4f, 0.4f, 0.4f), PlayerTrait.None);
            badges.Add(none);
        }
        else
        {
            for (int i = 0; i < currentTraits.Length; i++)
            {
                var t     = currentTraits[i];
                var color = BadgeColors[(int)t % BadgeColors.Length];
                badges.Add(CreateBadge(PlayerTraitInfo.GetName(t), color, t));
            }
        }
    }

    // ── 뱃지 생성 ─────────────────────────────────────────────────
    GameObject CreateBadge(string label, Color bgColor, PlayerTrait trait)
    {
        var go  = new GameObject(label, typeof(RectTransform));
        go.transform.SetParent(traitContainer, false);

        go.AddComponent<Image>().color = bgColor;

        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 42f;
        le.preferredWidth  = 180f;
        le.flexibleWidth   = 1f;

        var textGO = new GameObject("Label", typeof(RectTransform));
        textGO.transform.SetParent(go.transform, false);
        var rect = textGO.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;

        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 20f;
        tmp.color     = Color.white;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        if (badgeFont != null) tmp.font = badgeFont;

        if (trait != PlayerTrait.None)
        {
            var badge = go.AddComponent<TraitBadge>();
            badge.Trait = trait;
        }

        return go;
    }

    // ── 툴팁 ──────────────────────────────────────────────────────
    public void ShowTraitTooltip(PlayerTrait trait, RectTransform badge)
    {
        if (tooltipPanel == null) return;
        tooltipNameText.text = PlayerTraitInfo.GetName(trait);
        tooltipDescText.text = PlayerTraitInfo.GetDesc(trait);

        // 뱃지 바로 위에 위치
        var corners = new Vector3[4];
        badge.GetWorldCorners(corners);
        tooltipPanel.position = new Vector3(
            (corners[0].x + corners[3].x) * 0.5f,
            corners[1].y + tooltipPanel.rect.height * 0.5f + 8f,
            0f);

        tooltipPanel.gameObject.SetActive(true);
    }

    public void HideTraitTooltip()
        => tooltipPanel?.gameObject.SetActive(false);

    // ── 슬라이더 범위 ─────────────────────────────────────────────
    void SetupSliderRanges()
    {
        shootPowerSlider    .minValue = 0.75f; shootPowerSlider    .maxValue = 1.5f;
        sustainTimeSlider   .minValue = 0f;    sustainTimeSlider   .maxValue = 1f;
        boundarySizeSlider  .minValue = 5f;    boundarySizeSlider  .maxValue = 12f;
        boundaryCornersSlider.minValue = 3f;   boundaryCornersSlider.maxValue = 8f;
        boundaryCornersSlider.wholeNumbers = true;
    }

    // ── 게임 시작 ─────────────────────────────────────────────────
    void StartGame()
    {
        PlayerData.Instance.Stats  = currentStats;
        PlayerData.Instance.Traits = currentTraits;
        SceneManager.LoadScene(gameSceneName);
    }
}
