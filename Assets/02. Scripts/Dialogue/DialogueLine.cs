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

    public bool MatchesAge(int age)
    {
        if (minAge >= 0 && age < minAge) return false;
        if (maxAge >= 0 && age > maxAge) return false;
        return true;
    }
}
