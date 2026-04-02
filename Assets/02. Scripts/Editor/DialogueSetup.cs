using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class DialogueSetup
{
    private const string DbPath = "Assets/00. Scriptable Object/DefaultDialogueDatabase.asset";

    [MenuItem("Tools/Dialogue/Setup Scene (GameObject + Database)")]
    public static void SetupAll()
    {
        var db = GetOrCreateDatabase();
        SetupSceneObjects(db);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[DialogueSetup] 완료! DialogueSystem·DialogueUI GameObject + Database 연결됨.");
    }

    [MenuItem("Tools/Dialogue/Create Database Only")]
    public static void CreateDatabaseOnly() => GetOrCreateDatabase();

    [MenuItem("Tools/Dialogue/Rebuild Database (Overwrite)")]
    public static void RebuildDatabase()
    {
        var existing = AssetDatabase.LoadAssetAtPath<DialogueDatabase>(DbPath);
        if (existing != null)
        {
            existing.groups = BuildSampleGroups();
            EditorUtility.SetDirty(existing);
            AssetDatabase.SaveAssets();
            int total = existing.groups.ConvertAll(g => g.lines.Count).Sum();
            Debug.Log($"[DialogueSetup] DB 재빌드 완료: {DbPath}  ({existing.groups.Count}개 그룹, 총 {total}개 대사)");
            Selection.activeObject = existing;
        }
        else
        {
            GetOrCreateDatabase();
        }
    }

    // ──────────────────────────────────────────────
    // Scene objects
    // ──────────────────────────────────────────────
    private static void SetupSceneObjects(DialogueDatabase db)
    {
        // DialogueSystem
        var dsGo = FindOrCreate("DialogueSystem");
        var ds   = GetOrAddComponent<DialogueSystem>(dsGo);
        var soProp = new SerializedObject(ds).FindProperty("database");
        soProp.objectReferenceValue = db;
        new SerializedObject(ds).ApplyModifiedProperties();

        // SerializedObject 방식이 private field라 직접 SerializedObject 갱신
        var so = new SerializedObject(ds);
        so.FindProperty("database").objectReferenceValue = db;
        so.ApplyModifiedProperties();

        // DialogueUI
        FindOrCreate("DialogueUI", typeof(DialogueUI));

        // WaveTransitionUI
        FindOrCreate("WaveTransitionUI", typeof(WaveTransitionUI));

        Debug.Log($"[DialogueSetup] Scene 오브젝트 세팅 완료. DB: {AssetDatabase.GetAssetPath(db)}");
    }

    // ──────────────────────────────────────────────
    // Database 생성/로드
    // ──────────────────────────────────────────────
    private static DialogueDatabase GetOrCreateDatabase()
    {
        var db = AssetDatabase.LoadAssetAtPath<DialogueDatabase>(DbPath);
        if (db != null)
        {
            Debug.Log($"[DialogueSetup] 기존 DB 사용: {DbPath}");
            return db;
        }

        db = ScriptableObject.CreateInstance<DialogueDatabase>();
        db.groups = BuildSampleGroups();

        // 디렉토리가 없으면 생성
        var dir = System.IO.Path.GetDirectoryName(DbPath);
        if (!AssetDatabase.IsValidFolder(dir))
            System.IO.Directory.CreateDirectory(dir);

        AssetDatabase.CreateAsset(db, DbPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = db;

        int total = db.groups.ConvertAll(g => g.lines.Count).Sum();
        Debug.Log($"[DialogueSetup] DB 생성됨: {DbPath}  ({db.groups.Count}개 그룹, 총 {total}개 대사)");
        return db;
    }

    // ──────────────────────────────────────────────
    // 대사 그룹 — 감정당 고정 나이, 여러 대사 중 랜덤 선택
    // ──────────────────────────────────────────────
    private static List<DialogueGroup> BuildSampleGroups()
    {
        return new List<DialogueGroup>
        {
            // ── Any (기본 / 충돌 횟수 미달 시 출력) ─────────────────
            Group(ColorType.Any, -1,
                Line("...잘 모르겠어요. 기억이 잘 안 나요."),
                Line("그냥 평범한 날이었어요. 특별한 건 없었어요."),
                Line("...별로 하고 싶은 말이 없어요."),
                Line("그게 언제였는지도 잘 모르겠어요. 흐릿해요."),
                Line("기억이 많이 없어요. 그냥 지나간 것 같아요.")),

            // ── Red (분노) — 10살 ─────────────────────────────────
            Group(ColorType.Red, 10,
                Line("엄마가 내 게임기를 버려서 놀이터로 나갔는데, 집에 들어가기 싫어서 그네에 앉아 어두워질 때까지 있었어요. 배가 고파서 결국 들어갔는데 아무 말도 안 하고 밥만 먹었어요."),
                Line("쉬는 시간에 옆자리 애가 먼저 때렸는데 선생님한테 나만 불려갔어요. 반성문 쓰는 동안 눈물이 나려고 했는데 참았어요. 지면 안 될 것 같아서요."),
                Line("방과 후에 보스까지 갔던 게임을 하고 있었는데 엄마가 전원 코드를 뽑았어요. 저장도 안 됐는데. 소리 지르려다가 맞을 것 같아서 방에 들어가서 베개만 발로 찼어요."),
                Line("뚜껑 있는 과자를 학교에 가져갔더니 반 애들이 다 먹었어요. 거절을 못 했어요. 빈 통 들고 집에 오면서 나한테 화가 났어요."),
                Line("성적표 뜯었더니 수학이 60점이었어요. 아빠한테 말했더니 '그러게 공부 좀 하지' 하고 넘어갔어요. 밥도 안 먹고 방에 들어갔어요.")),

            // ── Blue (슬픔·외로움) — 13살 ───────────────────────────
            Group(ColorType.Blue, 13,
                Line("같이 게임 하던 동네 형이 이사 갔어요. 마지막으로 보는지도 몰랐어요. 어느 날 방 불이 꺼진 거 창문으로 봤을 때 알았어요. 연락처도 없었어요."),
                Line("학교에서 제일 친했던 친구가 다른 그룹이랑 같이 밥 먹기 시작했어요. 뭐라고 해야 할지 몰라서 그냥 혼자 먹었어요. 한 달 넘게."),
                Line("100시간 넘게 키운 세이브 파일이 날아갔어요. 형이 컴퓨터 포맷했다고 했어요. 화도 안 났어요. 그냥 폴더만 한참 보다가 닫았어요."),
                Line("중간고사 망쳤는데 성적표 서랍에 숨겼어요. 혼자 이불 뒤집어쓰고 있었는데 이유 없이 눈물이 났어요. 왜 우는지 몰랐어요."),
                Line("발표 수업에서 틀린 답 했더니 애들이 웃었어요. 선생님도 그냥 넘어갔는데, 그 웃음소리가 집에 와서도 귀에 맴돌았어요.")),

            // ── Green (평온·안정) — 9살 ─────────────────────────────
            Group(ColorType.Green, 9,
                Line("여름방학에 혼자 방에서 선풍기 틀어놓고 게임 하다가 깜빡 잠들었어요. 엄마가 이불 덮어줬는데 반쯤 깬 채로 그냥 그대로 있었어요. 그게 편안했어요."),
                Line("비 오는 날 창문 보면서 게임 하는 게 좋았어요. 빗소리에 따뜻한 방, 엄마가 군고구마 갖다 주고 가셨어요. 아무것도 안 해도 되는 날이었어요."),
                Line("할머니 댁 마루에 앉아있으면 시간이 느리게 갔어요. 뭘 해야 한다는 것도 없고, 그냥 바람이 오고 가고, 밥 냄새 나고. 그게 다였어요."),
                Line("학교 끝나고 동네 애들이랑 골목에서 뭘 하는지도 모르게 놀다가 엄마 목소리 들리면 들어갔어요. 그 목소리 들릴 때가 하루 중 제일 아늑한 것 같았어요."),
                Line("도서관에서 게임 공략집 빌렸어요. 이불 속에서 손전등으로 읽다가 잠들었어요. 다음날 그대로 해봤더니 다 맞았어요. 그게 좋았어요.")),

            // ── Yellow (기쁨·행복) — 8살 ─────────────────────────────
            Group(ColorType.Yellow, 8,
                Line("생일에 게임기 새 거 받았어요. 박스 뜯는데 손이 떨렸어요. 그날 밤새 했는데 엄마가 아무 말도 안 하셨어요. 그게 제일 좋았어요."),
                Line("컴퓨터 수업 때 선생님이 어려운 거 물어봤는데 저만 손 들었어요. 앞에 나가서 다 하고 '잘했어요' 들었는데, 학교에서 처음으로 그런 말 들어봤어요."),
                Line("오락실에서 게임을 처음으로 클리어했어요. 하이스코어 내 이름 넣을 때 뒤에서 모르는 형들이 '오 잘한다' 했어요. 집에 걸어오면서 계속 웃었어요."),
                Line("운동회 달리기에서 1등 했어요. 연습 때는 맨날 꼴찌였는데. 결승선 넘고 멈추는데 다리가 후들거렸어요. 엄마가 보러 와 계셨어요."),
                Line("크리스마스 아침에 소파에 게임 카세트가 있었어요. 형한테 보여줬더니 '나도 갖고 싶었던 건데'라고 했어요. 그 말이 오히려 더 기분 좋았어요.")),

            // ── Cyan (설렘·기대) — 15살 ─────────────────────────────
            Group(ColorType.Cyan, 15,
                Line("좋아하는 애가 '내일 모둠 같이 해도 돼?'라고 문자 보냈어요. 답장 뭐라고 할지 30분 고민했어요. 그날 밤 잠을 못 잤어요."),
                Line("온라인 게임에서 처음 만난 사람이 '실력 좋은데 오길래'라고 했어요. 얼굴도 모르는 사람한테 인정받은 게 이상하게 기뻤어요."),
                Line("밴드부 연습 때 처음으로 리프가 박자 맞게 됐어요. 선배가 고개 끄덕이는 거 보고 얼굴이 달아올랐어요. 버스 안에서 계속 손가락으로 박자 쳤어요."),
                Line("학교 축제 때 좋아하는 애가 제 공연 보러 온다고 했어요. 기타 치는 내내 그 앤 어디 있나 찾았어요. 나중에 '멋있었어' 라고 했는데 그 말이 하루 종일 머릿속에 맴돌았어요."),
                Line("좋아하는 게임 시리즈 신작 나오는 날 새벽 6시에 일어났어요. 다운로드 기다리다가 엄마한테 아침 세 번 불렸어요. 시작 화면 뜨는데 가슴이 두근거렸어요.")),

            // ── Magenta (불안·걱정) — 19살 ───────────────────────────
            Group(ColorType.Magenta, 19,
                Line("6월 모의고사를 완전히 망쳤어요. 그날 밤부터 배가 아팠어요. 병원 갔더니 스트레스성 장염이래요. 수액 맞으면서 수능이 다섯 달 남은 거 생각했어요."),
                Line("수능 원서 학교 코드를 잘못 입력한 것 같아서 세 시간 동안 잠을 못 잤어요. 알고 보니 제대로 됐는데 그 세 시간 동안 진짜 심장이 멎을 것 같았어요."),
                Line("수시 면접 대기실에서 옆에 애가 스펙 얘기를 계속 했어요. 들으면 들을수록 내가 아무것도 없는 것 같았어요. 이름 불릴 때까지 눈 감고 있었어요."),
                Line("수능 전날 밤, 가족이 다 자고 나서 혼자 거실에 앉아있었어요. 손이 떨렸어요. 물 마시러 나온 엄마가 '괜찮아'라고 했는데 오히려 눈물이 났어요."),
                Line("진로 상담 때 선생님이 '넌 뭐가 하고 싶어?' 물어봤어요. 게임 관련 일을 하고 싶다고 할 수가 없었어요. '아직 모르겠어요'라고 하고 집에 와서 이불 속에 있었어요.")),

            // ── White (공허·무감각) — 16살 ───────────────────────────
            Group(ColorType.White, 16,
                Line("어느 날 게임도, 기타도, 유튜브도 아무것도 하기 싫었어요. 이유도 없이. 그냥 천장 보다가 어두워졌어요."),
                Line("밥 먹으면서 TV 보고 있었는데 뭘 보고 있는지 모르겠었어요. 엄마가 '뭐 봐?' 물어봤는데 대답을 못 했어요. 몰랐거든요."),
                Line("친구들이랑 있는데 웃고는 있었어요. 근데 속은 조용했어요. 유리창 안에 있는 것 같은 느낌이요. 뭔가 한 겹 떨어져 있는 것 같은."),
                Line("그냥 하루가 지나갔어요. 뭘 했는지 기억이 안 나요. 잤는지, 밥을 먹었는지. 일기를 쓰려고 앉았는데 쓸 게 없었어요."),
                Line("좋아하는 게임을 켰는데 재미가 없었어요. 껐다가 다시 켰는데 또 없었어요. 그냥 화면만 봤어요. 이상했어요.")),

            // ── Gray (혐오) — 20살 ───────────────────────────────────
            Group(ColorType.Gray, 20,
                Line("아직도 그게 떠올라요. 역겨웠거든요. 왜 사람이 그럴 수 있는지, 으읏.... 토 나오네요.", nameOverride: "20살 1월의 기억"),
                Line("처음부터 역겨웠어요. 몸에서는 이상한 냄새가 나지 얼굴도 기괴하게 생겼지, 가까이하고 싶지 않았는데, 진작 거리를 둬야했어요.", nameOverride: "20살 2월의 기억"),
                Line("그 자리에 있었던 게 후회돼요. 너무 끔찍한걸 목격했거든요. 아무리 잊으려 해도 잊어지지가 않아요.", nameOverride: "20살 3월의 기억"),
                Line("그 눈빛이 싫었어요. 항상 뭔가를 계산하는 것 같은 눈이요. 웃을 때마다 소름이 돋았어요. 그런 사람인 줄 알았어요, 처음부터.", nameOverride: "20살 4월의 기억"),
                Line("왜 나만 그 사람이 이상하다는 걸 알았을까요. 다들 좋다고 했는데. 내가 틀린 건지, 나만 진실을 본 건지. 지금도 모르겠어요.", nameOverride: "20살 5월의 기억")),

            // ── Black (공포) — 20살 ──────────────────────────────────
            Group(ColorType.Black, 20,
                Line("여전히 무서워요. 높은 곳에서 바닥을 바라볼 때면 금방이라도 떨어질 것만 같아서요. 물리적인 것도 그렇지만, 사회적으로도 나락으로 떨어질 것만 같아요. 마치 그 친구의 인생처럼...", nameOverride: "20살 6월의 기억"),
                Line("그때 정말 무서웠어요. 앞이 아무것도 안 보이는 것 같았어요. 사람이 눈 앞에서 죽었는걸요. 그때 그 공포는 지금도 남아있어요. 지금도 손이 떨리네요... 스읍... 하... 심호흡을 하죠.", nameOverride: "20살 7월의 기억"),
                Line("처음으로 진짜 무서운 게 뭔지 알았어요. 귀신이 제일 무서운 줄 알았는데, 사람이 더 무서울 수도 있다는 걸 알았어요. 그것도 가까운 사람이.... 어떻게 그럴수가....", nameOverride: "20살 8월의 기억"),
                Line("그 소리가 아직도 귀에 남아있어요. 쿵 소리요. 뛰어가지 말았어야 했는데, 뛰어가서 봤어요. 다 봤어요. 그래서 더 무서운 거예요.", nameOverride: "20살 9월의 기억"),
                Line("몸이 안 움직였어요. 소리도 못 질렀어요. 그냥 서있었어요. 그게 지금도 제일 이해가 안 돼요. 왜 그때 아무것도 못 했는지.", nameOverride: "20살 10월의 기억")),

            // ── DeepBlack (붕괴) — 20살 ──────────────────────────────
            Group(ColorType.DeepBlack, 20,
                Line("<color=red><b>아니야아니야아니야아니야아니야아니야아니야아니야아니야아니야내가아니야내가아니야내가아니야내가아니야내가아니야내가죽이지않았어내가죽이지않았어내가죽이지않았어내가죽이지않았어내가죽이지않았어그녀석이야그녀석이야그녀석이야그녀석이야그녀석이야니가죽인거야니가죽인거야니가죽인거야니가죽인거야니가죽인거야니가죽인거야니가죽인거야니가죽인거야니가죽인거야니가죽인거야</b></color>", nameOverride: "20살 11월의 기억"),
                Line("<color=red><b>죄송해요죄송해요죄송해요죄송해요죄송해요죄송해요죄송해요죄송해요죄송해요죄송해요죄송해요잘못했어요잘못했어요잘못했어요잘못했어요잘못했어요잘못했어요잘못했어요잘못했어요잘못했어요잘못했어요잘못했어요잘못했어요잘못했어요잘못했어요잘못했어요잘못했어요잘못했어요잘못했어요다신안그럴게요다신안그럴게요다신안그럴게요다신안그럴게요다신안그럴게요다신안그럴게요다신안그럴게요다신안그럴게요다신안그럴게요</b></color>", nameOverride: "20살 11월의 기억"),
                Line("<color=red><b>나쁜건저사람이야나쁜건저사람이야나쁜건저사람이야나쁜건저사람이야나쁜건저사람이야내가뭘잘못했어내가뭘잘못했어내가뭘잘못했어내가뭘잘못했어왜나만잘못한거야왜나만잘못한거야왜나만잘못한거야왜나만잘못한거야억울해억울해억울해억울해억울해억울해</b></color>", nameOverride: "20살 12월의 기억"),
                Line("<color=red><b>...아무도 몰라. 나만 알고 있어. 그냥 묻어두면 되는 거야. 말 안 하면 없는 거야. 아무것도 안 일어난 거야. 그렇지? 그렇지.</b></color>", nameOverride: "20살 12월의 기억"),
                Line("<color=red><b>모든게 다 괜찮아질거야</b></color>", nameOverride: "지금의 기억")),
        };
    }

    // ── 헬퍼 ──────────────────────────────────────────
    private static DialogueGroup Group(ColorType emotion, int fixedAge, params DialogueLine[] lines)
    {
        var g = new DialogueGroup { emotion = emotion, fixedAge = fixedAge };
        g.lines.AddRange(lines);
        return g;
    }

    private static DialogueLine Line(string text, string nameOverride = null) =>
        new DialogueLine { text = text, nameOverride = nameOverride };

    // ── 유틸 ──────────────────────────────────────────
    private static GameObject FindOrCreate(string name, params System.Type[] components)
    {
        var go = GameObject.Find(name);
        if (go == null)
        {
            go = new GameObject(name);
            foreach (var t in components)
                go.AddComponent(t);
            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
        }
        return go;
    }

    private static T GetOrAddComponent<T>(GameObject go) where T : Component
    {
        var c = go.GetComponent<T>();
        if (c == null) c = go.AddComponent<T>();
        return c;
    }
}
