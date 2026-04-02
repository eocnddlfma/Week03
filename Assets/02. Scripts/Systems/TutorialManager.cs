using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public enum TutorialEvent { Shot }

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text   stepLabel;
    [SerializeField] private TMP_Text   instructionText;
    [SerializeField] private Button     confirmButton;

    [Header("설정")]
    [SerializeField] private string    mainMenuScene   = "MainMenu";
    [SerializeField] private ColorType targetBallColor = ColorType.Blue; // 우울 기억
    [SerializeField] private BallFactory ballFactory; // BallFactory 참조 추가
    [SerializeField] private GameObject enemySpawnPoint; // 적 공이 생성될 위치 (빈 게임 오브젝트)


    float targetAttack = -1f; // -1 = 아직 미설정, 최초 탐색 시 현재값+2로 확정

    // ── 단계별 지시 텍스트 ─────────────────────────────────────────────
    readonly string[] StepTexts =
    {
        /* 0 */ "마우스를 본인의 기억조각에 올려보세요.\n보면 지금은 기억이 희미할거에요.",
        /* 1 */ "새총을 생각하면서 기억조각을 당겨 날려보세요.\n한 번에 하나만 날릴 수 있어요.",
        /* 2 */ "기억들은 충돌하면서 점점 명확해질거에요.\n혹시 가장 강렬하게 남은 기억은 뭔가요?",
        /* 3 */ "검고 어두운 공들이 보이나요?\n" +
                "어두운 색 계열의 공들은부정적인 기억이에요.\n" +
                "  · 아군이 적과 충돌하면 자동으로 전투가 벌어져요.\n" +
                "  · 속도가 빠를수록 한 번에 여러 번 공격해요.\n" +
                "  · 적의 HP가 0이 되면 처치 완료!\n" +
                "  · 아군도 피해를 받으니 충분히 성장시킨 후 맞서보세요.",
        /* 4 */ "",   // 공격력 목표 — 동적 텍스트
        /* 5 */ "3웨이브마다 기억이 얼마나 성장했는지 확인할 수 있어요.\n기억마다 특히 더 잘 성장하는 능력치가 있으니 잊지 마세요.\n이제 본격적으로 기억을 되살려보죠.",
    };

    int          currentStep = 0;
    bool         reportSeen  = false;
    BilliardBall targetBall;                              // 캐시 — 매 프레임 FindObjects 방지
    readonly System.Collections.Generic.HashSet<int> seenBallIds = new(); // 툴팁 확인한 공 ID

    // 튜토리얼을 위한 임시 적 공 참조
    private BilliardBall tutorialEnemyBall;

    // ── 생명주기 ──────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        confirmButton.onClick.AddListener(OnConfirm);
        PhaseReportUI.OnClosed += OnReportClosed;
        ShowStep(0);
    }

    void OnDestroy()
    {
        PhaseReportUI.OnClosed            -= OnReportClosed;
        DialogueSystem.OnDialogueFinished -= OnDialogueFinished;
    }

    void Update()
    {
        if (currentStep != 4) return;

        // 목표 공 캐시
        if (targetBall == null)
        {
            foreach (var b in FindObjectsByType<BilliardBall>(FindObjectsSortMode.None))
                if (!b.IsEnemy && b.Color.GetColorType() == targetBallColor)
                { targetBall = b; break; }
            if (targetBall == null) return;
        }

        instructionText.text = AttackGoalText();

        if (targetBall.Stats.Attack >= targetAttack)
            Advance();
    }

    // ── 외부 이벤트 수신 ──────────────────────────────────────────────
    public static void Notify(TutorialEvent e) => Instance?.Handle(e);

    void Handle(TutorialEvent e)
    {
        if (currentStep == 1 && e == TutorialEvent.Shot) Advance();
    }

    // 모든 공을 다 확인해야 1단계 완료
    public static void NotifyBallHovered(BilliardBall ball)
    {
        var t = Instance;
        if (t == null || t.currentStep != 0) return;
        if (!t.seenBallIds.Add(ball.Id)) return;   // 이미 확인한 공

        var all = FindObjectsByType<BilliardBall>(FindObjectsSortMode.None);
        int total = 0;
        foreach (var b in all) if (b != null && b.gameObject.activeSelf && !b.IsEnemy) total++; // 플레이어 공만 카운트

        t.instructionText.text = t.StepTexts[0] + $"\n확인: {t.seenBallIds.Count} / {total}";

        foreach (var b in all)
            if (b != null && b.gameObject.activeSelf && !b.IsEnemy && !t.seenBallIds.Contains(b.Id)) // 플레이어 공만 확인
                return;   // 아직 못 본 공 있음

        t.Advance();
    }

    // ── 단계 전환 ─────────────────────────────────────────────────────
    void Advance()
    {
        currentStep++;
        if (currentStep >= StepTexts.Length) { Complete(); return; }

        ShowStep(currentStep);

        if (currentStep == 2)
            DialogueSystem.OnDialogueFinished += OnDialogueFinished;
        
        // 튜토리얼 3단계: 적 공 생성
        if (currentStep == 3)
        {
            SpawnTutorialEnemy();
        }

        if (currentStep == 5 && reportSeen)
            Complete();
    }

    void ShowStep(int idx)
    {
        panel.SetActive(true);
        stepLabel.text       = $"튜토리얼  {idx + 1} / {StepTexts.Length}";
        instructionText.text = idx == 4 ? AttackGoalText() : StepTexts[idx];
        // 적 안내(3) 및 마지막 단계(5)에서 확인 버튼 표시
        confirmButton.gameObject.SetActive(idx == 3 || idx == StepTexts.Length - 1);
    }

    string AttackGoalText()
    {
        if (targetBall == null)
        {
            foreach (var b in FindObjectsByType<BilliardBall>(FindObjectsSortMode.None))
                if (!b.IsEnemy && b.Color.GetColorType() == targetBallColor)
                { targetBall = b; break; }
        }

        if (targetBall == null)
            return "우울한 기억을 찾는 중...\n아군 공끼리 충돌하면 스탯이 증가합니다.";

        float cur = targetBall.Stats.Attack;
        if (targetAttack <= 0f) targetAttack = cur + 2f; // 최초 1회만 목표 설정
        return $"우울한 기억의 공격력을 {targetAttack} 이상으로 올려보세요.\n" +
               $"아군 공끼리 충돌하면 스탯이 증가합니다.\n" +
               $"현재: {cur:F1} / {targetAttack:F0}";
    }

    // 튜토리얼용 적 공 생성 메서드
    private void SpawnTutorialEnemy()
    {
        if (ballFactory == null)
        {
            Debug.LogError("BallFactory 참조가 없습니다!");
            return;
        }
        if (enemySpawnPoint == null)
        {
            Debug.LogError("Enemy Spawn Point가 지정되지 않았습니다!");
            return;
        }
        ColorType enemyColor = ColorType.Gray;
        tutorialEnemyBall = ballFactory.CreateEnemy(enemyColor, enemySpawnPoint.transform.position);
        tutorialEnemyBall.SetEnemyEmotion(enemyColor);
        GamePhaseManager.Instance.AddBall(tutorialEnemyBall); // 생성된 적 공을 GamePhaseManager에 등록
    }


    // ── 이벤트 핸들러 ─────────────────────────────────────────────────
    void OnDialogueFinished()
    {
        DialogueSystem.OnDialogueFinished -= OnDialogueFinished;
        Advance();
    }

    void OnReportClosed()
    {
        if (currentStep >= 5) Complete();
        else                  reportSeen = true;
    }

    void OnConfirm()
    {
        // 적 안내 단계: 다음 단계로 이동 / 마지막 단계: 완료
        if (currentStep == StepTexts.Length - 1) Complete();
        else Advance();
    }

    // ── 완료 ─────────────────────────────────────────────────────────
    void Complete()
    {
        PhaseReportUI.OnClosed -= OnReportClosed;
        // 튜토리얼 완료 시 생성된 적 공 제거 (선택 사항)
        if (tutorialEnemyBall != null)
        {
            GamePhaseManager.Instance.RemoveBall(tutorialEnemyBall);
            Destroy(tutorialEnemyBall.gameObject);
        }
        panel.SetActive(false);
        SceneManager.LoadScene(mainMenuScene);
    }
}