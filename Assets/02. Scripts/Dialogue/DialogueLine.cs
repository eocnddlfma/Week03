using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    [TextArea(3, 8)]
    public string text;

    [Tooltip("대사 표시 시 공 이름 대신 보여줄 이름 (비어있으면 그룹 고정 나이 이름 사용)")]
    public string nameOverride;
}
