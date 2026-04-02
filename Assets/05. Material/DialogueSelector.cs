using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DialogueSelector
{
    // 그룹(ColorType)별로 이미 보여준 라인 인덱스 추적 — 전부 순회하면 해당 그룹만 리셋
    private static readonly Dictionary<ColorType, HashSet<int>> _usedIndices = new();

    /// <summary>
    /// 1순위: 공의 감정과 일치하는 그룹에서 미출력 대사 랜덤 선택
    /// 2순위: Any 그룹에서 미출력 대사 랜덤 선택
    /// 반환: (text, displayName, emotionUsed)
    /// </summary>
    public static (string text, string displayName, ColorType emotionUsed) Select(DialogueDatabase db, BilliardBall ball)
    {
        if (db == null) return (string.Empty, null, ColorType.Any);

        ColorType emotion = ball.WaveStartEmotionType;

        var emotionGroup = db.groups.FirstOrDefault(g => g.emotion == emotion);
        if (emotionGroup != null && emotionGroup.lines.Count > 0)
        {
            var (text, nameOverride) = PickRandom(emotionGroup.lines, emotion);
            if (!string.IsNullOrEmpty(text))
            {
                string displayName = !string.IsNullOrEmpty(nameOverride) ? nameOverride
                    : emotionGroup.fixedAge >= 0 ? $"{emotionGroup.fixedAge}살의 기억" : null;
                return (text, displayName, emotion);
            }
        }

        var defaultGroup = db.groups.FirstOrDefault(g => g.emotion == ColorType.Any);
        if (defaultGroup != null && defaultGroup.lines.Count > 0)
        {
            var (text, nameOverride) = PickRandom(defaultGroup.lines, ColorType.Any);
            if (!string.IsNullOrEmpty(text))
                return (text, nameOverride, ColorType.Any);
        }

        return (string.Empty, null, ColorType.Any);
    }

    // 감정 그룹 무시하고 Any 그룹에서만 선택 (충돌 횟수 미달 시 호출)
    public static (string text, string displayName, ColorType emotionUsed) SelectAny(DialogueDatabase db)
    {
        if (db == null) return (string.Empty, null, ColorType.Any);

        var defaultGroup = db.groups.FirstOrDefault(g => g.emotion == ColorType.Any);
        if (defaultGroup == null || defaultGroup.lines.Count == 0) return (string.Empty, null, ColorType.Any);

        var (text, nameOverride) = PickRandom(defaultGroup.lines, ColorType.Any);
        return (text, nameOverride, ColorType.Any);
    }

    // 게임 재시작 시 호출
    public static void ResetAll() => _usedIndices.Clear();

    // 아직 안 나온 라인 중 랜덤 선택 — 전부 돌면 해당 그룹 리셋 후 다시 뽑기
    private static (string text, string nameOverride) PickRandom(List<DialogueLine> lines, ColorType groupKey)
    {
        if (!_usedIndices.TryGetValue(groupKey, out var used))
            _usedIndices[groupKey] = used = new HashSet<int>();

        var unshown = Enumerable.Range(0, lines.Count).Where(i => !used.Contains(i)).ToList();
        if (unshown.Count == 0)
        {
            used.Clear();
            unshown = Enumerable.Range(0, lines.Count).ToList();
        }

        int chosen = unshown[Random.Range(0, unshown.Count)];
        used.Add(chosen);
        var line = lines[chosen];
        return (line.text, line.nameOverride);
    }
}
