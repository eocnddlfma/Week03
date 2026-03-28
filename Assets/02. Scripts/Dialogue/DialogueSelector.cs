using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DialogueSelector
{
    // 그룹(ColorType)별로 이미 보여준 라인 인덱스 추적
    // 해당 그룹의 나이 조건 후보를 전부 순회하면 해당 그룹만 리셋
    private static readonly Dictionary<ColorType, HashSet<int>> _usedIndices = new();

    /// <summary>
    /// 1순위: 공의 감정과 일치하는 그룹에서 나이 조건 만족 + 미출력 대사
    /// 2순위: Any 그룹에서 나이 조건 만족 + 미출력 대사
    /// 3순위: Any 그룹 전체에서 미출력 대사 (나이 무시)
    /// </summary>
    public static string Select(DialogueDatabase db, BilliardBall ball)
    {
        if (db == null) return string.Empty;

        ColorType emotion = ball.WaveStartEmotionType;
        int       age     = ball.MemoryAge;

        // 감정 일치 그룹
        var emotionGroup = db.groups.FirstOrDefault(g => g.emotion == emotion);
        if (emotionGroup != null && emotionGroup.lines.Count > 0)
        {
            string result = PickUnshown(emotionGroup.lines, age, emotion);
            if (!string.IsNullOrEmpty(result)) return result;
        }

        // Any(기본) 그룹 — 나이 조건 있는 것 우선
        var defaultGroup = db.groups.FirstOrDefault(g => g.emotion == ColorType.Any);
        if (defaultGroup != null && defaultGroup.lines.Count > 0)
        {
            string result = PickUnshown(defaultGroup.lines, age, ColorType.Any);
            if (!string.IsNullOrEmpty(result)) return result;

            // 나이 조건 무시하고 미출력 대사
            result = PickUnshown(defaultGroup.lines, -1, ColorType.Any);
            if (!string.IsNullOrEmpty(result)) return result;
        }

        return string.Empty;
    }

    // 게임 재시작 시 호출
    public static void ResetAll() => _usedIndices.Clear();

    // ── 핵심: 나이 조건 만족 후보 중 아직 안 나온 것에서 뽑기 ──────
    // age == -1 이면 나이 조건 무시
    private static string PickUnshown(List<DialogueLine> lines, int age, ColorType groupKey)
    {
        // 나이 조건에 맞는 후보 인덱스 수집
        var candidates = new List<int>();
        for (int i = 0; i < lines.Count; i++)
            if (age < 0 || lines[i].MatchesAge(age))
                candidates.Add(i);

        if (candidates.Count == 0) return string.Empty;

        if (!_usedIndices.TryGetValue(groupKey, out var used))
            _usedIndices[groupKey] = used = new HashSet<int>();

        // 아직 안 나온 후보
        var unshown = candidates.Where(i => !used.Contains(i)).ToList();

        // 이 그룹의 나이 조건 후보를 전부 순회했으면 해당 그룹만 리셋
        if (unshown.Count == 0)
        {
            foreach (int i in candidates) used.Remove(i);
            unshown = candidates;
        }

        int chosen = unshown[Random.Range(0, unshown.Count)];
        used.Add(chosen);
        return lines[chosen].text;
    }
}
