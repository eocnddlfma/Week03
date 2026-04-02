using System.Collections.Generic;
using UnityEngine;

/// 대화 전체를 담는 ScriptableObject
/// 우클릭 → Create → Story → Dialogue Graph 로 생성
[CreateAssetMenu(menuName = "Story/Dialogue Graph", fileName = "NewStoryGraph")]
public class StoryGraph : ScriptableObject
{
    [Tooltip("처음 재생할 노드의 ID")]
    public string StartNodeId = "start";

    public List<StoryNode> Nodes = new();

    // ── 런타임 접근 ───────────────────────────────────────────────
    public StoryNode GetNode(string id)
        => Nodes.Find(n => n.Id == id);

    public StoryNode StartNode => GetNode(StartNodeId);

#if UNITY_EDITOR
    // 에디터에서 노드 추가 시 중복 ID 방지 헬퍼
    public string NextAvailableId()
    {
        int i = Nodes.Count;
        string id;
        do { id = $"node_{i++}"; } while (Nodes.Exists(n => n.Id == id));
        return id;
    }
#endif
}
