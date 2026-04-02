using System.Collections.Generic;

[System.Serializable]
public class StoryNode
{
    [UnityEngine.Tooltip("그래프 내 고유 ID")]
    public string Id = "start";

    [UnityEngine.Header("대사 목록 (위→아래 순서)")]
    public List<StoryLine> Lines = new();

    [UnityEngine.Tooltip("다음 노드 ID. 비어있으면 대화 종료")]
    public string NextNodeId = "";

    [UnityEngine.Tooltip("노드 끝난 뒤 씬 전환. 비어있으면 전환 없음")]
    public string LoadScene = "";
}
