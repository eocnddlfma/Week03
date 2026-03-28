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
    // 샘플 대사 그룹
    // ──────────────────────────────────────────────
    private static List<DialogueGroup> BuildSampleGroups()
    {
        return new List<DialogueGroup>
        {
            // ── Any (기본 / 나이 조건 없음) ──────────────────────
            Group(ColorType.Any,
                Line("그 기억이... 아직도 남아있어요. 왜인지는 모르겠지만, 그냥 마음속에 박혀있어요."),
                Line("별로 특별한 날이 아니었는데, 그때가 자꾸 떠올라요. 기억이란 게 참 이상하죠."),
                Line("...그냥 평범했어요. 그게 다예요.")),

            // ── Red (분노) ────────────────────────────────────────
            Group(ColorType.Red,
                Line("그때 처음으로 진짜 화를 냈어요. 너무 불공평하다고 생각했거든요. 어린 마음에 온 세상이 나만 미워하는 것 같았어요.", 5, 10),
                Line("친구가 내 걸 망가뜨려 놓고 모른 척했어요. 그게 너무 화났는데 아무도 편을 안 들어줬어요. 억울해서 혼자 울었어요.", 5, 10),
                Line("선생님한테 혼났을 때 억울했어요. 내가 한 게 아닌데. 그때부터 뭔가 어른들을 쉽게 믿지 않게 된 것 같아요.", 11, 14),
                Line("친구들 앞에서 망신당한 기억이에요. 웃음거리가 됐는데 아무도 말려주지 않았어요. 그때의 분함이 지금도 생생해요.", 11, 14),
                Line("열심히 했는데 결과가 안 나왔을 때요. 아무도 노력을 인정 안 해줬어요. 그 화가 어디로 가야 할지를 몰랐어요.", 15, 20),
                Line("처음으로 '이건 아니다' 싶어서 맞섰어요. 결과는 좋지 않았지만, 그래도 그때 말을 해서 다행이라고 생각해요.", 15, 20)),

            // ── Blue (슬픔·외로움) ────────────────────────────────
            Group(ColorType.Blue,
                Line("친한 친구가 이사를 가던 날이에요. 연락하자고 했는데, 점점 멀어지더라고요. 그게 이별이라는 걸 그때 처음 알았어요.", 5, 10),
                Line("운동회 날 아무도 같이 뛰어주지 않았어요. 사람들이 많은데 혼자인 느낌. 그 운동장이 엄청 넓게 느껴졌어요.", 5, 10),
                Line("그 시기에는 교실에서도 혼자인 것 같았어요. 무리에 낄 수가 없었거든요. 점심을 어디서 먹어야 하나 고민하던 게 아직도 기억나요.", 11, 14),
                Line("아무한테도 하지 못한 고민이 있었어요. 말하면 이상하게 볼 것 같아서요. 혼자 삼켜야 했던 그 기억이 제일 외로워요.", 11, 14),
                Line("친했던 친구랑 멀어졌어요. 딱히 싸운 것도 아닌데, 어느 순간 서로 다른 방향을 보고 있었어요. 그게 더 씁쓸했어요.", 15, 20),
                Line("그때 처음으로 '아, 나는 혼자구나' 싶은 게 실감 났어요. 사람이 많아도 진짜 나를 아는 사람이 없는 것 같아서요.", 15, 20)),

            // ── Green (평온·안정) ─────────────────────────────────
            Group(ColorType.Green,
                Line("비 오는 날 처마 밑에서 빗소리 듣던 기억이에요. 아무것도 안 하고 그냥 앉아 있었는데, 그게 좋았어요.", 5, 10),
                Line("강아지랑 누워있던 오후예요. 따뜻하고 아무 걱정이 없었어요. 그 순간이 지금 생각해도 편안해요.", 5, 10),
                Line("좋아하는 걸 찾았을 때요. 책이든 그림이든 뭔가 하나 빠져들게 되면 시간이 어떻게 지나는지 몰랐어요. 그게 제일 편한 시간이었어요.", 11, 14),
                Line("혼자 산책하던 날이에요. 아무 생각 없이 그냥 걸었는데, 이상하게 머리가 맑아졌어요. 그때부터 혼자만의 시간이 필요하다는 걸 알았어요.", 11, 14),
                Line("결과에 집착 안 하고 그냥 한 적이 있어요. 그런데 그게 제일 잘 됐어요. 힘을 뺀다는 게 그런 건가 싶었죠.", 15, 20),
                Line("조용히 앉아서 좋아하는 음악 듣던 밤이에요. 아무도 없고 아무것도 안 해도 되는 시간. 그때가 제일 나다웠던 것 같아요.", 15, 20)),

            // ── Yellow (기쁨·행복) ────────────────────────────────
            Group(ColorType.Yellow,
                Line("그날이 제일 신났어요. 친구들이랑 종일 뛰어놀고, 밥도 같이 먹고. 해가 지는 것도 몰랐어요. 그때가 제일 좋았어요.", 5, 10),
                Line("생일에 케이크 초를 끄던 날이에요. 소원을 엄청 열심히 빌었는데 뭘 빌었는지는 기억 안 나요. 그냥 설레었어요.", 5, 10),
                Line("처음으로 뭔가를 잘해냈다는 말을 들었을 때요. 그 칭찬 하나로 일주일이 기분 좋았어요. 아직도 그 느낌이 기억나요.", 11, 14),
                Line("수학여행이 정말 좋았어요. 친구들이랑 밤새 얘기하고, 규칙도 조금씩 어기고. 그때 웃음이 지금도 떠올라요.", 11, 14),
                Line("시험 끝나고 친구들이랑 먹었던 떡볶이예요. 아무것도 안 중요하고, 그냥 같이 있는 게 좋았어요. 소소했는데 정말 행복했어요.", 15, 20),
                Line("뭔가를 해냈을 때요. 오래 연습했는데 드디어 됐을 때의 그 기분. 남한테 보여주고 싶어서 어쩔 줄 몰랐어요.", 15, 20)),

            // ── Cyan (설렘·기대) ─────────────────────────────────
            Group(ColorType.Cyan,
                Line("새 학년이 시작되던 날이에요. 새 교과서 냄새랑 새 친구들. 어떻게 될지 몰라서 두근두근했어요.", 5, 10),
                Line("처음 가보는 곳에 갔을 때예요. 뭐가 있을지 모르는데 자꾸 앞으로 걷고 싶었어요. 그 설레는 느낌이 좋았어요.", 5, 10),
                Line("좋아하는 사람이 생겼을 때요. 말 한마디에 하루 종일 의미를 찾고, 심장이 빨리 뛰고. 그 느낌이 이상하면서도 좋았어요.", 13, 17),
                Line("고등학교 처음 들어가던 날이에요. 다 낯선데 이상하게 기대됐어요. 앞으로 뭔가 달라질 것 같은 예감 같은 게 있었어요.", 15, 16),
                Line("뭔가를 처음 시작하던 날이에요. 잘 될지 모르겠지만, 해보고 싶다는 게 더 컸어요. 그 두근거림이 아직도 생생해요.", 15, 20)),

            // ── Magenta (불안·걱정) ───────────────────────────────
            Group(ColorType.Magenta,
                Line("발표 전날 밤에 잠을 못 잤어요. 틀리면 어쩌지, 친구들이 웃으면 어쩌지. 별별 생각이 다 들었어요.", 8, 13),
                Line("시험 결과가 나오는 날이면 항상 심장이 쿵쾅거렸어요. 잘 봤어도 불안하고, 못 봤을 것 같으면 더 불안하고.", 11, 14),
                Line("그 시기는 진짜 불안했어요. 내가 어떻게 보일지, 말이 이상하지 않은지, 계속 신경이 쓰였어요. 자연스럽게 있는 게 제일 어려웠어요.", 13, 17),
                Line("수능이 가까워지던 때요. 열심히 했는데도 자꾸 부족한 것 같은 느낌이 들었어요. 그 불안감은 지금도 꿈에 나와요.", 17, 20),
                Line("앞으로 어떻게 해야 할지 모르겠는 시간이 있었어요. 다들 뭔가를 향해 가는 것 같은데, 나만 멈춰있는 것 같아서요.", 18, 20)),

            // ── White (공허·무감각) ───────────────────────────────
            Group(ColorType.White,
                Line("아무것도 하기 싫었어요. 이유도 없이요. 그냥 다 무의미한 것 같았어요."),
                Line("그때 아무런 감정이 없었어요. 슬프지도 기쁘지도 않고. 그냥 텅 빈 것 같은 느낌이었어요.", 13, 20),
                Line("열심히 했는데 뭔가 남은 게 없는 것 같았어요. 의미가 있었겠죠? 그렇게 믿고 싶어요.", 16, 20)),

            // ── Gray (혐오·혐의) ─────────────────────────────────
            // 서사: 그 사람이 처음부터 이상했다 — 죄를 떠넘길 대상에 대한 혐오 기억
            Group(ColorType.Gray,
                Line("아직도 그게 떠올라요. 역겨웠거든요. 왜 사람이 그럴 수 있는지, 으읏.... 토 나오네요."),
                Line("처음부터 역겨웠어요. 몸에서는 이상한 냄새가 나지 얼굴도 기괴하게 생겼지, 가까이하고 싶지 않았는데, 진작 거리를 둬야했어요."),
                Line("그 자리에 있었던 게 후회돼요. 너무 끔찍한걸 목격했거든요. 아무리 잊으려 해도 잊어지지가 않아요."),
                Line("그 눈빛이 싫었어요. 항상 뭔가를 계산하는 것 같은 눈이요. 웃을 때마다 소름이 돋았어요. 그런 사람인 줄 알았어요, 처음부터."),
                Line("왜 나만 그 사람이 이상하다는 걸 알았을까요. 다들 좋다고 했는데. 내가 틀린 건지, 나만 진실을 본 건지. 지금도 모르겠어요."),
                Line("그 사람 생각만 하면 몸이 굳어요. 같은 공간에 있었다는 것만으로도 더러운 것 같아서 집에 와서 씻었어요. 그 기억은 아무리 씻어도 안 지워지더라고요.")),

            // ── Black (공포·목격) ────────────────────────────────
            // 서사: 눈 앞에서 일어난 죽음 — 도망치지도 막지도 못한 공포
            Group(ColorType.Black,
                Line("여전히 무서워요. 높은 곳에서 바닥을 바라볼 때면 금방이라도 떨어질 것만 같아서요. 물리적인 것도 그렇지만, 사회적으로도 나락으로 떨어질 것만 같아요. 마치 그 친구의 인생처럼..."),
                Line("그때 정말 무서웠어요. 앞이 아무것도 안 보이는 것 같았어요. 사람이 눈 앞에서 죽었는걸요. 그때 그 공포는 지금도 남아있어요. 지금도 손이 떨리네요... 스읍... 하... 심호흡을 하죠."),
                Line("처음으로 진짜 무서운 게 뭔지 알았어요. 귀신이 제일 무서운 줄 알았는데, 사람이 더 무서울 수도 있다는 걸 알았어요. 그것도 가까운 사람이.... 어떻게 그럴수가...."),
                Line("그 소리가 아직도 귀에 남아있어요. 쿵 소리요. 뛰어가지 말았어야 했는데, 뛰어가서 봤어요. 다 봤어요. 그래서 더 무서운 거예요."),
                Line("몸이 안 움직였어요. 소리도 못 질렀어요. 그냥 서있었어요. 그게 지금도 제일 이해가 안 돼요. 왜 그때 아무것도 못 했는지."),
                Line("눈을 감으면 그 표정이 떠올라요. 쓰러질 때 그 얼굴이요. 제일 무서운 건, 그 표정이 가끔 낯익다는 거예요.")),

            // ── DeepBlack (붕괴·부정·전가) ───────────────────────
            // 서사: 책임 부정 → 타인에게 전가 → 자책 → 공허한 자기 위로
            Group(ColorType.DeepBlack,
                Line("<color=red><b>아니야아니야아니야아니야아니야아니야아니야아니야아니야아니야내가아니야내가아니야내가아니야내가아니야내가아니야내가죽이지않았어내가죽이지않았어내가죽이지않았어내가죽이지않았어내가죽이지않았어그녀석이야그녀석이야그녀석이야그녀석이야그녀석이야니가죽인거야니가죽인거야니가죽인거야니가죽인거야니가죽인거야니가죽인거야니가죽인거야니가죽인거야니가죽인거야니가죽인거야</color>"),
                Line("<color=red><b>죄송해요죄송해요죄송해요죄송해요죄송해요죄송해요죄송해요죄송해요죄송해요죄송해요죄송해요잘못했어요잘못했어요잘못했어요잘못했어요잘못했어요잘못했어요잘못했어요잘못했어요잘못했어요잘못했어요잘못했어요잘못했어요잘못했어요잘못했어요잘못했어요잘못했어요잘못했어요잘못했어요다신안그럴게요다신안그럴게요다신안그럴게요다신안그럴게요다신안그럴게요다신안그럴게요다신안그럴게요다신안그럴게요다신안그럴게요</color>"),
                Line("<color=red><b>모든게 다 괜찮아질거야</color>"),
                Line("<color=red><b>나쁜건저사람이야나쁜건저사람이야나쁜건저사람이야나쁜건저사람이야나쁜건저사람이야내가뭘잘못했어내가뭘잘못했어내가뭘잘못했어내가뭘잘못했어왜나만잘못한거야왜나만잘못한거야왜나만잘못한거야왜나만잘못한거야억울해억울해억울해억울해억울해억울해</color>"),
                Line("...아무도 몰라. 나만 알고 있어. 그냥 묻어두면 되는 거야. 말 안 하면 없는 거야. 아무것도 안 일어난 거야. 그렇지? 그렇지.")),
        };
    }

    // ── 헬퍼 ──────────────────────────────────────────
    private static DialogueGroup Group(ColorType emotion, params DialogueLine[] lines)
    {
        var g = new DialogueGroup { emotion = emotion };
        g.lines.AddRange(lines);
        return g;
    }

    private static DialogueLine Line(string text, int minAge = -1, int maxAge = -1) =>
        new DialogueLine { text = text, minAge = minAge, maxAge = maxAge };

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
