using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    [TextArea(3, 8)]
    public string text;

    [Tooltip("최소 나이 (-1 = 제한 없음)")]
    public int minAge = -1;

    [Tooltip("최대 나이 (-1 = 제한 없음)")]
    public int maxAge = -1;

    [Tooltip("대사 표시 시 공 이름 대신 보여줄 이름 (비어있으면 공 기본 이름 사용)")]
    public string nameOverride;

    public bool MatchesAge(int age)
    {
        if (minAge >= 0 && age < minAge) return false;
        if (maxAge >= 0 && age > maxAge) return false;
        return true;
    }
}
