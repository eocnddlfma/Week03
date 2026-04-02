using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueGroup
{
    [Tooltip("Any = 기본 대사 (감정 조건 없음)")]
    public ColorType emotion = ColorType.Any;

    [Tooltip("이 감정 그룹의 고정 나이. -1이면 공의 이름 그대로 표시")]
    public int fixedAge = -1;

    public List<DialogueLine> lines = new List<DialogueLine>();
}

[CreateAssetMenu(fileName = "DialogueDatabase", menuName = "Game/Dialogue Database")]
public class DialogueDatabase : ScriptableObject
{
    [Tooltip("감정별 대사 그룹. Any 그룹은 조건 미충족 시 fallback으로 사용됨")]
    public List<DialogueGroup> groups = new List<DialogueGroup>();
}
